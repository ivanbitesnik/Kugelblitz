using Godot;
using System;
using Kugelblitz.Character;

public partial class Projectile : RigidBody3D
{
	[Export] private float Intensity;
	private Color Color;
	
	private Graviton Graviton;
	
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
		MaxContactsReported = 1; //Bullet should be deleted on first collision??? How does parry work
		
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
		GD.Print(body.Name);
		QueueFree();
	}
	
}