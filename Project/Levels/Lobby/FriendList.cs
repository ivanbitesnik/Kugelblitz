using Godot;
using GodotSteam;
using System;

public partial class FriendList : GridContainer
{
	[Export] public PackedScene FriendItem;

	public override void _Ready()
	{
		if (!SteamStatic.IsSteamRunning()) return;
		
		var friendsList = SteamStatic.GetUserSteamFriends();

		ClearList();
		
		foreach (var friend in friendsList)
		{
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
		newFriendItem.SetFriendAndLevel(friend, friendLevel);
			
		AddChild(newFriendItem);
	}
}
