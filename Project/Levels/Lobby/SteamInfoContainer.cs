using Godot;
using GodotSteam;
using System;

public partial class SteamInfoContainer : CanvasLayer
{
	private const uint AppId = 480;

	[Export] public PlayerInfo PlayerInfo;
	[Export] public SteamStatus SteamStatus;
	[Export] public FriendList FriendList;

	public override void _EnterTree()
	{
		OS.SetEnvironment("SteamAppId", AppId.ToString());
		OS.SetEnvironment("SteamGameId", AppId.ToString());
	}

	public override void _Ready()
	{	
		SteamInitResult result = SteamStatic.SteamInit();
		GD.Print("Steam init result: ", result.Status.ToString());
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		SteamStatic.RunCallbacks();
    }
}
