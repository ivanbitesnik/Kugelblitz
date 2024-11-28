using Godot;
using GodotSteam;
using System;

public partial class Menu : Node
{
	private PackedScene WorldScene = GD.Load<PackedScene>("res://Levels/world.tscn");

	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";

	private Control Hud;
	private SteamInfoContainer SteamInfoContainer;
	private Control LobbyContainer;
	private LineEdit IPField;
	private ulong lobby_id = 0;

	public override void _Ready()
	{
		SetMultiplayerAuthority(1);
		
		IPField = GetNode<LineEdit>("CenterContainer/VBoxContainer/HBoxContainer/LineEdit");
		Hud = GetNode<Control>("CenterContainer");
		SteamInfoContainer = GetNode<SteamInfoContainer>("SteamInfoContainer");
		SteamInfoContainer.Hide();
		LobbyContainer = GetNode<Control>("LobbyContainer");
		LobbyContainer.Hide();

		SteamStatic.InitRelayNetworkAccess();
    	SteamStatic.LobbyCreated += _on_lobby_created;
    	SteamStatic.LobbyInvite += _on_lobby_invite;
    	SteamStatic.LobbyJoined += _on_lobby_joined;

    	CheckCommandLine();
	}

	private void CheckCommandLine()
	{
		var args = OS.GetCmdlineArgs();
		if (!args.IsEmpty())
		{
			if (args[0] == "+connect_lobby")
			{
				if (args[1].ToInt() > 0)
				{
					JoinLobby((ulong)args[1].ToInt());
				}
			}
		}
	}

	private void _on_lobby_invite(ulong inviter, ulong lobby, ulong game)
	{
		string debugString = "Inviter: " + inviter.ToString() + " lobby: " + lobby.ToString() + " game: " + game.ToString();
		DebugManager.PrintString(this, debugString, 10.0f);
	}

	// Called by client for the present server and called by server for connected client
	private void PeerConnected(long id)
	{
		string contextString = Multiplayer.IsServer() ? "Server" : "Client";
		GD.Print("Peer connected, ID: ", id.ToString(), ", Context: ", contextString);
		
		TravelToWorld();
	}

	private void ConnectedToServer()
	{
		GD.Print("Connected to server, ID: ", Multiplayer.GetUniqueId().ToString());

		Rpc(nameof(TravelToWorld));

		GD.Print("Joining match...");
	}

	private void ConnectionFailed()
	{
		IPField.Text = "";
		IPField.PlaceholderText = "Invalid IP!";
		Hud.Show();
		SteamInfoContainer.Hide();
	}

	private void _on_host_pressed()
	{
		if (!SteamStatic.IsSteamRunning())
		{
			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			Error err = peer.CreateServer(PORT, 2);
			Multiplayer.MultiplayerPeer = peer;

			GD.Print("Waiting for players...");
			Hud.Hide();
			SteamInfoContainer.Show();
			return;
		}

		if (lobby_id != 0) return;

		SteamStatic.CreateLobby(SteamStatic.LobbyType.Public, 2);

		Hud.Hide();
		SteamInfoContainer.Show();
	}

	public void _on_lobby_created(long connect, ulong lobbyId)
	{
		if (connect == 1)
		{
			lobby_id = lobbyId;
			string debugString = "Created lobby id:" + lobbyId.ToString();
			DebugManager.PrintString(this,  debugString, 10.0f);

			SteamStatic.SetLobbyJoinable(lobby_id, true);

			SteamStatic.SetLobbyData(lobby_id, "name", "Lobby");
			SteamStatic.SetLobbyData(lobby_id, "mode", "Godot steam test");

			SteamStatic.CreateListenSocketP2P(0, new Godot.Collections.Array());

			SteamInfoContainer.LobbyId = lobby_id;
		}
	}

	private void _on_search_pressed()
	{
		Hud.Hide();
		LobbyContainer.Show();
	}

	private void _on_join_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error err = peer.CreateClient(ADDRESS, PORT);

		Multiplayer.MultiplayerPeer = peer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Hud.Hide();

		SteamInfoContainer.Hide();
	}

	private void JoinLobby(ulong lobbyId)
	{
		SteamStatic.JoinLobby(lobbyId);
	}

	private void _on_lobby_joined(ulong lobbyId, long permissions, bool locked, long response)
	{
		if (response == 1)
		{
			lobby_id = lobbyId;
			string debugString = "Joined lobby id: " + lobbyId.ToString();
			DebugManager.PrintString(this, debugString, 10.0f);

			SteamStatic.ConnectP2P(lobbyId, 0, new Godot.Collections.Array());
		}
	}

	private void _on_line_edit_changed(string newText)
	{
		ADDRESS = newText;
	}

	private void _on_line_edit_text_submitted(string textSubmitted)
	{
		ADDRESS = textSubmitted;
		_on_join_pressed();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void TravelToWorld()
	{
		GetTree().ChangeSceneToPacked(WorldScene);
	}

	public override void _Notification(int what)
	{
    	if (what == NotificationWMCloseRequest)
		{
			Multiplayer.MultiplayerPeer = null;
        	GetTree().Quit(); // default behavior
		}
	}
}
