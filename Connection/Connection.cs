using Godot;
using System;

public partial class Connection : Node
{
	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";
	private PackedScene PlayerScene = GD.Load<PackedScene>("res://Character/player.tscn");
	private CenterContainer Hud;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;

		Hud = (CenterContainer)GetNode("CenterContainer");
	}

	private void PeerDisconnected(long id)
	{
		GD.Print("Peer disconnected ID:" + id.ToString());
	}

	private void PeerConnected(long id = 1)
	{
		GD.Print("Peer connected ID:" + id.ToString());
		SpawnPlayer(id);
	}

	private void _on_host_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateServer(PORT, 2);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for players");
		PeerConnected();
		Hud.Hide();
	}

	private void _on_join_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateClient("localhost", PORT);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting to join");
		Hud.Hide();

		SpawnPlayer(Multiplayer.GetUniqueId());
	}

	private void SpawnPlayer(long id)
	{
		var playerInstance = PlayerScene.Instantiate();
		playerInstance.Name = id.ToString();
		AddChild(playerInstance, true);
	}
}
