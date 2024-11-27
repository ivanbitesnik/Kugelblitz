using Godot;
using GodotSteam;
using System;
using System.Drawing;
using System.Numerics;

public partial class SteamInfoContainer : VBoxContainer
{
	private const uint AppId = 480;

	public PlayerInfo PlayerInfo = new PlayerInfo();
	public FriendList FriendList = new FriendList();

	[Export] public float FriendItemHeight = 50.0f;
	[Export] public float PlayerInfoHeight = 150.0f;

	public override void _EnterTree()
	{
		OS.SetEnvironment("SteamAppId", AppId.ToString());
		OS.SetEnvironment("SteamGameId", AppId.ToString());
	}

	public override void _Ready()
	{	
		SteamInitResult result = SteamStatic.SteamInit();
		string steamInitResult = "Steam init result: " + result.Status.ToString();
		GD.Print(steamInitResult);
		DebugManager.PrintString(this, steamInitResult, 10.0f);

		PlayerInfo.SizeFlagsVertical = SizeFlags.ExpandFill;
		AddChild(PlayerInfo);

		FriendList.SizeFlagsVertical = SizeFlags.ExpandFill;
		FriendList.SizeFlagsStretchRatio = 3.0f;
		AddChild(FriendList);
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		SteamStatic.RunCallbacks();
    }
}
