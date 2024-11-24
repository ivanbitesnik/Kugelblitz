using Godot;
using GodotSteam;
using System;

public partial class FriendList : GridContainer
{
	public VBoxContainer VerticalContainer = new VBoxContainer();
	public PackedScene FriendItem;
	public float FriendItemHeight;

	public override void _Ready()
	{
		if (!SteamStatic.IsSteamRunning()) return;

		VerticalContainer.SetSize(Size);
		VerticalContainer.SizeFlagsHorizontal = SizeFlags.Expand;
		VerticalContainer.ClipChildren = ClipChildrenMode.Disabled;
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

	private void AddFriend(Friend friend)
	{
		int friendLevel = SteamStatic.GetFriendSteamLevel(friend.Id);
		
		FriendItem newFriendItem = new FriendItem();
		newFriendItem.SetSize(new Vector2(Size.X, FriendItemHeight));
		newFriendItem.SetFriend(friend);
			
		VerticalContainer.AddChild(newFriendItem);
	}
}
