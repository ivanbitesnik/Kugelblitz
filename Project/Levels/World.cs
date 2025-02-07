using Godot;
using System;
using Kugelblitz.Character;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

public partial class World : Node3D
{
	private PackedScene MenuScene = GD.Load<PackedScene>("res://Levels/Lobby/menu.tscn");
	private PackedScene OptionsScene = GD.Load<PackedScene>("res://Levels/OptionsMenu.tscn");
	private PackedScene PlayerScene = GD.Load<PackedScene>("res://Character/player.tscn");
	
	private Godot.Collections.Array<long> PlayerIds = new Godot.Collections.Array<long>();
	private Node OptionsMenuInstance;

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

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("OpenOptions"))
		{
			if (OptionsMenuInstance is null)
			{
				OptionsMenuInstance = OptionsScene.Instantiate();
				AddChild(OptionsMenuInstance);
			}
			else
			{
				OptionsMenuInstance.QueueFree();
				OptionsMenuInstance = null;
			}
		}
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
		playerInstance.OnPlayerDead += ResetGame;

		AddChild(playerInstance, true);
		if (!PlayerIds.Contains(id)) PlayerIds.Add(id);
	}

	private void ResetGame()
	{
		ResetGameTask();
	}

	private async Task ResetGameTask()
	{
		await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);
		foreach (long id in PlayerIds)
		{
			Rpc(nameof(RemovePlayer), id);
			SpawnPlayer(id);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RemovePlayer(long playerId)
	{
		Node player = FindChild(playerId.ToString());
		if (player is not null)
		{
			player.QueueFree();
		}
	}

	private void TravelToMenu()
	{
		Multiplayer.MultiplayerPeer.Close();
		Multiplayer.MultiplayerPeer = null;
		QueueFree();
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
