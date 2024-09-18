using Godot;
using System;

public partial class Connection : Node
{
	[Export] private int port = 8080;
	[Export] private string address = "127.0.0.1";
	private PackedScene PlayerScene = GD.Load<PackedScene>("res://Projectile/player.tscn");

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ServerDisconnected += ServerDisconnected;
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
		var err = peer.CreateServer(port, 2);
		
		peer.Host.Compress(ENetConnection.CompressionMode.None);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for players");
		PeerConnected(Guid.NewGuid().GetHashCode());
	}

	private void _on_join_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateClient(address, port);
		
		peer.Host.Compress(ENetConnection.CompressionMode.None);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting to join");
		
	}
}
