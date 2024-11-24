using Godot;
using GodotSteam;
using System;
using System.Drawing;

public partial class SteamInfoContainer : Control
{
	private const uint AppId = 480;

	public PlayerInfo PlayerInfo;
	public SteamStatus SteamStatus;
	public FriendList FriendList;

	[Export] public float SteamStatusSize = 75.0f;

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

		SteamStatus = new SteamStatus();
		SteamStatus.SetSize(new Vector2(Size.X, SteamStatusSize));
		AddChild(SteamStatus);

		FriendList = new FriendList();
		FriendList.SetSize(new Vector2(Size.X, Size.Y - SteamStatusSize));
		AddChild(FriendList);
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		SteamStatic.RunCallbacks();
    }
}
