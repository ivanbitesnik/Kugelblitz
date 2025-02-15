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
	private Timer ResetGameTimer = new Timer();

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

		ResetGameTimer.OneShot = true;
		ResetGameTimer.Autostart = false;
		ResetGameTimer.WaitTime = 3.0f;
		AddChild(ResetGameTimer);
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void OnPeerReturnedToMenu(int peerId)
	{
		if (Multiplayer.IsServer())
		{
			Multiplayer.MultiplayerPeer.DisconnectPeer(peerId);
		}
	}

	// Called on any context for disconnected peer
	private void PeerDisconnected(long id)
	{
		if (Multiplayer.IsServer())
		{
			Rpc(nameof(RemovePlayer), id);
		}
	}

	private void ServerDisconnected()
	{
		GD.Print("Server disconnected");
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
		if (ResetGameTimer.TimeLeft > 0.0f) return;
		ResetGameTask();
	}

	private async Task ResetGameTask()
	{
		ResetGameTimer.Start();
		await ToSignal(ResetGameTimer, SceneTreeTimer.SignalName.Timeout);
		foreach (long id in PlayerIds)
		{
			Rpc(nameof(RemovePlayer), id);
			SpawnPlayer(id);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RemovePlayer(long playerId)
	{
		Node player = FindChild(playerId.ToString(), true, false);
		if (player is not null)
		{
			player.Name = "PendingDestroyPlayer"; // We change the name so that its not taken when spawning a new one
			player.QueueFree();
		}
	}

	public void TravelToMenu()
	{
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
