using Godot;
using System;
using Kugelblitz.Character;

public partial class World : Node3D
{
	private PackedScene MenuScene = GD.Load<PackedScene>("res://Levels/Lobby/menu.tscn");

	[Export] private PackedScene PlayerScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Input.SetMouseMode(Input.MouseModeEnum.Confined);

		SetMultiplayerAuthority(1);

		if (!Multiplayer.IsServer())
		{
			Rpc(nameof(SendClientData), Multiplayer.GetUniqueId());
		}

		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ServerDisconnected += ServerDisconnected;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SendClientData(long id)
	{
		SpawnPlayers(id);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called on any context for disconnected peer
	private void PeerDisconnected(long id)
	{
		string contextString = IsMultiplayerAuthority() ? "Server" : "Client";
		GD.Print("Peer disconnected ID: ", id.ToString(), ", Context: ", contextString);

		if (Multiplayer.IsServer())
		{
			TravelToMenu();
		}
	}

	private void ServerDisconnected()
	{
		GD.Print("Server disconnected");
		QueueFree();
		TravelToMenu();
	}

	private void SpawnPlayers(long id)
	{
		SpawnPlayer(1);		// Server player
		SpawnPlayer(id);	// Client player
	}

	private void SpawnPlayer(long id)
	{
		Player playerInstance = (Player)PlayerScene.Instantiate();
		playerInstance.Name = id.ToString();

		AddChild(playerInstance, true);
	}

	private void TravelToMenu()
	{
		Multiplayer.MultiplayerPeer.Close();
		Multiplayer.MultiplayerPeer = null;
		GetTree().ChangeSceneToPacked(MenuScene);
	}

	public override void _Notification(int what)
	{
    	if (what == NotificationWMCloseRequest)
		{
        	GetTree().Quit(); // default behavior
		}
	}
}
