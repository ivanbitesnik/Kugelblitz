using Godot;
using GodotSteam;
using System;

public partial class SteamStatus : PanelContainer
{
	[Export] public Label Status = new Label();

	public override void _EnterTree()
	{
		Modulate = Colors.Transparent;
	}

	public override void _Ready()
	{
		bool bSteamRunning = SteamStatic.IsSteamRunning();
		
		Modulate = bSteamRunning ? Colors.Green : Colors.Red;

		Status.SetSize(Size);
		Status.HorizontalAlignment = HorizontalAlignment.Center;
		Status.Text = bSteamRunning ? "Steam is running" : "Steam is not running";
		AddChild(Status);
	}
}
