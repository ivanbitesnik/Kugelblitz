using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	
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
		// Add the gravity.
		if (!IsOnFloor())
		{
			Velocity += GetGravity() * (float)delta;
		}
		
		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(inputDir, direction);
		
		MoveAndSlide();
		
		if (Input.IsActionPressed("Fire"))
		{
			TryFire();
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			Vector2 relative = eventMouseMotion.GetRelative();
			LookAt(new Vector3(relative[0], Position[1], relative[1]), Vector3.Up);
		}
		// Print the size of the viewport.
		//GD.Print("Viewport Resolution is: ", GetViewport().GetVisibleRect().Size);
	}
	
	public void TryFire()
	{
		GetWeapon().PressTrigger();
	}
	
	public Weapon GetWeapon()
	{
		return GetNode<Weapon>("Weapon");
	}
}
