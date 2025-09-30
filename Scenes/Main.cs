using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class Main : Node2D
{
    [Export] public int PlayerPoints { get; set; } = 0;
    [Export] public int EnemyPoints { get; set; }= 0;
    [Export] public PackedScene YodaScene { get; set; } = GD.Load<PackedScene>("res://Scenes/Players/yoda.tscn");
    [Export] public PackedScene GeneralGrievousScene { get; set; } = GD.Load<PackedScene>("res://Scenes/Players/GeneralGrievous.tscn");
    
    //private RigidBody2D Player;
    //private RigidBody2D Enemy;
    private Ball ball;
    private UI ui;
    private AudioStreamPlayer YodaAudio;
    private AudioStreamPlayer GeneralGrievousAudio;

    public override void _Ready()
    {
        //Player =GetNode<RigidBody2D>("Yoda");
        //Enemy = GetNode<RigidBody2D>("GeneralGrievous");
        ball = GetNode<Ball>("Ball");
        ui = GetNode<UI>("UI");
        YodaAudio = GetNode<AudioStreamPlayer>("AudioYoda");
        GeneralGrievousAudio = GetNode<AudioStreamPlayer>("AudioGeneralGrievous");
        
        YodaScene = GD.Load<PackedScene>("res://Scenes/Players/yoda.tscn");
        GeneralGrievousScene = GD.Load<PackedScene>("res://Scenes/Players/GeneralGrievous.tscn");

        GD.Print("GameManager.Players contents: " + GameManager.Players);

        int index = 0;
        foreach (var key in GameManager.Players.Keys)
        {
            long id = Convert.ToInt64(key);

            var playerDict = (Godot.Collections.Dictionary)GameManager.Players[key];
            var currentPlayer = (Node2D)YodaScene.Instantiate();

            currentPlayer.Name = playerDict["id"].ToString();
            AddChild(currentPlayer);

            foreach (Node spawn in GetTree().GetNodesInGroup("PlayerSpawn"))
            {
                if (spawn.Name == index.ToString() && spawn is Node2D spawnNode)
                    currentPlayer.GlobalPosition = spawnNode.GlobalPosition; 
                break;
            }
        }
        index++;
    }

    private void OnEnemyScored()
    {
        EnemyPoints += 1;
        ui.UpdateEnemyPoints(EnemyPoints);
        GD.Print(EnemyPoints);
        ResetGameState();
        GeneralGrievousAudio.Play();   
    }
    
    private void OnPlayerScored()
    {
        PlayerPoints += 1;
        ui.UpdatePlayerPoints(PlayerPoints);
        GD.Print(PlayerPoints);
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
