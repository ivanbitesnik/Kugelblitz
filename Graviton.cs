using Godot;
using System;
using Kugelblitz.GravityHandler;

public partial class Graviton : RigidBody3D
{
	[Export] private bool isAttractor;
	[Export] private bool isAttractee;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
}
