using Godot;
using GodotSteam;
using System;
using System.Collections.Generic;

public partial class LobbyContainer : VBoxContainer
{
	[Export] public string HeaderText = "Lobby List";

	public HBoxContainer HeaderContainer = new HBoxContainer();
	public Button BackToMenuButton = new Button();
	public Button RefreshButton = new Button();
	public ScrollContainer ScrollBox = new ScrollContainer();
	public VBoxContainer LobbyList = new VBoxContainer();

	public override void _Ready()
	{
		AddChild(HeaderContainer);

		BackToMenuButton.SizeFlagsVertical  = SizeFlags.ExpandFill;
		BackToMenuButton.Text = "Back to menu";
		BackToMenuButton.Modulate = Colors.IndianRed;
		HeaderContainer.AddChild(BackToMenuButton);

		RefreshButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		RefreshButton.Text = "Refresh";
		RefreshButton.ButtonUp += OnRefreshPressed;
		RefreshButton.Modulate = Colors.SkyBlue;
		HeaderContainer.AddChild(RefreshButton);

		ScrollBox.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
		ScrollBox.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
		ScrollBox.SizeFlagsVertical = SizeFlags.ExpandFill;
		ScrollBox.SizeFlagsHorizontal = SizeFlags.Fill;
		ScrollBox.SizeFlagsStretchRatio = 4.0f;
		AddChild(ScrollBox);

		LobbyList.SizeFlagsVertical = SizeFlags.ExpandFill;
		LobbyList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		ScrollBox.AddChild(LobbyList);
	}

	private void OnRefreshPressed()
	{
		ClearLobbies();

		SteamStatic.RequestLobbyList();
	}

	public void ClearLobbies()
	{
		foreach (Node node in LobbyList.GetChildren())
		{
			LobbyList.RemoveChild(node);
		}
	}

	public void SetLobbies(Godot.Collections.Array lobbies)
	{
		for (int i = 0; i < lobbies.Count; i++)
		{
			LobbyButton Button = new LobbyButton();
			string lobbyName = SteamStatic.GetLobbyData((ulong)lobbies[i], "name");
			lobbyName = lobbyName.Length == 0 ? SteamStatic.GetLobbyData((ulong)lobbies[i], "Name") : lobbyName;
			if (lobbyName.Length == 0) continue;
			Button.Text = lobbyName;
			Button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			Button.LobbyId = (ulong)lobbies[i];

			LobbyList.AddChild(Button);
		}
	}
}

public partial class LobbyButton : Button
{
	public ulong LobbyId;

    public override void _Ready()
    {
        ButtonUp += OnLobbyButtonPressed;
    }

    public void OnLobbyButtonPressed()
	{
		SteamStatic.JoinLobby(LobbyId);
	}
}