using Godot;

public partial class Main : Node2D
{
    [Export] public int PlayerPoints { get; set; } = 0;
    [Export] public int EnemyPoints { get; set; } = 0;

    [Export] public PackedScene YodaScene { get; set; } = GD.Load<PackedScene>("res://Scenes/Players/yoda.tscn");
    [Export] public PackedScene GeneralGrievousScene { get; set; } = GD.Load<PackedScene>("res://Scenes/Players/GeneralGrievous.tscn");

    private Ball ball;
    private UI ui;
    private AudioStreamPlayer YodaAudio;
    private AudioStreamPlayer GeneralGrievousAudio;

    public override void _Ready()
    {
        ball = GetNode<Ball>("Ball");
        ui = GetNode<UI>("UI");
        YodaAudio = GetNode<AudioStreamPlayer>("AudioYoda");
        GeneralGrievousAudio = GetNode<AudioStreamPlayer>("AudioGeneralGrievous");

        GD.Print("GameManager.Players contents: " + GameManager.Players);

        int index = 0;
        foreach (var kvp in GameManager.Players)
        {
            long id = kvp.Key;
            var playerDict = kvp.Value;

            // Assign Yoda to first player, Grievous to second
            Node2D currentPlayer = (index == 0)
                ? (Node2D)YodaScene.Instantiate()
                : (Node2D)GeneralGrievousScene.Instantiate();

            currentPlayer.Name = id.ToString();
            AddChild(currentPlayer);

            foreach (Node spawn in GetTree().GetNodesInGroup("PlayerSpawn"))
            {
                if (spawn is Node2D spawnNode && spawnNode.Name == index.ToString())
                {
                    currentPlayer.GlobalPosition = spawnNode.GlobalPosition;
                    break;
                }
            }

            index++;
        }
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
        ball.Velocity = Vector2.Zero;
        ball.StartBall();
    }
}
