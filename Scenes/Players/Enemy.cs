using Godot;
using System;

public partial class Enemy : RigidBody2D
{
    [Export] public float Speed = 3500f;

    [Export] public Ball Ball;

    public override void _PhysicsProcess(double delta)
    {
        if (Ball == null)
            return;

        Vector2 direction = (Ball.Position - Position).Normalized();

        LinearVelocity = new Vector2(
            LinearVelocity.X,
            direction.Y * Speed * (float)delta
        );
    }
}
