using Godot;
using GodotSteam;
using System;

public partial class FriendItem : PanelContainer
{
	public HBoxContainer HorizontalContainer = new HBoxContainer();
	public Panel PlayerStatus = new Panel();
	public Label PlayerName = new Label();

	public void SetFriend(Friend friend)
	{
		HorizontalContainer.SetSize(Size);
		HorizontalContainer.SizeFlagsHorizontal = SizeFlags.Fill;
		HorizontalContainer.ClipChildren = ClipChildrenMode.Disabled;
		AddChild(HorizontalContainer);

		PlayerName.SetSize(Size);
		PlayerName.Text = friend.Name;
		PlayerName.HorizontalAlignment = HorizontalAlignment.Left;
		AddChild(PlayerName);

		var stylebox = new StyleBoxFlat();
		stylebox.BgColor = GetStatusColor(friend.Status);
		PlayerStatus.AddThemeStyleboxOverride("panel", stylebox);
		HorizontalContainer.AddChild(PlayerStatus);
	}

	private Color GetStatusColor(PersonaState personaState)
	{
		switch (personaState)
		{
			case PersonaState.Busy:
				return new Color("#872032");
			case PersonaState.Away:
				return new Color("#a6862e");
			case PersonaState.Snooze:
				return new Color("#77b3e0");
			case PersonaState.Online:
			case PersonaState.LookingToTrade:
			case PersonaState.LookingToPlay:
				return new Color("#59d952");
			case PersonaState.Offline:
			case PersonaState.Invisible:
			default:
				return new Color("#444");
		}
	}
}
