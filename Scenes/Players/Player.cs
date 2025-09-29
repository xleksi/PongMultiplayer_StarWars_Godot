using Godot;

public partial class Player : RigidBody2D
{
    [Export]
    public float Speed = 500f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 movement = Vector2.Zero;

        if (Input.IsActionPressed("move_up"))
            movement = Vector2.Up;
        
        else if (Input.IsActionPressed("move_down"))
            movement = Vector2.Down;

        LinearVelocity = movement * Speed;
    }
}