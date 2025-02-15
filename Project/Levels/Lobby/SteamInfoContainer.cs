using Godot;
using GodotSteam;
using System;
using System.Drawing;
using System.Numerics;

public partial class SteamInfoContainer : VBoxContainer
{
	private const uint AppId = 480;

	public Button CloseLobbyButton = new Button();
	public PlayerInfo PlayerInfo = new PlayerInfo();
	public FriendList FriendList = new FriendList();

	public override void _EnterTree()
	{
		OS.SetEnvironment("SteamAppId", AppId.ToString());
		OS.SetEnvironment("SteamGameId", AppId.ToString());
	}

	public override void _Ready()
	{	
		SteamInitResult result = SteamStatic.SteamInit();

		CloseLobbyButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		CloseLobbyButton.Text = "Close lobby";
		CloseLobbyButton.Modulate = Colors.IndianRed;
		AddChild(CloseLobbyButton);

		PlayerInfo.SizeFlagsVertical = SizeFlags.ExpandFill;
		AddChild(PlayerInfo);

		FriendList.SizeFlagsVertical = SizeFlags.ExpandFill;
		FriendList.SizeFlagsStretchRatio = 3.0f;
		AddChild(FriendList);
	}

	public void Initialize()
	{
		FriendList.Initialize();
	}

	public void OnLobbyClosed()
	{
		FriendList.ClearList();
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		if (SteamStatic.IsSteamRunning())
		{
			SteamStatic.RunCallbacks();
		}
    }
}
