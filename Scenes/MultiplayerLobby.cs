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

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisonnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;

    }

    private void OnHostButtonPressed()
    {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(Port, MAX_PLAYERS);

        if (error != Error.Ok)
        {
            GD.Print($"Cannot host: {error} + ({(int)error})");
            return;
        }

        if (peer.Host != null)
            peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

        Multiplayer.MultiplayerPeer = peer;
        GD.Print("WAITING FOR OTHER PLAYER!");
        SendPlayerInfo(lineEdit.Text, Multiplayer.GetUniqueId());
    }

    private void OnJoinButtonPressed()
    {
        peer = new ENetMultiplayerPeer();
        peer.CreateClient(Address, Port);
        if (peer.Host != null)
            peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.SetMultiplayerPeer(peer);
    }

    private void OnStartServerButtonPressed()
    {
        Rpc(nameof(StartGame));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void StartGame()
    {
        var packed = GD.Load<PackedScene>("res://Scenes/main.tscn");
        var scene = packed.Instantiate();
        GetTree().Root.AddChild(scene);
        Hide();
    }
    
    private void OnPeerConnected(long id) => GD.Print($"Player connected: {id}");
    private void OnPeerDisonnected(long id) => GD.Print($"Player disconnected: {id}");
    private void OnConnectedToServer()
    {
        GD.Print("Connected to server!");
        RpcId(1, nameof(SendPlayerInfo), lineEdit.Text, Multiplayer.GetUniqueId());
    }

    private void OnConnectionFailed() => GD.Print("Connection failed!");

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]    
    public void SendPlayerInfo(string name, long id)
    {
        if (!GameManager.HasPlayer(id))
            GameManager.AddPlayer(id, name);

        long senderId = Multiplayer.GetRemoteSenderId();

        if (Multiplayer.IsServer() && senderId != 0)
        {
            foreach (object rawKey in GameManager.Players.Keys)
            {
                long targetPeerId;
                
                if (rawKey is long l) targetPeerId = l;
                else if (rawKey is int i) targetPeerId = i;
                else
                {
                    if (!long.TryParse(rawKey?.ToString() ?? "", out targetPeerId))
                        continue;
                }
                
                if (targetPeerId == Multiplayer.GetUniqueId() || targetPeerId == senderId)
                    continue;

                RpcId(targetPeerId, nameof(SendPlayerInfo), name, id);
            }
        }
    }
}
