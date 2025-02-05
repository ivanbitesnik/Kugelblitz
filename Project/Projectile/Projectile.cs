using Godot;
using System;
using Kugelblitz.Character;
using System.Threading.Tasks;

public partial class Projectile : RigidBody3D
{
	[Export] private float Damage;
	private Color Color;
	private Graviton Graviton;
	public float ProjectileSize = 1.0f;

	private bool bReportedHit = false;
	private bool bWaitingHitReport = false;
	private bool bProjectileMerged = false;
	
	public void Initialize(Vector3 position, Vector3 direction, float speed, Color color)
	{
		Color = color;
		Position = position;
		SetLinearVelocity(new Vector3(direction.X * speed, 0.0f, direction.Z * speed));
	}
	
	public override void _Ready()
	{
		GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Color});
		
		ContactMonitor = true;
		MaxContactsReported = 1;
		
		SetAxisLock(PhysicsServer3D.BodyAxis.LinearY, true);
		Graviton = GetTree().Root.GetNode<Graviton>("World/Arena/Graviton");

		Timer FixedTick = new Timer();
		FixedTick.Timeout += ProcessFixedTick;
		FixedTick.OneShot = false;
		FixedTick.WaitTime = 0.01f;
		FixedTick.Autostart = true;
		AddChild(FixedTick);
	}
	
	private void ProcessFixedTick()
	{
		float distanceToGraviton = Position.DistanceTo(Graviton.Position);
		if (distanceToGraviton <= Graviton.GetInfluenceRange())
		{
			Vector3 pullForce = (Position.DirectionTo(Graviton.Position) * Graviton.GetIntensity()) / distanceToGraviton;
			ApplyForce(pullForce / Scale);
		}
	}

    private void _on_body_entered(Node body)
	{
		// The instigator reported the hit first, the observer awaits to simulate the hit from their pov
		if (bWaitingHitReport)
		{
			QueueFree();
			return;
		}

		if (!IsMultiplayerAuthority()) return;	// We trust the instigator to simulate and report the hit first

		bReportedHit = true;
		Rpc(nameof(HandleImpact), body.GetPath());
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void HandleImpact(string bodyPath)
	{	
		Node body = GetTree().Root.GetNode(bodyPath);
		if (body is null) DebugManager.PrintString(this, "Invalid body path, " + bodyPath, 10.0f);
		

		var projectile = body as Projectile;
		if (projectile is not null)
		{
			if (bProjectileMerged) return;	// The other projectile merged into this one
			projectile.MergeProjectiles(this);
		}
		
		var player = body as Player;
		if (player is not null)
		{
			if (player.IsParrying())
			{
				return;
			}

			player.ReceiveDamage(Damage);
		}

		if (!bReportedHit)
		{
			HandleImpactTask();
			return;
		}

		QueueFree();
	}

	private async Task HandleImpactTask()
	{
		bWaitingHitReport = true;
		await ToSignal(GetTree().CreateTimer(0.01f), SceneTreeTimer.SignalName.Timeout);
		QueueFree();
	}

	private void MergeProjectiles(Projectile otherProjectile)
	{
		ProjectileSize += otherProjectile.ProjectileSize / 10.0f;
		Scale = new Vector3(ProjectileSize, ProjectileSize, ProjectileSize);
		foreach (Node child in GetChildren())
		{
			Node3D child3D = child as Node3D;
			if (child3D is not null) child3D.Scale = Scale;
		}

		bProjectileMerged = true;
	}
}