using Godot;
using System;
using System.Threading.Tasks;

public partial class World : Node
{
	[Export] public PackedScene MapScene = GD.Load<PackedScene>("res://Maps/MainMap.tscn");
	[Export] public PackedScene PlayerScene = GD.Load<PackedScene>("res://Character/PlayableCharacter.tscn");
	public PackedScene MenuScene = GD.Load<PackedScene>("res://Levels/Lobby/menu.tscn");
	public PackedScene OptionsScene = GD.Load<PackedScene>("res://Levels/OptionsMenu.tscn");

	private Node OptionsMenuInstance;
	public Timer ResetGameTimer = new Timer();
	private Input.MouseModeEnum PreviousMouseMode;

	private Godot.Collections.Array<long> PlayerIds = new Godot.Collections.Array<long>();

	public override void _Ready()
	{
		SetMultiplayerAuthority(1);

		Node map = MapScene.Instantiate();
		map.Name = "Map";
		AddChild(map);

		if (Globals.GlobalSettings.Instance.bIsMultiplayer)
		{
			if (!Multiplayer.IsServer())
			{
				Rpc(nameof(SendClientData), Multiplayer.GetUniqueId());
			}

			Multiplayer.PeerDisconnected += PeerDisconnected;
			Multiplayer.ServerDisconnected += ServerDisconnected;
		}
		else
		{
			SpawnPlayer(1);
		}
		
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
		Character playerInstance = (Character)PlayerScene.Instantiate();
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

	public override void _Notification(int what)
	{
    	if (what == NotificationWMCloseRequest)
		{
        	GetTree().Quit(); // default behavior
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("OpenOptions"))
		{
			if (OptionsMenuInstance is null)
			{
				OptionsMenuInstance = OptionsScene.Instantiate();
				AddChild(OptionsMenuInstance);
				PreviousMouseMode = Input.MouseMode;
				Input.MouseMode = Input.MouseModeEnum.Confined;
			}
			else
			{
				OptionsMenuInstance.QueueFree();
				OptionsMenuInstance = null;
				Input.MouseMode = PreviousMouseMode;
			}
		}
	}

	public void TravelToMenu()
	{
		GetTree().ChangeSceneToPacked(MenuScene);
	}
}
