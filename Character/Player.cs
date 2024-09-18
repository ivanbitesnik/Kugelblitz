using Godot;
using System;
using System.Linq;
using System.Reflection.Metadata;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	[Export] private bool IsShootingOn = true;
	[Export] private float DashDuration = 0.1f;
	[Export] private float DashDistance = 50.0f;
	[Export] private float DashCooldown = 1.0f;
	[Export] private Camera3D Camera;

	private float DashTimeLeft = 0.0f;
	private float DashCooldownLeft = 0.0f;
	
	
	private void _EnterTree()
	{
		SetMultiplayerAuthority(Int32.Parse(Name));
	}
	private void CalculateVelocity(double delta, Vector3 direction)
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

		velocity += CalculateDashVelocity(delta, direction);
		Velocity = velocity;
	}

	private Vector3 CalculateDashVelocity(double delta, Vector3 direction)
	{
		Vector3 velocity = Vector3.Zero;

		if (DashCooldownLeft > 0.0f)
		{
			DashCooldownLeft = (float)Mathf.Clamp(DashCooldownLeft - delta, 0.0f, DashCooldownLeft);
		}
		else if (DashTimeLeft > 0.0f)
		{
			Vector3 dashDirection = (direction == Vector3.Zero ? -GetGlobalTransform().Basis.Z : direction);
			velocity = ((DashDistance * (float)delta) / DashDuration) * dashDirection;

			DashTimeLeft = (float)Mathf.Clamp(DashTimeLeft - delta, 0.0f, DashTimeLeft);
			if (DashTimeLeft <= 0.0f)
			{
				DashCooldownLeft = DashCooldown;
			}
		}

		return velocity;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority()) return;
			
		// Add the gravity.
		if (!IsOnFloor())
		{
			Velocity += GetGravity() * (float)delta;
		}
		
		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(delta, direction);
		
		MoveAndSlide();

		if (Input.IsActionPressed("Fire") && IsShootingOn)
		{
			TryFire();
		}

		
	}

	private void LookAtCursor(Vector2 mousePos)
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		Vector3 rayOrigin = Camera.ProjectRayOrigin(mousePos);
		Vector3 rayEnd = rayOrigin + Camera.ProjectRayNormal(mousePos) * 2000.0f;

		PhysicsRayQueryParameters3D queryParams = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		Godot.Collections.Dictionary hit = spaceState.IntersectRay(queryParams);

		if (hit.Count != 0)
		{
			Vector3 hitPos = (Vector3)hit["position"];
			LookAt(new Vector3(hitPos.X, Position.Y, hitPos.Z), Vector3.Up);
		}

	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			LookAtCursor(eventMouseMotion.Position);
		}

		if (@event.IsActionPressed("dash"))
		{
			if (DashCooldownLeft <= 0.0f)
			{
				DashTimeLeft = DashDuration;
			}
		}
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
