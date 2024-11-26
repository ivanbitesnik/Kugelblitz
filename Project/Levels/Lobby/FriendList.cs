using Godot;
using GodotSteam;
using System;
using System.Collections;

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
		
		friendsList.Sort(CompareFriends);
		foreach (var friend in friendsList)
		{
			AddFriend(friend);
		}
	}

	public static int CompareFriends(Friend a, Friend b)
	{
		return (int)a.Status;
	}

	private void ClearList()
	{
		foreach (var child in VerticalContainer.GetChildren())
		{
			child.QueueFree();
		}
	}

	private void AddFriend(Friend friend)
	{
		FriendItem newFriendItem = new FriendItem();
		newFriendItem.SetFriend(friend);
			
		VerticalContainer.AddChild(newFriendItem);
	}
}
