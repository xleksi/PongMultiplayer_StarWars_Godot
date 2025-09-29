using Godot;
using System;

public partial class Edge : Area2D
{
    [Signal]
    public delegate void PointScoredEventHandler();
    private void OnBodyEntered(Node body)
    {
        if (body is Ball)
            EmitSignalPointScored();
    }
}
