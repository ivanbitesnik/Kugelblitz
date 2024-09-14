using Godot;
using System;

public partial class Weapon : StaticBody3D
{
	private PackedScene projectileScene = GD.Load<PackedScene>("res://Projectile/projectile.tscn");
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void TriggerFire()
	{
		Projectile projectile = (Projectile)projectileScene.Instantiate();
		projectile.Initialize(GetNode<Marker3D>("Muzzle").GlobalPosition, Rotation);
		GetTree().Root.AddChild(projectile);
	}
}
