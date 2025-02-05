using Godot;
using System;

public partial class OptionsMenu : Control
{
	private PackedScene MenuScene = GD.Load<PackedScene>("res://Levels/Lobby/menu.tscn");

	public void _on_return_to_menu_button_up()
	{
		Multiplayer.MultiplayerPeer.Close();
		Multiplayer.MultiplayerPeer = null;
		GetTree().ChangeSceneToPacked(MenuScene);
	}

	public void _on_exit_game_button_up()
	{
		GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
	}
}
