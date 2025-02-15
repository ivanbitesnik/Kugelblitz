using Godot;
using GodotSteam;
using System;

public partial class StagingScreen : Control
{
	private Label LobbyName;
	private HBoxContainer Player1HBox;
	private HBoxContainer Player2HBox;
	private Button LocalPlayerReadyButton;
	private Label RemotePlayerReadyLabel;

	private bool LocalPlayerReady = false;
	private bool RemotePlayerReady = false;

	[Signal]
	public delegate void OnAllPlayersReadyEventHandler();

	public override void _Ready()
	{
		LobbyName = FindChild("LobbyName") as Label;
		Player1HBox = FindChild("Player1HBox") as HBoxContainer;
		Player2HBox = FindChild("Player2HBox") as HBoxContainer;

		LocalPlayerReadyButton = new Button();
		LocalPlayerReadyButton.Text = "Not ready";
		LocalPlayerReadyButton.ButtonUp += OnLocalPlayerButtonUp;
		RemotePlayerReadyLabel = new Label();
		RemotePlayerReadyLabel.Text = "Not ready";
	}

	public void Initialize(ulong lobbyId)
	{
		if (Multiplayer.IsServer())
		{
			Player1HBox.AddChild(LocalPlayerReadyButton);
			Player2HBox.AddChild(RemotePlayerReadyLabel);
		}
		else
		{
			Player1HBox.AddChild(RemotePlayerReadyLabel);
			Player2HBox.AddChild(LocalPlayerReadyButton);
		}
		
		if (!SteamStatic.IsSteamRunning()) return;

		LobbyName.Text = SteamStatic.GetLobbyData(lobbyId, "name");
		Label Player1Name = Player1HBox.FindChild("Player1Name") as Label;
		Label Player2Name = Player2HBox.FindChild("Player2Name") as Label;
		Player1Name.Text = SteamStatic.GetLobbyMemberData(lobbyId, SteamStatic.GetLobbyMemberByIndex(lobbyId, 0), "name");
		Player2Name.Text = SteamStatic.GetLobbyMemberData(lobbyId, SteamStatic.GetLobbyMemberByIndex(lobbyId, 1), "name");

	}

	private void OnLocalPlayerButtonUp()
	{
		LocalPlayerReady = !LocalPlayerReady;
		LocalPlayerReadyButton.Text = LocalPlayerReady ? "Ready" : "Not ready";
		Rpc(nameof(SetRemotePlayerReady), LocalPlayerReady);
		if (LocalPlayerReady && RemotePlayerReady)
		{
			EmitSignal(nameof(OnAllPlayersReady));
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SetRemotePlayerReady(bool NewState)
	{
		RemotePlayerReadyLabel.Text = NewState ? "Ready" : "Not ready";
		RemotePlayerReady = NewState;

		if (LocalPlayerReady && RemotePlayerReady)
		{
			EmitSignal(nameof(OnAllPlayersReady));
		}
	}
}
