using Godot;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Kugelblitz.Character
{
	public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	[Export] private float DashDuration = 0.1f;
	[Export] private float DashDistance = 50.0f;
	[Export] private float DashCooldown = 1.0f;
	[Export] private float ParryDuration = 0.1f;
	[Export] private float ParryCooldown = 1.0f;
	[Export] private float DamageReceivedFeedbackDuration = 0.5f;

	public Color Color;
	private float Health = 100.0f;
	
	private float DashTimeLeft = 0.0f;
	private bool bIsDashOnCooldown = false;
	private bool bIsParrying = false;
	private bool bIsParryOnCooldown = false;

	public bool IsServer()
	{
		return GetPlayerId() == 1;
	}

	public int GetPlayerId()
	{
		return int.Parse(Name);
	}

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(GetPlayerId());
		string contextString = Multiplayer.IsServer() ? "Server" : "Client";
		GD.Print("Character spawned, ID: ", Name, ", Context: ", contextString);
		
		Position = IsServer() ? GetTree().Root.GetNode<Marker3D>("World/Arena/PlayerOneSpawnPoint").Position : GetTree().Root.GetNode<Marker3D>("World/Arena/PlayerTwoSpawnPoint").Position;
        
		Color = IsServer() ? Colors.Blue : Colors.Green;
		GetNode<MeshInstance3D>("PlayerMesh")
			.SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Color });
	}

	public override void _ExitTree()
	{
		QueueFree();
	}

    public override void _Ready()
    {
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
	}
	
	public override void _PhysicsProcess(double delta)
	{	
		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(delta, direction);
		
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
				Rpc(nameof(StartParrying));
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

	private async Task StartDashing()
	{
		DashTimeLeft = DashDuration;
		bIsDashOnCooldown = true;
		await ToSignal(GetTree().CreateTimer(DashCooldown), SceneTreeTimer.SignalName.Timeout);
		bIsDashOnCooldown = false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartParrying()
	{
		StartParryTask();
	}

	private async Task StartParryTask()
	{
		bIsParrying = true;
		GetNode<MeshInstance3D>("PlayerMesh")
			.SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Colors.Gray });
		await ToSignal(GetTree().CreateTimer(ParryDuration), SceneTreeTimer.SignalName.Timeout);
		EndParry();
	}

	private async Task EndParry()
	{
		bIsParrying = false;
		GetNode<MeshInstance3D>("PlayerMesh")
					.SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Color });

		bIsParryOnCooldown = true;
		await ToSignal(GetTree().CreateTimer(ParryCooldown), SceneTreeTimer.SignalName.Timeout);
		bIsParryOnCooldown = false;
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
		return bIsDashOnCooldown;
	}

	public bool IsParrying()
	{
		return bIsParrying;
	}

	public bool IsParryOnCooldown()
	{
		return bIsParryOnCooldown;
	}

	public void ReceiveDamage(float damage)
	{
		Health -= damage;

		Rpc(nameof(PlayDamageFeedback), Health <= 0);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void PlayDamageFeedback(bool bIsDeathFeedback)
	{
		if (bIsDeathFeedback)
		{
			QueueFree();
		}
		else
		{
			PlayDamageFeedbackTask();
		}
	}

	private async Task PlayDamageFeedbackTask()
	{
		GetNode<MeshInstance3D>("PlayerMesh")
			.SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Colors.Red });
		await ToSignal(GetTree().CreateTimer(DamageReceivedFeedbackDuration), SceneTreeTimer.SignalName.Timeout);
		GetNode<MeshInstance3D>("PlayerMesh")
			.SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Color });
	}
}

}
