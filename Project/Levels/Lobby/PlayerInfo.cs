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
		Modulate = Colors.Transparent;

		SteamStatic.AvatarLoaded += LoadPlayerAvatar;
	}

	public override void _ExitTree()
	{
		SteamStatic.AvatarLoaded -= LoadPlayerAvatar;
	}

	public override void _Ready()
	{
		HorizontalContainer.SetSize(Size);
		HorizontalContainer.SizeFlagsHorizontal = SizeFlags.Fill;
		HorizontalContainer.ClipChildren = ClipChildrenMode.Disabled;
		AddChild(HorizontalContainer);

		PlayerName.SetSize(Size);
		if (SteamStatic.IsSteamRunning())
		{
			var SteamStaticId = SteamStatic.GetSteamID();
			var name = SteamStatic.GetFriendPersonaName(SteamStaticId);

			Modulate = Colors.Green;
			
			PlayerName.SetSize(Size);
			PlayerName.HorizontalAlignment = HorizontalAlignment.Left;
			PlayerName.Text = name;
			
			SteamStatic.GetPlayerAvatar();
		}
		else
		{
			PlayerName.Text = "Steam is not running";
			PlayerName.HorizontalAlignment = HorizontalAlignment.Center;
			Modulate = Colors.Red;
		}

		HorizontalContainer.AddChild(PlayerName);
	}

	private void LoadPlayerAvatar(ulong avatarId, int size, byte[] data)
	{
		var avatarImage = Image.CreateFromData(size, size, false, Image.Format.Rgba8, data);
		
		var stylebox = new StyleBoxTexture();
		stylebox.Texture = ImageTexture.CreateFromImage(avatarImage);
		
		PlayerAvatar.AddThemeStyleboxOverride("panel", stylebox);
		PlayerAvatar.SetSize(new Godot.Vector2(20.0f, 20.0f));

		HorizontalContainer.AddChild(PlayerAvatar);
	}
}
