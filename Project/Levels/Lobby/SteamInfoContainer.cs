using Godot;
using GodotSteam;
using System;
using System.Drawing;
using System.Numerics;

public partial class SteamInfoContainer : Control
{
	private const uint AppId = 480;

	public PlayerInfo PlayerInfo;
	public FriendList FriendList;
	public VBoxContainer VerticalContainer;

	[Export] public float SteamStatusHeight = 50.0f;
	[Export] public float PlayerInfoHeight = 75.0f;

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

		VerticalContainer = new VBoxContainer();
		VerticalContainer.SetSize(Size);
		VerticalContainer.SizeFlagsVertical = SizeFlags.Expand;
		VerticalContainer.ClipChildren = ClipChildrenMode.Disabled;
		AddChild(VerticalContainer);

		PlayerInfo = new PlayerInfo();
		PlayerInfo.SetSize(new Godot.Vector2(Size.X, PlayerInfoHeight));
		VerticalContainer.AddChild(PlayerInfo);

		FriendList = new FriendList();
		FriendList.FriendItemHeight = PlayerInfoHeight;
		FriendList.SetSize(new Godot.Vector2(Size.X, Size.Y - (SteamStatusHeight + PlayerInfoHeight)));
		VerticalContainer.AddChild(FriendList);
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		SteamStatic.RunCallbacks();
    }
}
