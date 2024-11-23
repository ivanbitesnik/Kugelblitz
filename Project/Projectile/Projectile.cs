using Godot;
using System;
using Kugelblitz.Character;

public partial class Projectile : RigidBody3D
{
	[Export] private float Intensity;
	private Color Color;
	[Export] private float Damage;
	private Graviton Graviton;
	
	public void Initialize(Vector3 position, Vector3 direction, float speed, Color color)
	{
		Color = color;
		Position = position;
		Damage = 25.0f;
		SetLinearVelocity(new Vector3(direction.X * speed, 0.0f, direction.Z * speed));
	}
	
	public override void _Ready()
	{
		SetMultiplayerAuthority(1, false);

		GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, new StandardMaterial3D() { AlbedoColor = Color});
		
		ContactMonitor = true;
		MaxContactsReported = 1;
		
		SetAxisLock(PhysicsServer3D.BodyAxis.LinearY, true);
		Graviton = GetTree().Root.GetNode<Graviton>("World/Arena/Graviton");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float distanceToGraviton = Position.DistanceTo(Graviton.Position);
		if (distanceToGraviton <= Graviton.GetInfluenceRange())
		{
			Vector3 pullForce = Position.DirectionTo(Graviton.Position) * Graviton.GetIntensity() * (1.0f / distanceToGraviton);
			ApplyForce(pullForce);
		}
	}

	private void _on_body_entered(Node body)
	{
		if (!IsMultiplayerAuthority()) return;
		
		GD.Print("Bullet hit: ", body.Name, " Context: ", Multiplayer.GetUniqueId() == 1 ? "Server" : "Client");
		
		var player = body as Player;
		if (player is not null)
		{
			if (player.IsParrying())
			{
				return;
			}

			player.DealDamage(Damage);
		}

		Rpc(nameof(HandleImpact));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void HandleImpact()
	{	
		QueueFree();
	}
}