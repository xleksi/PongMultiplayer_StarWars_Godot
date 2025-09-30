using Godot;
using System;

public partial class Ball : CharacterBody2D
{
    [Export] public float InitialBallSpeed = 20f;
    [Export] public float SpeedMultiplier = 1.02f; // 102% each hit

    // How often the server (authority) sends state to clients
    private float syncInterval = 0.05f; // 50 ms
    private float syncTimer = 0f;

    public float BallSpeed;

    // Local "goal" velocity used by clients to apply motion when not authority
    [Export] public Vector2 GoalVelocity { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        BallSpeed = InitialBallSpeed;
        // Defer init until multiplayer peer exists to avoid "not active" errors
        CallDeferred(nameof(DeferredInit));
    }

    private void DeferredInit()
    {
        if (Multiplayer.MultiplayerPeer == null)
        {
            // try again later
            CallDeferred(nameof(DeferredInit));
            return;
        }

        // Only the authority (server) should generate/start the ball
        if (IsMultiplayerAuthority())
            ResetBall();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Wait until multiplayer is initialized
        if (Multiplayer.MultiplayerPeer == null)
            return;

        // Non-authority clients: we don't simulate collisions locally.
        // We follow the last known GoalVelocity (or the last SyncState)
        if (!IsMultiplayerAuthority())
        {
            // Keep CharacterBody2D.Velocity in sync with GoalVelocity for local engine movement
            Velocity = GoalVelocity;
            // Optionally move locally for visuals. We do not run collision logic here.
            MoveAndCollide(Velocity * BallSpeed * (float)delta);
            return;
        }

        // Authority (server) simulates physics
        // Move the ball according to current Velocity
        var collision = MoveAndCollide(Velocity * BallSpeed * (float)delta);

        if (collision != null)
        {
            // Bounce and speed up
            Velocity = Velocity.Bounce(collision.GetNormal()) * SpeedMultiplier;
            GoalVelocity = Velocity;

            // Immediately inform clients about bounce via unreliable sync
            Rpc(nameof(SyncState), GlobalPosition, Velocity);
        }
        else
        {
            // No collision this frame — update goal velocity so clients can use it
            GoalVelocity = Velocity;
            // Periodic sync to keep clients in sync (even if no collision)
            syncTimer += (float)delta;
            if (syncTimer >= syncInterval)
            {
                syncTimer = 0f;
                Rpc(nameof(SyncState), GlobalPosition, Velocity);
            }
        }
    }

    // Authority calls this to pick the random starting velocity and tell everyone
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

        // Inform everyone (including us because CallLocal = true) about the start
        Rpc(nameof(StartBallRpc), start);
        // Also do an immediate unreliable state sync so clients get pos+velocity fast:
        Rpc(nameof(SyncState), GlobalPosition, Velocity);
    }

    // Called on all peers (CallLocal = true) to set initial state
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void StartBallRpc(Vector2 startVelocity)
    {
        GlobalPosition = Vector2.Zero;
        Velocity = startVelocity;
        GoalVelocity = startVelocity;
        BallSpeed = InitialBallSpeed;
    }

    // Unreliable frequent sync that authority uses to update clients
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SyncState(Vector2 pos, Vector2 velocity)
    {
        GlobalPosition = pos;
        Velocity = velocity;
        GoalVelocity = velocity;
    }

    // Optional: helper that authority can call externally (e.g., when a point is scored)
    public void RestartFromServer()
    {
        if (IsMultiplayerAuthority())
            ResetBall();
    }
}
