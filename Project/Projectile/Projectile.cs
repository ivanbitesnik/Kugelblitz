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

		SetPhysicsProcess(true);
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		float distanceToGraviton = Position.DistanceTo(Graviton.Position);
		if (distanceToGraviton <= Graviton.GetInfluenceRange())
		{
			float deltaFloat = 1.0f/60.0f;
			Vector3 pullForce = (Position.DirectionTo(Graviton.Position) * Graviton.GetIntensity()) / distanceToGraviton;
			Vector3 deltaForce = (pullForce / Scale) * new Vector3(deltaFloat, deltaFloat, deltaFloat);
			ApplyForce(deltaForce);
		}
    }

    private void _on_body_entered(Node body)
	{
		GD.Print("Bullet hit: ", body.Name, " Context: ", Multiplayer.GetUniqueId() == 1 ? "Server" : "Client");
		
		var projectile = body as Projectile;
		if (projectile is not null)
		{
			if (bProjectileMerged) return;	// The other projectile merged into this one
			projectile.MergeProjectiles(this);
		}

		// The other player reported the hit first, this one waits to simulate the hit from their pov
		if (bWaitingHitReport)
		{
			QueueFree();
			return;
		}

		if (!IsMultiplayerAuthority()) return;	// We trust the bullet owner to simulate and report the hit first
		
		var player = body as Player;
		if (player is not null)
		{
			if (player.IsParrying())
			{
				return;
			}

			player.ReceiveDamage(Damage);
		}

		bReportedHit = true;
		Rpc(nameof(HandleImpact));
	}

	private void MergeProjectiles(Projectile otherProjectile)
	{
		ProjectileSize += otherProjectile.ProjectileSize / 10.0f;
		Scale = new Vector3(ProjectileSize, ProjectileSize, ProjectileSize);
		foreach (Node3D child in GetChildren())
		{
			child.Scale = Scale;
		}

		bProjectileMerged = true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void HandleImpact()
	{	
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
}