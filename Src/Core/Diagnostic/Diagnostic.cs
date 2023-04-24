using Hyper.Core.Text;

namespace Hyper.Core.Diagnostic;

public sealed class Diagnostic
{
    public Diagnostic(TextLocation location, string message)
    {
        Location = location;
        Message = message;
    }

    public TextLocation Location { get; }
    public string       Message  { get; }

    public override string ToString() => Message;
}
