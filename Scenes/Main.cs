using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class Main : Node2D
{
    private int playerPoints = 0;
    private int enemyPoints = 0;

    private RigidBody2D Player;
    private RigidBody2D Enemy;
    private Ball ball;
    private UI ui;
    private AudioStreamPlayer YodaAudio;
    private AudioStreamPlayer GeneralGrievousAudio;

    public override void _Ready()
    {
        Player =GetNode<RigidBody2D>("Yoda");
        Enemy = GetNode<RigidBody2D>("GeneralGrievous");
        ball = GetNode<Ball>("Ball");
        ui = GetNode<UI>("UI");
        YodaAudio = GetNode<AudioStreamPlayer>("AudioYoda");
        GeneralGrievousAudio = GetNode<AudioStreamPlayer>("AudioGeneralGrievous");
    }

    private void OnEnemyScored()
    {
        enemyPoints += 1;
        ui.UpdateEnemyPoints(enemyPoints);
        GD.Print(enemyPoints);
        ResetGameState();
        GeneralGrievousAudio.Play();   
    }
    
    private void OnPlayerScored()
    {
        playerPoints += 1;
        ui.UpdatePlayerPoints(playerPoints);
        GD.Print(playerPoints);
        ResetGameState();
        YodaAudio.Play();
    }

    public void ResetGameState()
    {
        ball.Position = Vector2.Zero;
        //Player.GlobalPosition = new Vector2(Player.GlobalPosition.X, 0);
        //Enemy.GlobalPosition = new Vector2(Player.GlobalPosition.X, 0);

        ball.Velocity = Vector2.Zero;
        //Enemy.LinearVelocity = Vector2.Zero;
        //Player.LinearVelocity = Vector2.Zero;

        ball.StartBall();
    }
}
