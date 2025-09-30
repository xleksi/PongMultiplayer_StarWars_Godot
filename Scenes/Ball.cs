using Godot;
using System;
using System.Linq;

public partial class Ball : CharacterBody2D
{
    [Export] public float InitialBallSpeed = 20f;
    [Export] public float SpeedMultiplier = 1.02f; // 102% speed increase per hit

    private float syncInterval = 0.05f; // 50 ms
    private float syncTimer = 0f;

    public float BallSpeed;
    private Sprite2D? ballSprite;
    private bool lastFlip = false;

    [Export] public Vector2 GoalVelocity { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        BallSpeed = InitialBallSpeed;

        ballSprite = GetNodeOrNull<Sprite2D>("Sprite2D")
                     ?? GetNodeOrNull<Sprite2D>("Sprite")
                     ?? GetChildren().OfType<Sprite2D>().FirstOrDefault();

        CallDeferred(nameof(DeferredInit));
    }

    private void DeferredInit()
    {
        if (Multiplayer.MultiplayerPeer == null)
        {
            CallDeferred(nameof(DeferredInit));
            return;
        }

        if (IsMultiplayerAuthority())
            ResetBall();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Multiplayer.MultiplayerPeer == null)
            return;

        if (!IsMultiplayerAuthority())
        {
            // Just follow server values
            Velocity = GoalVelocity;
            MoveAndCollide(Velocity * BallSpeed * (float)delta);
            return;
        }

        // Authority moves the ball
        var collision = MoveAndCollide(Velocity * BallSpeed * (float)delta);

        if (collision != null)
        {
            Velocity = Velocity.Bounce(collision.GetNormal()) * SpeedMultiplier;
            GoalVelocity = Velocity;

            bool flipNow = lastFlip; // default = keep previous value
            var colliderObj = collision.GetCollider();
            if (colliderObj is Node colliderNode)
            {
                // Flip only if collider is paddle
                if (colliderNode.IsInGroup("Yoda"))
                    flipNow = true;
                else if (colliderNode.IsInGroup("GeneralGrievous"))
                    flipNow = false;
            }

            if (ballSprite != null)
            {
                ballSprite.FlipH = flipNow;
                lastFlip = flipNow;
            }

            // Sync after collision
            Rpc(nameof(SyncState), GlobalPosition, Velocity, lastFlip);
        }
        else
        {
            GoalVelocity = Velocity;
            syncTimer += (float)delta;
            if (syncTimer >= syncInterval)
            {
                syncTimer = 0f;
                Rpc(nameof(SyncState), GlobalPosition, Velocity, lastFlip);
            }
        }
    }

    public void ResetBall()
    {
        if (!IsMultiplayerAuthority())
            return;

        GD.Randomize();
        int xSign = (GD.Randi() % 2 == 0) ? -1 : 1;
        float ySign = (GD.Randi() % 2 == 0) ? -0.8f : 0.8f;

        Vector2 start = new Vector2(
            xSign * InitialBallSpeed,
            ySign * InitialBallSpeed
        );

        BallSpeed = InitialBallSpeed;
        Velocity = start;
        GoalVelocity = start;
        GlobalPosition = Vector2.Zero;

        Rpc(nameof(StartBallRpc), start);
        Rpc(nameof(SyncState), GlobalPosition, Velocity, lastFlip);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void StartBallRpc(Vector2 startVelocity)
    {
        GlobalPosition = Vector2.Zero;
        Velocity = startVelocity;
        GoalVelocity = startVelocity;
        BallSpeed = InitialBallSpeed;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SyncState(Vector2 pos, Vector2 velocity, bool flipH)
    {
        GlobalPosition = pos;
        Velocity = velocity;
        GoalVelocity = velocity;

        if (ballSprite != null)
        {
            ballSprite.FlipH = flipH;
            lastFlip = flipH;
        }
        else
        {
            ballSprite = GetNodeOrNull<Sprite2D>("Sprite2D")
                         ?? GetNodeOrNull<Sprite2D>("Sprite")
                         ?? GetChildren().OfType<Sprite2D>().FirstOrDefault();
            if (ballSprite != null)
            {
                ballSprite.FlipH = flipH;
                lastFlip = flipH;
            }
        }
    }

    public void RestartFromServer()
    {
        if (IsMultiplayerAuthority())
            ResetBall();
    }
}
