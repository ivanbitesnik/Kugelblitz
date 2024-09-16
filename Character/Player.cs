using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	[Export] private bool IsShootingOn = true;
	[Export] private float DashDuration = 0.1f;
	[Export] private float DashDistance = 50.0f;
	[Export] private float DashCooldown = 2.0f;

	private float DashTimeLeft = 0.0f;
	private float DashCooldownLeft = 0.0f;
	
	public void CalculateVelocity(double delta, Vector2 inputDir, Vector3 direction)
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

		if (DashCooldownLeft > 0.0f)
		{
			DashCooldownLeft = (float)Mathf.Clamp(DashCooldownLeft - delta, 0.0f, DashCooldownLeft);
		}
		else if (DashTimeLeft > 0.0f)
		{
			velocity += ((DashDistance * (float)delta) / DashDuration) * -GetGlobalTransform().Basis.Z;
			DashTimeLeft = (float)Mathf.Clamp(DashTimeLeft - delta, 0.0f, DashTimeLeft);
			if (DashTimeLeft <= 0.0f)
			{
				DashCooldownLeft = DashCooldown;
			}
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
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(delta, inputDir, direction);
		
		MoveAndSlide();
		
		if (Input.IsActionPressed("Fire") && IsShootingOn)
		{
			TryFire();
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			Vector2 screenRelative = eventMouseMotion.GetScreenRelative();
			LookAt(new Vector3(screenRelative[0], Position[1], screenRelative[1]), Vector3.Up);
		}

		if (@event.IsActionPressed("dash"))
		{
			if (DashCooldownLeft <= 0.0f)
			{
				DashTimeLeft = DashDuration;
			}
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
