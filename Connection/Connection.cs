using Godot;
using System;

public partial class Connection : Node
{
	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";
	private PackedScene PlayerScene = GD.Load<PackedScene>("res://Character/player.tscn");
	private CenterContainer Hud;

	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;

		Hud = GetNode<CenterContainer>("CenterContainer");
	}

	// Called by client for the present server and called by server for connected client
	private void PeerConnected(long id)
	{
		string contextString = IsMultiplayerAuthority() ? "Server" : "Client";
		GD.Print("Peer connected, ID: ", id.ToString(), ", Context: ", contextString);
		SpawnPlayer(id);
	}

	// Called on any context for disconnected peer
	private void PeerDisconnected(long id)
	{
		string contextString = IsMultiplayerAuthority() ? "Server" : "Client";
		GD.Print("Peer disconnected ID: ", id.ToString(), ", Context: ", contextString);
		RemovePlayer(id);

		bool bServerDisconnected = (id == 1);
		if (bServerDisconnected)
		{
			RemovePlayer(Multiplayer.GetUniqueId());
			Hud.Show();
		}
	}

	// Called by client when connected to the match
	private void ConnectedToServer()
	{
		string contextString = IsMultiplayerAuthority() ? "Server" : "Client";
		GD.Print("Connected to server, ID: ", Multiplayer.GetUniqueId(), ", Context: ", contextString);
		SpawnPlayer(Multiplayer.GetUniqueId());
	}

	private void _on_host_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateServer(PORT, 2);
		Multiplayer.MultiplayerPeer = peer;

		SetMultiplayerAuthority(1);

		SpawnPlayer(1);
		Hud.Hide();
	}

	private void _on_join_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateClient("localhost", PORT);
		Multiplayer.MultiplayerPeer = peer;

		SetMultiplayerAuthority(1);

		Multiplayer.ConnectedToServer += ConnectedToServer;

		Hud.Hide();
	}

	private void SpawnPlayer(long id)
	{
		var playerInstance = PlayerScene.Instantiate();
		playerInstance.Name = id.ToString();
		GetTree().Root.AddChild(playerInstance, true);
	}

	private void RemovePlayer(long id)
	{
		GetTree().Root.GetNode(id.ToString()).QueueFree();
	}
}
