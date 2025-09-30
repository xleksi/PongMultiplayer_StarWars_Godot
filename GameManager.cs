using Godot;
using System.Collections.Generic;


public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    // Key = peer id (long), Value = player data dictionary {"id": long, "name": string}
    public Dictionary<long, Godot.Collections.Dictionary<string, Variant>> Players { get; private set; }
        = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
        Players.Clear();
    }

    public static bool HasPlayer(long id)
    {
        return Instance != null && Instance.Players.ContainsKey(id);
    }

    public static void AddPlayer(long id, string name)
    {
        if (Instance == null) return;
        if (!Instance.Players.ContainsKey(id))
        {
            var playerData = new Godot.Collections.Dictionary<string, Variant>
            {
                ["id"] = id,
                ["name"] = name
            };
            Instance.Players[id] = playerData;
        }
    }

    public static void RemovePlayer(long id)
    {
        if (Instance == null) return;
        if (Instance.Players.ContainsKey(id))
            Instance.Players.Remove(id);
    }
}