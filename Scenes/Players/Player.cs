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
        
        if (int.TryParse(Name, out int parsedId))
            sync.SetMultiplayerAuthority(parsedId);
        else
            GD.PrintErr($"Player node Name '{Name}' could not be parsed to an integer for multiplayer authority.");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (sync != null)
        {
            if (sync.GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
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