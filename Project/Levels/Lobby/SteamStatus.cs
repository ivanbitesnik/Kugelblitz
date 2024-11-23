using Godot;
using GodotSteam;
using System;

public partial class SteamStatus : PanelContainer
{
	[Export] public Label Status;

	public override void _EnterTree()
	{
		Modulate = Colors.Transparent;
	}

	public override void _Ready()
	{
		bool bSteamRunning = SteamStatic.IsSteamRunning();
		
		Modulate = bSteamRunning ? Colors.Green : Colors.Red;
		//Status.Text = bSteamRunning ? "Steam is running" : "Steam is not running";
	}
}
