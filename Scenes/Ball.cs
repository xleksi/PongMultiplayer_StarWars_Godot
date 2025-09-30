using Godot;
using System;

public partial class Ball : CharacterBody2D
{
    [Export] public float InitialBallSpeed = 20f;
    [Export] public float SpeedMultiplier = 1.02f;     // 102% faster each hit
    public float BallSpeed;
    [Export] public Vector2 GoalVelocity { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        BallSpeed = InitialBallSpeed;
        if (IsMultiplayerAuthority())
            StartBall();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Multiplayer.MultiplayerPeer == null)
            return;
        
        if (!IsMultiplayerAuthority())
        {
            Velocity = GoalVelocity;
            return;
        }

        var collision = MoveAndCollide(Velocity * BallSpeed * (float)delta);

        if (collision != null)
            Rpc(nameof(Bounce), collision.GetNormal());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Bounce(Vector2 normal)
    {
        Velocity = Velocity.Bounce(normal) * SpeedMultiplier;
        GoalVelocity = Velocity;
    }

    public void StartBall()
    {
        GD.Randomize();
        int xSign = (GD.Randi() % 2 == 0) ? -1 : 1;
        float ySign = (GD.Randi() % 2 == 0) ? -0.8f : 0.8f;

        Vector2 start = new Vector2(
            xSign * InitialBallSpeed,
            ySign * InitialBallSpeed
        );

        Rpc(nameof(StartBallRpc), start);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void StartBallRpc(Vector2 startVelocity)
    {
        Velocity = startVelocity;
        GoalVelocity = startVelocity;
        BallSpeed = InitialBallSpeed;
        GlobalPosition = Vector2.Zero;
    }
}