using Godot;
using GodotSteam;
using System;

public partial class FriendList : GridContainer
{
	public PackedScene FriendItem;
	public float FriendItemHeight;

	public override void _Ready()
	{
		if (!SteamStatic.IsSteamRunning()) return;
		
		var friendsList = SteamStatic.GetUserSteamFriends();

		ClearList();
		
		foreach (var friend in friendsList)
		{
			DebugManager.PrintString(this, friend.Name, 10.0f);
			AddFriend(friend);
		}
	}

	private void ClearList()
	{
		foreach (var child in GetChildren())
		{
			child.QueueFree();
		}
	}

	private void AddFriend(Friend friend)
	{
		int friendLevel = SteamStatic.GetFriendSteamLevel(friend.Id);
		
		FriendItem newFriendItem = FriendItem.Instantiate<FriendItem>();
		newFriendItem.SetSize(new Vector2(Size.X, FriendItemHeight));
		newFriendItem.SetFriendAndLevel(friend, friendLevel);
			
		AddChild(newFriendItem);
	}
}
