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
	[Export] private float ParryDuration = 0.1f;
	[Export] private float ParryCooldown = 1.0f;
	[Export] Color PlayerOneColor = new Color(1.0f, 0.0f, 0.0f);
	[Export] Color PlayerTwoColor = new Color(0.0f, 0.0f, 1.0f);

	private float DashTimeLeft = 0.0f;
	private float DashCooldownLeft = 0.0f;
	private float ParryTimeLeft = 0.0f;
	private float ParryCooldownLeft = 0.0f;
	
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

		string contextString = Multiplayer.IsServer() ? "Server" : "Client";
		GD.Print("Character spawned, ID: ", Name, ", Context: ", contextString);
	}

    public override void _Ready()
    {
        Position = IsServer() ? GetTree().Root.GetNode<Marker3D>("World/Arena/PlayerOneSpawnPoint").Position : GetTree().Root.GetNode<Marker3D>("World/Arena/PlayerTwoSpawnPoint").Position;

		StandardMaterial3D colorMaterial = new StandardMaterial3D() { AlbedoColor = IsServer() ? PlayerOneColor : PlayerTwoColor };
		GetNode<MeshInstance3D>("PlayerMesh").SetSurfaceOverrideMaterial(0, colorMaterial);

		SetAxisLock(PhysicsServer3D.BodyAxis.LinearY, true);

		SetPhysicsProcess(IsMultiplayerAuthority());
		SetProcessInput(IsMultiplayerAuthority());
    }

    private void CalculateVelocity(double delta, Vector3 direction)
	{
		Vector3 velocity = Velocity;

		ProcessDashing(delta, direction, ref velocity);

		if (!IsDashing())
		{
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
		}

		Velocity = velocity;
	}

	private void ProcessDashing(double delta, Vector3 direction, ref Vector3 velocity)
	{
		if (IsDashing())
		{
			DashTimeLeft = (float)Mathf.Clamp(DashTimeLeft - delta, 0.0f, DashTimeLeft);

			Vector3 dashDirection = (direction == Vector3.Zero ? -GetGlobalTransform().Basis.Z : direction);
			velocity = ((DashDistance * (float)delta) / DashDuration) * dashDirection;
		}

		if (IsDashOnCooldown())
		{
			DashCooldownLeft = (float)Mathf.Clamp(DashCooldownLeft - delta, 0.0f, DashCooldownLeft);
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{	
		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(delta, direction);
		
		MoveAndSlide();

		if (Input.IsActionPressed("Fire") && IsShootingOn)
		{
			TryFire();
		}

		ProcessParry(delta);
	}

	private void ProcessParry(double delta)
	{
		if (IsParrying())
		{
			ParryTimeLeft = (float)Mathf.Clamp(ParryTimeLeft - delta, 0.0f, ParryTimeLeft);
		}
		else if (IsParryOnCooldown())
		{
			ParryCooldownLeft = (float)Mathf.Clamp(ParryCooldownLeft - delta, 0.0f, ParryCooldownLeft);
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
				StartDashing();
			}
		}

		if (@event.IsActionPressed("Parry"))
		{
			if (!IsParryOnCooldown())
			{
				StartParrying();
			}
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

	private void StartDashing()
	{
		DashTimeLeft = DashDuration;
		DashCooldownLeft = DashCooldown;
	}

	private void StartParrying()
	{
		ParryTimeLeft = ParryDuration;
		ParryCooldownLeft = ParryCooldown;
	}

	private void TryFire()
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

	public bool IsDashing()
	{
		return DashTimeLeft > 0.0f;
	}

	public bool IsDashOnCooldown()
	{
		return DashCooldownLeft > 0.0f;
	}

	public bool IsParrying()
	{
		return ParryTimeLeft > 0.0f;
	}

	public bool IsParryOnCooldown()
	{
		return ParryCooldownLeft > 0.0f;
	}
}
