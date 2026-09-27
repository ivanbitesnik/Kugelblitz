using Godot;
using GodotSteam;
using System;
using System.Diagnostics;
using System.Net.Sockets;

public partial class Menu : Node
{
	[Export] private PackedScene WorldScene = GD.Load<PackedScene>("res://Levels/World.tscn");
	
	private GDScript P2PFunctionLibrary = GD.Load<GDScript>("res://Levels/Lobby/P2PFunctionLibrary.gd");
	private Node P2PFunctionLibraryNode;

	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";

	private Control MenuContainer;
	private SteamInfoContainer SteamInfoContainer;
	private LobbyContainer LobbyContainer;
	private StagingScreen StagingScreen;
	private LineEdit IPField;
	private ulong lobby_id = 0;

	public override void _Ready()
	{
		SetMultiplayerAuthority(1);
		Globals.GlobalSettings.Instance.bIsMultiplayer = true;

		P2PFunctionLibraryNode = (Node)P2PFunctionLibrary.New();
		AddChild(P2PFunctionLibraryNode);
		
		IPField = GetNode<LineEdit>("CenterContainer/VBoxContainer/HBoxContainer/LineEdit");
		MenuContainer = GetNode<Control>("CenterContainer");

		SteamInfoContainer = GetNode<SteamInfoContainer>("SteamInfoContainer");
		SteamInfoContainer.Hide();
		SteamInfoContainer.CloseLobbyButton.ButtonUp += OnBackToMenuPressed;

		LobbyContainer = GetNode<LobbyContainer>("LobbyContainer");
		LobbyContainer.Hide();
		LobbyContainer.BackToMenuButton.ButtonUp += OnBackToMenuPressed;

		StagingScreen = GetNode<StagingScreen>("StagingScreen");
		StagingScreen.Hide();
		StagingScreen.OnAllPlayersReady += TravelToWorld;

		SteamStatic.InitRelayNetworkAccess();

    	SteamStatic.LobbyCreated += _on_lobby_created;
    	SteamStatic.LobbyJoined += _on_lobby_joined;
		SteamStatic.LobbyMatchList += _on_lobby_search_finished;
		SteamStatic.JoinRequested += _on_join_requested;

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
					SteamStatic.JoinLobby((ulong)args[1].ToInt());
				}
			}
		}
	}

	private void ConnectedToServer()
	{
		DebugManager.PrintString(this, "Connected to server, ID: " + Multiplayer.GetUniqueId().ToString(), 10.0f);

		if (SteamStatic.IsSteamRunning())
		{
			Rpc(nameof(LoadStagingArea));
		}
		else
		{
			Rpc(nameof(TravelToWorld));
		}
	}

	private void ConnectionFailed()
	{
		if (SteamStatic.IsSteamRunning())
		{
			DebugManager.PrintString(this, "Failed to connect to server", 10.0f);
			return;
		}

		IPField.Text = "";
		IPField.PlaceholderText = "Invalid IP!";
		MenuContainer.Show();
	}

	private void _on_single_player_pressed()
	{
		Globals.GlobalSettings.Instance.bIsMultiplayer = false;
		TravelToWorld();
    }

	private void _on_host_pressed()
	{
		if (!SteamStatic.IsSteamRunning())
		{
			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			Error err = peer.CreateServer(PORT, 2);
			Multiplayer.MultiplayerPeer = peer;

			MenuContainer.Hide();
			SteamInfoContainer.Show();
			return;
		}

		if (lobby_id != 0) return;

		SteamStatic.CreateLobby(SteamStatic.LobbyType.Public, 2);

		MenuContainer.Hide();
		SteamInfoContainer.Show();
	}

	public void _on_lobby_created(long connect, ulong lobbyId)
	{
		if (connect == 1)
		{
			lobby_id = lobbyId;

			SteamStatic.SetLobbyOwner(lobby_id, SteamStatic.GetSteamID());
			SteamStatic.SetLobbyJoinable(lobby_id, true);

			
			string steamName = SteamStatic.GetPersonaName();
			SteamStatic.SetLobbyData(lobby_id, "name", steamName + "'s Lobby");
			SteamStatic.SetLobbyMemberData(lobbyId, "name", steamName);

			SteamInfoContainer.Initialize();

			P2PFunctionLibraryNode.Call("CreateServerPeer");
		}
		else
		{
			DebugManager.PrintString(this, "Lobby creation failed", 10.0f);
		}
	}

	private void _on_search_pressed()
	{
		MenuContainer.Hide();
		SteamStatic.AddRequestLobbyListDistanceFilter(SteamStatic.LobbyDistanceFilter.Close);
		SteamStatic.RequestLobbyList();
	}

	private void _on_lobby_search_finished(Godot.Collections.Array lobbies)
	{
		LobbyContainer.SetLobbies(lobbies);
		LobbyContainer.Show();
	}

	private void OnBackToMenuPressed()
	{
		if (lobby_id != 0)
		{
			LeaveLobby();
			SteamInfoContainer.Hide();
			SteamInfoContainer.OnLobbyClosed();
		}
		else
		{
			LobbyContainer.Hide();
			LobbyContainer.ClearLobbies();
		}

		MenuContainer.Show();
	}

	private void _on_join_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error err = peer.CreateClient(ADDRESS, PORT);

		Multiplayer.MultiplayerPeer = peer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		
		Multiplayer.ConnectedToServer += ConnectedToServer;
		MenuContainer.Hide();
	}

	// Steam invites trigger this callback on the invitee
	private void _on_join_requested(ulong lobbyId, ulong steamId)
	{
		SteamStatic.JoinLobby(lobbyId);
	}

	private void _on_lobby_joined(ulong lobbyId, long permissions, bool locked, long response)
	{
		if (lobby_id == lobbyId) return;

		ChatRoomEnterResponse Response = (ChatRoomEnterResponse)response;
		DebugManager.PrintString(this, "Attempting to join lobby: " + lobbyId.ToString() + ", response: " + Response.ToString(), 10.0f);
		if (Response == ChatRoomEnterResponse.Success)
		{
			lobby_id = lobbyId;
			SteamStatic.SetLobbyMemberData(lobby_id, "name", SteamStatic.GetPersonaName());
			P2PFunctionLibraryNode.Call("CreateClientPeer", SteamStatic.GetLobbyOwner(lobby_id));

			Multiplayer.ConnectionFailed += ConnectionFailed;
			Multiplayer.ConnectedToServer += ConnectedToServer;
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

	public ulong GetLobbyId()
	{
		return lobby_id;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void LoadStagingArea()
	{
		MenuContainer.Hide();
		LobbyContainer.Hide();
		SteamInfoContainer.Hide();

		StagingScreen.Initialize(lobby_id);
		StagingScreen.Show();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void TravelToWorld()
	{
		LeaveLobby();
		GetTree().ChangeSceneToPacked(WorldScene);
	}

	public override void _Notification(int what)
	{
    	if (what == NotificationWMCloseRequest)
		{
			LeaveLobby();

        	GetTree().Quit(); // default behavior
		}
	}

	public void LeaveLobby()
	{
		if (lobby_id != 0)
		{
			SteamStatic.LeaveLobby(lobby_id);
			lobby_id = 0;
		}
	}

	public void _on_exit_game_button_up()
	{
		GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
	}
}
