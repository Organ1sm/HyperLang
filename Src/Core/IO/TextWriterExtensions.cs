﻿using System.CodeDom.Compiler;
using System.Runtime.InteropServices;

namespace Hyper.Core.IO;

internal static class TextWriterExtensions
{
    public static bool IsOutToConsole(this TextWriter writer)
    {
        if (writer == Console.Out)
            return true;

        return writer is IndentedTextWriter iw && iw.InnerWriter.IsOutToConsole();
    }

    public static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if (writer.IsOutToConsole())
            Console.ForegroundColor = color;
    }

    public static void ResetColor(this TextWriter writer)
    {
        if (writer.IsOutToConsole())
            Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter writer, string text)
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

    public static void WritePunctuation(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.White);
        writer.Write(text);
        writer.ResetColor();
    }
}