using Godot;
using System;

public partial class OptionsMenu : Control
{
	private World worldNode;

	public override void _Ready()
	{
		worldNode = GetParent<World>();
	}

	public void _on_return_to_menu_button_up()
	{
		if (Multiplayer.IsServer())
		{
			foreach (int peer in Multiplayer.GetPeers())
			{
				worldNode.OnPeerReturnedToMenu(peer);
			}

			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
			worldNode.TravelToMenu();
		}
		else
		{
			worldNode.Rpc(nameof(World.OnPeerReturnedToMenu), Multiplayer.MultiplayerPeer.GetUniqueId());
		}
	}

	public void _on_exit_game_button_up()
	{
		GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
	}
}
