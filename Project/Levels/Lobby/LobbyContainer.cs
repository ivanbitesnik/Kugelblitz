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
}
