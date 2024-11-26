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
		HorizontalContainer.SizeFlagsHorizontal = SizeFlags.Fill;
		AddChild(HorizontalContainer);

		PlayerName.SizeFlagsHorizontal = SizeFlags.Expand;
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
		PlayerAvatar.SizeFlagsHorizontal = SizeFlags.Fill;
		HorizontalContainer.AddChild(PlayerAvatar);

		Modulate = Colors.Green;

		var SteamId = SteamStatic.GetSteamID();
		var name = SteamStatic.GetFriendPersonaName(SteamId);
		
		PlayerName.HorizontalAlignment = HorizontalAlignment.Left;
		PlayerName.Text = name;
		HorizontalContainer.AddChild(PlayerName);
	}
}
