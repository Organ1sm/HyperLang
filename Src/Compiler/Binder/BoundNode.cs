﻿namespace Hyper.Compiler.Binding;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
}
