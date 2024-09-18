using Godot;
using System;
using Kugelblitz.GravityHandler;

public partial class Projectile : RigidBody3D
{
	
	public void Initialize(Vector3 position, Vector3 direction, float speed)
	{
		Position = position;
		SetLinearVelocity(new Vector3(direction[0] * speed, 0.0f, direction[2] * speed));
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
		//var attractor = blabla
		
		GravityHandler.AddGravityForce(this);
		
		
	}
}
