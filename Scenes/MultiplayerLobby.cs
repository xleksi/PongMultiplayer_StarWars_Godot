using Godot;

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
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(Port, MAX_PLAYERS);

        if (error != Error.Ok)
        {
            GD.Print($"Cannot host: {error} ({(int)error})");
            return;
        }

        peer.Host?.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;

        GD.Print("WAITING FOR OTHER PLAYER!");
        SendPlayerInfo(lineEdit.Text, Multiplayer.GetUniqueId());
    }

    private void OnJoinButtonPressed()
    {
        peer = new ENetMultiplayerPeer();
        peer.CreateClient(Address, Port);
        peer.Host?.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
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
    private void OnPeerDisconnected(long id) => GD.Print($"Player disconnected: {id}");

    private void OnConnectedToServer()
    {
        GD.Print("Connected to server!");
        RpcId(1, nameof(SendPlayerInfo), lineEdit.Text, Multiplayer.GetUniqueId());
    }

    private void OnConnectionFailed() => GD.Print("Connection failed!");

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void SendPlayerInfo(string name, long id)
    {
        if (Multiplayer.IsServer())
        {
            if (!GameManager.HasPlayer(id))
                GameManager.AddPlayer(id, name);

            foreach (var kvp in GameManager.Players)
            {
                var d = kvp.Value;
                long pid = d["id"].AsInt64();
                string pname = d["name"].AsString();
                Rpc(nameof(ReceivePlayerEntry), pname, pid);
            }
        }
        else
        {
            RpcId(1, nameof(SendPlayerInfo), name, id);
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
