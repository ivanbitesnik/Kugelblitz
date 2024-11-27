using Godot;
using GodotSteam;
using System;
using System.Numerics;

public partial class PlayerInfo : PanelContainer
{
	public HBoxContainer HorizontalContainer = new HBoxContainer();
	public Panel PlayerAvatar = new Panel();
	public Label PlayerName = new Label();

	public override void _EnterTree()
	{
		SteamStatic.AvatarLoaded += LoadPlayerAvatar;
	}

	public override void _ExitTree()
	{
		SteamStatic.AvatarLoaded -= LoadPlayerAvatar;
	}

	public override void _Ready()
	{
		AddChild(HorizontalContainer);

		PlayerName.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		if (SteamStatic.IsSteamRunning())
		{
			SteamStatic.GetPlayerAvatar();
		}
		else
		{
			PlayerName.Text = "Steam is not running";
			PlayerName.HorizontalAlignment = HorizontalAlignment.Center;
			HorizontalContainer.AddChild(PlayerName);

			Modulate = Colors.Red;
		}

	}

	private void LoadPlayerAvatar(ulong avatarId, int size, byte[] data)
	{
		var avatarImage = Image.CreateFromData(size, size, false, Image.Format.Rgba8, data);
		
		var stylebox = new StyleBoxTexture();
		stylebox.Texture = ImageTexture.CreateFromImage(avatarImage);
		
		PlayerAvatar.AddThemeStyleboxOverride("panel", stylebox);
		PlayerAvatar.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		HorizontalContainer.AddChild(PlayerAvatar);

		var SteamId = SteamStatic.GetSteamID();
		var name = SteamStatic.GetFriendPersonaName(SteamId);
		
		PlayerName.HorizontalAlignment = HorizontalAlignment.Center;
		PlayerName.Text = name;
		PlayerName.SizeFlagsStretchRatio = 2.0f;
		HorizontalContainer.AddChild(PlayerName);
	}
}
