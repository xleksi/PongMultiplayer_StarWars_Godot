using Godot;
using System;

public partial class UI : CanvasLayer
{
    private Label playerPointsLabel;
    private Label enemyPointsLabel;
    
    public override void _Ready()
    {
        playerPointsLabel = GetNode<Label>("%PlayerPoints");
        enemyPointsLabel = GetNode<Label>("%EnemyPoints");

        playerPointsLabel.Text = "0";
        enemyPointsLabel.Text = "0";
    }

    public void UpdateEnemyPoints(int points)
    {
        enemyPointsLabel.Text = points.ToString();
    }

    public void UpdatePlayerPoints(int points)
    {
        playerPointsLabel.Text = points.ToString();
    }
}
