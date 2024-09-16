using Godot;
using System;

public partial class Connection : Node
{
	private ENetMultiplayerPeer peer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_host_pressed()
	{
		GD.Print("Host pressed");	
	}

	private void _on_join_pressed()
	{
		GD.Print("Join pressed");	
		
	}
}
