using Godot;
using System;

public partial class Projectile : CharacterBody3D
{
	public const float ProjectileSpeed = 20.0f;
	
	public void Initialize(Vector3 position, Vector3 direction)
	{
		Position = position;
		Velocity = new Vector3(direction[0] * ProjectileSpeed, 0.0f, direction[2] * ProjectileSpeed);
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var collision = MoveAndCollide(Velocity * (float)delta);
		if (collision != null)
		{
			Velocity = Velocity.Bounce(collision.GetNormal());
			if (collision.GetCollider().HasMethod("Hit"))
			{
				collision.GetCollider().Call("Hit");
			}
		}
	}
}
