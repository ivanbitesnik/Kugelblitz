using Godot;
using System;

public partial class Projectile : RigidBody3D
{
	public const float ProjectileSpeed = 20.0f;
	
	public void Initialize(Vector3 position, Vector3 direction)
	{
		Rotation = direction;
		//Rotation += new Vector3(0.0f, 0.0f, 90.0f);
		Position = position;
		SetLinearVelocity(new Vector3(Rotation[2] * -ProjectileSpeed, 0.0f, Rotation[0] * -ProjectileSpeed));
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
