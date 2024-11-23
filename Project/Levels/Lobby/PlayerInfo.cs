using Godot;
using GodotSteam;
using System;

public partial class PlayerInfo : PanelContainer
{
	[Export] public Panel PlayerAvatar;
	
	[Export] public Label PlayerName;
	[Export] public Label PlayerLevel;

	public override void _EnterTree()
	{
		Modulate = Colors.Transparent;

		SteamStatic.AvatarLoaded += LoadPlayerAvatar;
	}

	public override void _ExitTree()
	{
		SteamStatic.AvatarLoaded -= LoadPlayerAvatar;
	}

	public override void _Ready()
	{
		if (!SteamStatic.IsSteamRunning()) return;
		
		var SteamStaticId = SteamStatic.GetSteamID();
		var name = SteamStatic.GetFriendPersonaName(SteamStaticId);
		var level = SteamStatic.GetPlayerSteamLevel();
		
		Modulate = Colors.White;
		
		PlayerName.Text = name;
		PlayerLevel.Text = "Level " + level;
		
		SteamStatic.GetPlayerAvatar();
	}

	private void LoadPlayerAvatar(ulong avatarId, int size, byte[] data)
	{
		var avatarImage = Image.CreateFromData(size, size, false, Image.Format.Rgba8, data);
		
		var stylebox = new StyleBoxTexture();
		stylebox.Texture = ImageTexture.CreateFromImage(avatarImage);
		
		PlayerAvatar.AddThemeStyleboxOverride("panel", stylebox);
	}
}
