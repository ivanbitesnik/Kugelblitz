using Godot;
using GodotSteam;
using System;
using System.Diagnostics;
using System.Net.Sockets;

public partial class Menu : Node
{
	private PackedScene WorldScene = GD.Load<PackedScene>("res://Levels/world.tscn");
	private GDScript P2PFunctionLibrary = GD.Load<GDScript>("res://Levels/Lobby/P2PFunctionLibrary.gd");
	private GodotObject P2PFunctionLibraryNode;

	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";

	private Control Hud;
	private SteamInfoContainer SteamInfoContainer;
	private LobbyContainer LobbyContainer;
	private LineEdit IPField;
	private ulong lobby_id = 0;

	public override void _Ready()
	{
		SetMultiplayerAuthority(1);

		P2PFunctionLibraryNode = (GodotObject)P2PFunctionLibrary.New();
		
		IPField = GetNode<LineEdit>("CenterContainer/VBoxContainer/HBoxContainer/LineEdit");
		Hud = GetNode<Control>("CenterContainer");
		SteamInfoContainer = GetNode<SteamInfoContainer>("SteamInfoContainer");
		SteamInfoContainer.Hide();
		LobbyContainer = GetNode<LobbyContainer>("LobbyContainer");
		LobbyContainer.Hide();

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

	// Called by client for the present server and called by server for connected client
	private void PeerConnected(long id)
	{
		string contextString = Multiplayer.IsServer() ? "Server" : "Client";
		GD.Print("Peer connected, ID: ", id.ToString(), ", Context: ", contextString);
		
		TravelToWorld();
	}

	private void ConnectedToServer()
	{
		DebugManager.PrintString(this, "Connected to server, ID: " + Multiplayer.GetUniqueId().ToString(), 10.0f);

		Rpc(nameof(TravelToWorld));
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
		Hud.Show();
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

			SteamStatic.SetLobbyOwner(lobby_id, SteamStatic.GetSteamID());
			SteamStatic.SetLobbyJoinable(lobby_id, true);

			string steamName = SteamStatic.GetFriendPersonaName(SteamStatic.GetSteamID());
			SteamStatic.SetLobbyData(lobby_id, "name", steamName + "'s Lobby");

			SteamInfoContainer.LobbyId = lobby_id;

			P2PFunctionLibraryNode.Call("CreateServerPeer");
		}
		else
		{
			DebugManager.PrintString(this, "Lobby creation failed", 10.0f);
		}
	}

	private void _on_search_pressed()
	{
		Hud.Hide();
		SteamStatic.AddRequestLobbyListDistanceFilter(SteamStatic.LobbyDistanceFilter.Close);
		SteamStatic.RequestLobbyList();
	}

	private void _on_lobby_search_finished(Godot.Collections.Array lobbies)
	{
		LobbyContainer.SetLobbies(lobbies);
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
			if (lobby_id != 0)
			{
				SteamStatic.CloseListenSocket(0);
				SteamStatic.LeaveLobby(lobby_id);
				lobby_id = 0;
			}

        	GetTree().Quit(); // default behavior
		}
	}
}
