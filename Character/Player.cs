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
	[Export] Color PlayerOneColor = new Color(1.0f, 0.0f, 0.0f);
	[Export] Color PlayerTwoColor = new Color(0.0f, 0.0f, 1.0f);

	private float DashTimeLeft = 0.0f;
	private float DashCooldownLeft = 0.0f;
	
	public int GetId()
	{
		return int.Parse(Name);
	}

	public bool IsServer()
	{
		return GetId() == 1;
	}

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(GetId());
		string contextString = IsServer() ? "Server" : "Client";
		GD.Print("Character spawned, ID: ", Name, ", Context: ", contextString);
	}

    public override void _Ready()
    {
        Position = IsServer() ? GetTree().Root.GetNode<Marker3D>("World/Arena/PlayerOneSpawnPoint").Position : GetTree().Root.GetNode<Marker3D>("World/Arena/PlayerTwoSpawnPoint").Position;

		StandardMaterial3D colorMaterial = new StandardMaterial3D() { AlbedoColor = IsServer() ? PlayerOneColor : PlayerTwoColor };
		GetNode<MeshInstance3D>("PlayerMesh").SetSurfaceOverrideMaterial(0, colorMaterial);

		SetAxisLock(PhysicsServer3D.BodyAxis.LinearY, true);
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

		CalculateDashVelocity(delta, ref velocity);
		Velocity = velocity;
	}

	private void CalculateDashVelocity(double delta, ref Vector3 velocity)
	{
		if (IsDashOnCooldown())
		{
			DashCooldownLeft = (float)Mathf.Clamp(DashCooldownLeft - delta, 0.0f, DashCooldownLeft);
		}
		else if (IsDashing())
		{
			Vector3 dashDirection = (velocity == Vector3.Zero ? -GetGlobalTransform().Basis.Z : velocity.Normalized());
			velocity += ((DashDistance * (float)delta) / DashDuration) * dashDirection;

			DashTimeLeft = (float)Mathf.Clamp(DashTimeLeft - delta, 0.0f, DashTimeLeft);
			if (DashTimeLeft <= 0.0f)
			{
				DashCooldownLeft = DashCooldown;
			}
		}
	}

	public bool IsDashing()
	{
		return DashTimeLeft > 0.0f;
	}

	public bool IsDashOnCooldown()
	{
		return DashCooldownLeft > 0.0f;
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

		Camera3D camera = GetCamera();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePos) * 2000.0f;

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
			if (!IsDashOnCooldown())
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

	public Camera3D GetCamera()
	{
		return GetTree().Root.GetNode<Camera3D>("World/Camera");
	}
}
