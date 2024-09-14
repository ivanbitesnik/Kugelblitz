using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	
	public void GetInput()
	{	
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(inputDir, direction);
		
		if (Input.IsActionPressed("Fire"))
		{
			TryFire();
		}
	}
	
	public void CalculateVelocity(Vector2 inputDir, Vector3 direction)
	{
		Vector3 velocity = Velocity;
		
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// Calculates velocity and deals with any additional actions
		GetInput();
		
		// Add the gravity.
		if (!IsOnFloor())
		{
			Velocity += GetGravity() * (float)delta;
		}
		
		MoveAndSlide();
	}
	
	public void TryFire()
	{
		GetWeapon().TriggerFire();
	}
	
	public Weapon GetWeapon()
	{
		return GetNode<Weapon>("Weapon");
	}
}
