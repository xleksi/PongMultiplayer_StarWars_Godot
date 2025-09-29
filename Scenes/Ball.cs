using Godot;
using System;

public partial class Ball : CharacterBody2D
{
    [Export] public float InitialBallSpeed = 20f;
    [Export] public float SpeedMultiplier = 1.02f;     //102% faster each hit

    public float BallSpeed;

    public override void _Ready()
    {
        BallSpeed = InitialBallSpeed;
        StartBall();
    }

    public override void _PhysicsProcess(double delta)
    {
        var collision = MoveAndCollide(Velocity * BallSpeed * (float)delta);

        if (collision != null)
            Velocity = Velocity.Bounce(collision.GetNormal()) * SpeedMultiplier;
    }

    public void StartBall()
    {
        GD.Randomize();
        
        //Randomize X
        int xSign = (GD.Randi() % 2 == 0) ? -1 : 1;
        //Randomize Y
        float ySign = (GD.Randi() % 2 == 0) ? -0.8f : 0.8f;

        Velocity = new Vector2(
            xSign * InitialBallSpeed,
            ySign * InitialBallSpeed
        );
    }
}
