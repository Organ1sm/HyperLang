namespace Hyper.Core.Text;

public struct TextSpan
{
    public TextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public static TextSpan MakeTextSpanFromBound(int start, int end)
    {
        var length = end - start;
        return new TextSpan(start, length);
    }

    public int Start  { get; }
    public int Length { get; }
    public int End    => Start + Length;

    public override string ToString() => $"{Start}...{End}";
}
