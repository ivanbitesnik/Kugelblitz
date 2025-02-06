using Godot;
using System;
using Kugelblitz.Character;
using System.Threading.Tasks;

public partial class Projectile : RigidBody3D
{
	[Export] private float Damage;
	private Color Color;
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

		AxisLockLinearY = true;
	}

    private void _on_body_entered(Node body)
	{
		if (IsMultiplayerAuthority())
		{
			HandleImpact(body);
		}
	}

	private void HandleImpact(Node otherBody)
	{
		var projectile = otherBody as Projectile;
		if (projectile is not null)
		{
			if (bProjectileMerged) return;	// The other projectile merged into this one
			projectile.Rpc(nameof(MergeProjectiles), ProjectileSize);
			return;
		}
		
		var player = otherBody as Player;
		if (player is not null)
		{
			if (player.IsParrying()) return;
			Rpc(nameof(ApplyDamage), player.GetPath());
			return;
		}

		Rpc(nameof(DestroyProjectile));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void MergeProjectiles(float otherProjectileSize)
	{
		ProjectileSize += otherProjectileSize / 5.0f;
		Scale = new Vector3(ProjectileSize, ProjectileSize, ProjectileSize);
		foreach (Node child in GetChildren())
		{
			Node3D child3D = child as Node3D;
			if (child3D is not null) child3D.Scale = Scale;
		}

		bProjectileMerged = true;

		DestroyProjectile();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ApplyDamage(string playerPath)
	{
		Player player = GetTree().Root.GetNode<Player>(playerPath);
		player.ReceiveDamage(Damage);

		DestroyProjectile();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DestroyProjectile()
	{
		Freeze = true;
		QueueFree();
	}
}