﻿using System.CodeDom.Compiler;
using Hyper.Core.Syntax;
using Hyper.Core.Text;

namespace Hyper.Core.IO;

public static class TextWriterExtensions
{
    private static bool IsConsole(this TextWriter writer)
    {
        if (writer == Console.Out)
            return !Console.IsOutputRedirected;

        if (writer == Console.Error)
            return !Console.IsErrorRedirected &&
                   !Console.IsOutputRedirected; // Color codes are always output to Console.Out

        return writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole();
    }

    private static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if (writer.IsConsole())
            Console.ForegroundColor = color;
    }

    private static void ResetColor(this TextWriter writer)
    {
        if (writer.IsConsole())
            Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter writer, SyntaxKind kind) =>
        writer.WriteKeyword(Factors.GetText(kind));

    public static void WriteKeyword(this TextWriter writer, string? text)
    {
        writer.SetForeground(ConsoleColor.Blue);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteIdentifier(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkYellow);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteNumber(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Cyan);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteString(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Magenta);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteSpace(this TextWriter writer) => writer.WritePunctuation(" ");

    public static void WritePunctuation(this TextWriter writer, SyntaxKind kind) =>
        writer.WritePunctuation(Factors.GetText(kind));

    public static void WritePunctuation(this TextWriter writer, string? text)
    {
        writer.SetForeground(ConsoleColor.White);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteDiagnostics(this TextWriter writer,
                                        IEnumerable<Diagnostic.Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics.Where(d => d.Location.Text == null))
        {
            writer.SetForeground(ConsoleColor.DarkRed);
            writer.WriteLine(diagnostic.Message);
            writer.ResetColor();
        }

        foreach (var diagnostic in diagnostics.Where(d => d.Location.Text != null)
                                              .OrderBy(d => d.Location.FileName)
                                              .ThenBy(d => d.Location.Span.Start)
                                              .ThenBy(d => d.Location.Span.Length))
        {
            var text           = diagnostic.Location.Text;
            var fileName       = diagnostic.Location.FileName;
            var startLine      = diagnostic.Location.StartLine + 1;
            var startCharacter = diagnostic.Location.StartCharacter + 1;
            var endLine        = diagnostic.Location.EndLine + 1;
            var endCharacter   = diagnostic.Location.EndCharacter + 1;

            var span      = diagnostic.Location.Span;
            var lineIndex = text.GetLineIndex(span.Start);
            var line      = text.Lines[lineIndex];

            writer.WriteLine();

            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
            writer.WriteLine(diagnostic);
            writer.ResetColor();

            var prefixSpan = TextSpan.MakeTextSpanFromBound(line.Start, span.Start);
            var suffixSpan = TextSpan.MakeTextSpanFromBound(span.End, line.End);

            var prefix = text.ToString(prefixSpan);
            var error  = text.ToString(span);
            var suffix = text.ToString(suffixSpan);

            writer.Write("    ");
            writer.Write(prefix);

            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write(error);
            writer.ResetColor();

            writer.Write(suffix);

            writer.WriteLine();
        }

        writer.WriteLine();
    }
}
