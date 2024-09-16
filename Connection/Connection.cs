using Godot;
using System;

public partial class Connection : Node
{
	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";
	private PackedScene PlayerScene = GD.Load<PackedScene>("res://Character/player.tscn");

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var upnp = new Upnp();
		upnp.Discover();
		upnp.AddPortMapping(PORT);
	}

	private void ConnectedToServer()
	{
		GD.Print("Connected to server");
	}

	private void ServerDisconnected()
	{
		GD.Print("Server disconnected");
	}

	private void PeerDisconnected(long id)
	{
		GD.Print("Peer disconnected ID:" + id.ToString());
	}

	
	private void PeerConnected(long id)
	{
		GD.Print("Peer connected ID:" + id.ToString());
		var playerInstance = PlayerScene.Instantiate();
		AddChild(playerInstance);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_host_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateServer(PORT, 2);
		
		//peer.Host.Compress(ENetConnection.CompressionMode.None);
		
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for players");
		
		Multiplayer.PeerDisconnected += remove_player;

		LoadGame();

	}

	
	private void _on_join_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateClient(ADDRESS, PORT);
		
		peer.Host.Compress(ENetConnection.CompressionMode.None);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting to join");
		
		Multiplayer.ConnectedToServer += LoadGame;
		Multiplayer.ServerDisconnected += ConnectionLost;
	}

	private void LoadGame()
	{
		add_player(Multiplayer.GetUniqueId());
	}

	private void ConnectionLost()
	{
		
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void add_player(long id)
	{
		var player = PlayerScene.Instantiate();
		player.Name = id.ToString();
		GetTree().Root.AddChild(player);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void remove_player(long id)
	{
		
	}
	
}
