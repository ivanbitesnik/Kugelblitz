using Godot;
using System;

public partial class Projectile : RigidBody3D
{
	[Export] private float Intensity;
	
	private Graviton Graviton;

	public void Initialize(Vector3 position, Vector3 direction, float speed)
	{
		Position = position;
		SetLinearVelocity(new Vector3(direction.X * speed, 0.0f, direction.Z * speed));
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetAxisLock(PhysicsServer3D.BodyAxis.LinearY, true);

		Graviton = GetTree().Root.GetNode<Graviton>("World/Arena/Graviton");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float DistanceToGraviton = Position.DistanceTo(Graviton.Position);
		if (DistanceToGraviton <= Graviton.GetInfluenceRange())
		{
			Vector3 PullForce = Position.DirectionTo(Graviton.Position) * Graviton.GetIntensity() * (1.0f / DistanceToGraviton);
			ApplyForce(PullForce);
		}
	}
}