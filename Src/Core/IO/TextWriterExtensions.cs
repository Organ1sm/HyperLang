using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using Hyper.Core.Syntax;

namespace Hyper.Core.IO;

internal static class TextWriterExtensions
{
    private static bool IsOutToConsole(this TextWriter writer)
    {
        if (writer == Console.Out)
            return true;

        return writer is IndentedTextWriter iw && iw.InnerWriter.IsOutToConsole();
    }

    private static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if (writer.IsOutToConsole())
            Console.ForegroundColor = color;
    }

    private static void ResetColor(this TextWriter writer)
    {
        if (writer.IsOutToConsole())
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
}
