using Godot;
using GodotSteam;
using System;

public partial class FriendList : ScrollContainer
{
	public VBoxContainer VerticalContainer = new VBoxContainer();
	public float FriendItemHeight;

	public override async void _Ready()
	{
		if (!SteamStatic.IsSteamRunning()) return;

		VerticalScrollMode = ScrollMode.ShowAlways;
		HorizontalScrollMode = ScrollMode.Disabled;
		SizeFlagsVertical = SizeFlags.ExpandFill;
		SizeFlagsHorizontal = SizeFlags.Fill;
		VerticalContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
		VerticalContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		AddChild(VerticalContainer);
		
		var friendsList = SteamStatic.GetUserSteamFriends();

		ClearList();
		
		foreach (var friend in friendsList)
		{
			AddFriend(friend);
		}
	}

	private void ClearList()
	{
		foreach (var child in VerticalContainer.GetChildren())
		{
			child.QueueFree();
		}
	}

	bool colorpick = false;
	private void AddFriend(Friend friend)
	{
		FriendItem newFriendItem = new FriendItem();
		newFriendItem.SetFriend(friend);
		newFriendItem.Modulate = colorpick ? Colors.Yellow : Colors.Blue;
		colorpick = !colorpick;
			
		VerticalContainer.AddChild(newFriendItem);
	}
}
