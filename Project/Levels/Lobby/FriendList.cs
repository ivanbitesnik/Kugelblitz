using Godot;
using GodotSteam;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class FriendList : ScrollContainer
{
	public VBoxContainer VerticalContainer = new VBoxContainer();

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
	}

	public void Initialize()
	{
		var friendsList = SteamStatic.GetUserSteamFriends();
		
		friendsList = SortFriendList(friendsList);
		foreach (var friend in friendsList)
		{
			AddFriend(friend);
		}
	}

	public List<Friend> SortFriendList(List<Friend> FriendList)
	{
		List<List<Friend>> FriendStates = new List<List<Friend>>();
		for (int i = 0; i < (int)PersonaState.Max; i++)
		{
			List<Friend> FriendState = new List<Friend>();
			foreach (Friend friend in FriendList)
			{
				if (friend.Status == (PersonaState)i)
				{
					FriendState.Add(friend);
				}
			}

			FriendStates.Add(FriendState);
		}

		FriendList.Clear();
		FriendList.AddRange(FriendStates[(int)PersonaState.Online]);
		FriendList.AddRange(FriendStates[(int)PersonaState.LookingToPlay]);
		FriendList.AddRange(FriendStates[(int)PersonaState.LookingToTrade]);
		FriendList.AddRange(FriendStates[(int)PersonaState.Busy]);
		FriendList.AddRange(FriendStates[(int)PersonaState.Away]);
		FriendList.AddRange(FriendStates[(int)PersonaState.Snooze]);
		FriendList.AddRange(FriendStates[(int)PersonaState.Offline]);
		FriendList.AddRange(FriendStates[(int)PersonaState.Invisible]);

		return FriendList;
	}

	public void ClearList()
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
