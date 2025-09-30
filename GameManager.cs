using Godot;
using System.Collections.Generic;

public static class GameManager
{
    // Strongly typed dictionary:
    // Key = player id (long), value = player info (name + id in Variant dict)
    public static Dictionary<long, Godot.Collections.Dictionary<string, Variant>> Players { get; private set; }
        = new();

    public static bool HasPlayer(long id)
    {
        return Players.ContainsKey(id);
    }

    public static void AddPlayer(long id, string name)
    {
        if (!Players.ContainsKey(id))
        {
            var playerData = new Godot.Collections.Dictionary<string, Variant>
            {
                ["id"] = id,
                ["name"] = name
            };
            Players[id] = playerData;
        }
    }

    public static void RemovePlayer(long id)
    {
        if (Players.ContainsKey(id))
            Players.Remove(id);
    }
}