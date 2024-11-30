using Godot;
using GodotSteam;
using System;
using System.Collections.Generic;

public partial class LobbyContainer : VBoxContainer
{
	[Export] public string HeaderText = "Lobby List";

	public Label ListHeader = new Label();
	public ScrollContainer ScrollBox = new ScrollContainer();
	public VBoxContainer LobbyList = new VBoxContainer();

	public override void _Ready()
	{
		ListHeader.SizeFlagsVertical = SizeFlags.ExpandFill;
		ListHeader.Text = HeaderText;
		ListHeader.HorizontalAlignment = HorizontalAlignment.Center;
		ListHeader.Modulate = Colors.Green;
		AddChild(ListHeader);

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
			Button.Lobby = lobbies[i];

			LobbyList.AddChild(Button);
		}
	}
}

public partial class LobbyButton : Button
{
	public Godot.Variant Lobby;

    public override void _Ready()
    {
        ButtonUp += OnLobbyButtonPressed;
    }

    public void OnLobbyButtonPressed()
	{
		SteamStatic.JoinLobby(Lobby.AsUInt32());
	}
}