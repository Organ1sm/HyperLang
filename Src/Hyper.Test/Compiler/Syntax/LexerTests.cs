﻿using Hyper.Core.Syntax;
using Hyper.Core.Text;
using Xunit;

namespace Hyper.Test.Compiler.Syntax;

public class LexerTests
{
    [Fact]
    public void LexerTestsAllTokens()
    {
        var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                             .Cast<SyntaxKind>()
                             .Where(k => k != SyntaxKind.SingleLineCommentTrivia &&
                                         k != SyntaxKind.MultiLineCommentTrivia)
                             .Where(k => k.IsToken());
        var testedTokenKinds = GetTokens().Concat(GetSeparators()).Select(t => t.kind);

        var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
        untestedTokenKinds.Remove(SyntaxKind.BadTokenTrivia);
        untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
        untestedTokenKinds.ExceptWith(testedTokenKinds);

        Assert.Empty(untestedTokenKinds);
    }

    [Fact]
    public void LexerLexUnterminatedString()
    {
        var text   = "\"text";
        var tokens = AST.ParseTokens(text, out var diagnostics);

        var token = Assert.Single(tokens);
        Assert.Equal(SyntaxKind.StringToken, token.Kind);
        Assert.Equal(text, token.Text);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
        Assert.Equal("Unterminated string literal.", diagnostic.Message);
    }

    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void LexerLexToken(SyntaxKind kind, string text)
    {
        var tokens = AST.ParseTokens(text);
        var token  = Assert.Single(tokens);

        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsData))]
    public void LexerLexTokenPairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
    {
        // TODO: interim solution
        if (t1Text[0] == '-' && t2Text[0] == '>') return;
        var text   = t1Text + t2Text;
        var tokens = AST.ParseTokens(text).ToArray();

        Assert.Equal(2, tokens.Length);
        Assert.Equal(tokens[0].Kind, t1Kind);
        Assert.Equal(tokens[0].Text, t1Text);
        Assert.Equal(tokens[1].Kind, t2Kind);
        Assert.Equal(tokens[1].Text, t2Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsWithSeparatorData))]
    public void LexerLexTokenPairsWithSeparators(SyntaxKind t1Kind,
                                                 string t1Text,
                                                 SyntaxKind separatorKind,
                                                 string separatorText,
                                                 SyntaxKind t2Kind,
                                                 string t2Text)
    {
        var text   = t1Text + separatorText + t2Text;
        var tokens = AST.ParseTokens(text).ToArray();

        Assert.Equal(3, tokens.Length);
        Assert.Equal(tokens[0].Kind, t1Kind);
        Assert.Equal(tokens[0].Text, t1Text);
        Assert.Equal(tokens[1].Kind, separatorKind);
        Assert.Equal(tokens[1].Text, separatorText);
        Assert.Equal(tokens[2].Kind, t2Kind);
        Assert.Equal(tokens[2].Text, t2Text);
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("foo42")]
    [InlineData("foo_42")]
    [InlineData("_foo")]
    public void LexerLexIdentifiers(string name)
    {
        var tokens = AST.ParseTokens(name).ToArray();

        Assert.Single(tokens);

        var token = tokens[0];
        Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
        Assert.Equal(name, token.Text);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        return GetTokens().Concat(GetSeparators()).Select(t => new object[] {t.kind, t.text});
    }

    public static IEnumerable<object[]> GetTokenPairsData()
    {
        return GetTokenPairs().Select(t => new object[] {t.t1Kind, t.t1Text, t.t2Kind, t.t2Text});
    }

    private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
    {
        foreach (var t1 in GetTokens())
        {
            foreach (var t2 in GetTokens())
            {
                if (!RequiresSeparator(t1.kind, t2.kind))
                    yield return (t1.kind, t1.text, t2.kind, t2.text);
            }
        }
    }

    public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
    {
        return GetTokenPairsWithSeparator()
            .Select(t => new object[]
            {
                t.t1Kind, t.t1Text, t.separatorKind, t.separatorText, t.t2Kind, t.t2Text
            });
    }

    private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
    {
        var t1IsKeyword = t1Kind.IsKeyword();
        var t2IsKeyword = t2Kind.IsKeyword();

        if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
            return true;

        if (t1IsKeyword && t2IsKeyword)
            return true;

        if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
            return true;

        if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
            return true;

        if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.NumberToken)
            return true;

        if (t1IsKeyword && t2Kind == SyntaxKind.NumberToken)
            return true;

        if (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken)
            return true;

        if (t1Kind == SyntaxKind.StringToken && t2Kind == SyntaxKind.StringToken)
            return true;

        if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken)
            return true;

        if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            return true;

        if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
            return true;

        if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            return true;

        if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsToken)
            return true;

        if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            return true;

        if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsToken)
            return true;

        if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsEqualsToken)
            return true;

        if (t1Kind == SyntaxKind.AmpersandToken && t2Kind == SyntaxKind.AmpersandToken)
            return true;

        if (t1Kind == SyntaxKind.AmpersandToken && t2Kind == SyntaxKind.AmpersandAmpersandToken)
            return true;

        if (t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipeToken)
            return true;

        if (t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipePipeToken)
            return true;

        if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.SlashToken)
            return true;

        if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.StarToken)
            return true;

        if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.SingleLineCommentTrivia)
            return true;

        if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.MultiLineCommentTrivia)
            return true;

        return false;
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        var fixedTokens = Enum.GetValues(typeof(SyntaxKind))
                              .Cast<SyntaxKind>()
                              .Select(k => (SyntaxKind: k, text: Factors.GetText(k)))
                              .Where(t => t.text != null);

        var dynamicTokens = new[]
        {
            (SyntaxKind.PlusToken, "+"),
            (SyntaxKind.NumberToken, "1"),
            (SyntaxKind.NumberToken, "123"),
            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc"),
            (SyntaxKind.StringToken, "\"Test\""),
            (SyntaxKind.StringToken, "\"Te\"\"st\""),
        };

        return fixedTokens.Concat(dynamicTokens);
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
    {
        return new[]
        {
            (SyntaxKind.WhitespaceTrivia, " "),
            (SyntaxKind.WhitespaceTrivia, "  "),
            (SyntaxKind.WhitespaceTrivia, "\r"),
            (SyntaxKind.WhitespaceTrivia, "\n"),
            (SyntaxKind.WhitespaceTrivia, "\r\n"),
            (SyntaxKind.MultiLineCommentTrivia, "/**/"),
        };
    }

    private static IEnumerable<(SyntaxKind t1Kind, string t1Text,
        SyntaxKind separatorKind, string separatorText,
        SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
    {
        foreach (var t1 in GetTokens())
        {
            foreach (var t2 in GetTokens())
            {
                if (RequiresSeparator(t1.kind, t2.kind))
                {
                    foreach (var s in GetSeparators())
                    {
                        if (!RequiresSeparator(t1.kind, s.kind) && !RequiresSeparator(s.kind, t2.kind))
                            yield return (t1.kind, t1.text, s.kind, s.text, t2.kind, t2.text);
                    }
                }
            }
        }
    }
}
