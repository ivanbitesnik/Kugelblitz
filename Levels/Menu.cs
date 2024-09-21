using Godot;
using System;

public partial class Menu : Node
{
	private PackedScene WorldScene = GD.Load<PackedScene>("res://Levels/world.tscn");

	[Export] private int PORT = 8080;
	[Export] private string ADDRESS = "127.0.0.1";

	private CenterContainer Hud;
	private LineEdit IPField;

	public override void _Ready()
	{
		SetMultiplayerAuthority(1);

		IPField = GetNode<LineEdit>("CenterContainer/VBoxContainer/HBoxContainer/LineEdit");
		Hud = GetNode<CenterContainer>("CenterContainer");
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
	}

	private void _on_host_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error err = peer.CreateServer(PORT, 2);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Waiting for players...");

		Hud.Hide();
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
}
