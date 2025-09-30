using Godot;

public partial class Player : RigidBody2D
{
    [Export] public float Speed { get; set; } = 500f;
    [Export] public Vector2 GoalVelocity { get; set; } = Vector2.Zero;
    private MultiplayerSynchronizer? sync;

    public override void _Ready()
    {
        sync = GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        if (sync == null)
        {
            GD.PrintErr("MultiplayerSynchronizer node not found as a child of Player.");
            return;
        }

        CallDeferred(nameof(DeferredAssignAuthority));
    }

    private void DeferredAssignAuthority()
    {
        if (Multiplayer.MultiplayerPeer == null)
        {
            CallDeferred(nameof(DeferredAssignAuthority));
            return;
        }

        if (!long.TryParse(Name, out long parsedLong))
        {
            GD.PrintErr($"Player node Name '{Name}' could not be parsed to a long for multiplayer authority.");
            return;
        }

        if (parsedLong > int.MaxValue || parsedLong < int.MinValue)
        {
            GD.PrintErr($"Parsed peer id {parsedLong} is out of int range. Can't assign authority.");
            return;
        }

        sync.SetMultiplayerAuthority((int)parsedLong);
        GD.Print($"[Player] Assigned authority {(int)parsedLong} for node '{Name}'. Local id: {Multiplayer.GetUniqueId()}");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Multiplayer.MultiplayerPeer == null)
            return;

        if (sync != null)
        {
            long authority = sync.GetMultiplayerAuthority();
            if (authority != Multiplayer.GetUniqueId())
            {
                LinearVelocity = GoalVelocity;
                return;
            }
        }

        Vector2 movement = Vector2.Zero;
        if (Input.IsActionPressed("move_up"))
            movement = Vector2.Up;
        else if (Input.IsActionPressed("move_down"))
            movement = Vector2.Down;

        LinearVelocity = movement * Speed;
        GoalVelocity = LinearVelocity;
    }
}
