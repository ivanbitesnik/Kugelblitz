using Godot;
using GodotSteam;
using System;
using System.Runtime.CompilerServices;

public partial class FriendItem : HBoxContainer
{
	public Panel PlayerStatus = new Panel();
	public Button PlayerName = new Button();
	private Friend FriendInfo;

	public void SetFriend(Friend friend)
	{
		FriendInfo = friend;
		SizeFlagsHorizontal = SizeFlags.Fill;
		SizeFlagsVertical = SizeFlags.Fill;

		PlayerName.Text = friend.Name;
		PlayerName.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		PlayerName.ButtonUp += FriendButtonPressed;
		AddChild(PlayerName);

		var stylebox = new StyleBoxFlat();
		stylebox.BgColor = GetStatusColor(friend.Status);
		PlayerStatus.AddThemeStyleboxOverride("panel", stylebox);
		AddChild(PlayerStatus);

		Modulate = Colors.LightGray;
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

	private void FriendButtonPressed()
	{
		SteamStatic.InviteUserToGame(FriendInfo.Id, "");
	}
}
