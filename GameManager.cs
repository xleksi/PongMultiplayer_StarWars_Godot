using Godot;
using System;
using Godot.Collections;

public partial class GameManager : Node
{
    public static readonly Dictionary Players = new Dictionary();

    public static void AddPlayer(long id, string name)
    {
        if (!Players.ContainsKey(id))
        {
            Players[id] = new Dictionary {
                {"name", name},
                { "id", id } };
        }
    }

    public static bool HasPlayer(long id) => Players.ContainsKey(id);

    public static Dictionary GetPlayerDict(long id) => Players.ContainsKey(id) ? (Dictionary)Players[id] : null;
}
