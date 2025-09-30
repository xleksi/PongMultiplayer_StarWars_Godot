using Godot;
using System;

public partial class MultiplayerLobby : Control
{
    private Button hostButton;
    private Button joinButton;
    private Button startServerButton;
    private LineEdit lineEdit;

    private const int MAX_PLAYERS = 2;

    [Export] public string Address { get; set; } = "127.0.0.1";
    [Export] public int Port { get; set; } = 8080;

    private ENetMultiplayerPeer peer;

    public override void _Ready()
    {
        hostButton = GetNode<Button>("HostButton");
        joinButton = GetNode<Button>("JoinButton");
        startServerButton = GetNode<Button>("StartServerButton");
        lineEdit = GetNode<LineEdit>("LineEdit");

        hostButton.Pressed += OnHostButtonPressed;
        joinButton.Pressed += OnJoinButtonPressed;
        startServerButton.Pressed += OnStartServerButtonPressed;

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
    }

    private void OnHostButtonPressed()
    {
        if (Multiplayer.MultiplayerPeer != null)
            Multiplayer.MultiplayerPeer.Close();

        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(Port, MAX_PLAYERS);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Cannot host: {error} ({(int)error})");
            return;
        }

        peer.Host?.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;

        var name = string.IsNullOrWhiteSpace(lineEdit.Text) ? $"Player{Multiplayer.GetUniqueId()}" : lineEdit.Text;
        if (!GameManager.HasPlayer(Multiplayer.GetUniqueId()))
            GameManager.AddPlayer(Multiplayer.GetUniqueId(), name);

        GD.Print("Server started — waiting for other players...");
    }

    private void OnJoinButtonPressed()
    {
        if (Multiplayer.MultiplayerPeer != null)
            Multiplayer.MultiplayerPeer.Close();

        peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(Address, Port);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Cannot create client: {error} ({(int)error})");
            return;
        }

        peer.Host?.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
    }

    private void OnStartServerButtonPressed()
    {
        if (Multiplayer.IsServer())
            Rpc(nameof(StartGame));
        else
            GD.Print("Only host can start the game.");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void StartGame()
    {
        if (Multiplayer.MultiplayerPeer == null)
        {
            GD.PrintErr("StartGame called but multiplayer peer is not active yet.");
            return;
        }

        var packed = GD.Load<PackedScene>("res://Scenes/main.tscn");
        var scene = packed.Instantiate();
        GetTree().Root.AddChild(scene);
        Hide();
    }

    private void OnPeerConnected(long id) => GD.Print($"[Lobby] Player connected: {id}");
    private void OnPeerDisconnected(long id) => GD.Print($"[Lobby] Player disconnected: {id}");

    private void OnConnectedToServer()
    {
        GD.Print("[Lobby] Connected to server!");
        var name = string.IsNullOrWhiteSpace(lineEdit.Text) ? $"Player{Multiplayer.GetUniqueId()}" : lineEdit.Text;
        RpcId(1, nameof(SendPlayerInfo), name, Multiplayer.GetUniqueId());
    }

    private void OnConnectionFailed() => GD.PrintErr("[Lobby] Connection failed!");

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void SendPlayerInfo(string name, long id)
    {
        if (!Multiplayer.IsServer())
        {
            RpcId(1, nameof(SendPlayerInfo), name, id);
            return;
        }

        GD.Print($"[Lobby][Server] Registering player {name} ({id})");
        if (!GameManager.HasPlayer(id))
            GameManager.AddPlayer(id, name);

        foreach (var kvp in GameManager.Instance.Players)
        {
            var d = kvp.Value;
            long pid = d["id"].AsInt64();
            string pname = d["name"].AsString();
            Rpc(nameof(ReceivePlayerEntry), pname, pid);
        }

        if (GameManager.Instance.Players.Count >= MAX_PLAYERS)
        {
            GD.Print("[Lobby][Server] Max players reached — starting game.");
            Rpc(nameof(StartGame));
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ReceivePlayerEntry(string name, long id)
    {
        if (!GameManager.HasPlayer(id))
            GameManager.AddPlayer(id, name);

        GD.Print($"[Lobby] Received player entry: {name} ({id})");
    }
}
