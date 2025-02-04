using Godot;
using System;

public partial class Graviton : RigidBody3D
{
	[Export] private float InfluenceRange = 200.0f;
	[Export] private float Intensity = 10000.0f;

	private Area3D GravityArea = new Area3D();
	private CollisionShape3D GravityShape = new CollisionShape3D();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetAxisLock(PhysicsServer3D.BodyAxis.LinearX, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.LinearY, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.LinearZ, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.AngularX, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.AngularY, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.AngularZ, true);
		SetFreezeMode(FreezeModeEnum.Static);

		GravityArea.AddChild(GravityShape);
		GravityShape.Shape = new SphereShape3D();
		GravityShape.Scale = new Vector3(InfluenceRange, InfluenceRange, InfluenceRange);

		GravityArea.Gravity = Intensity;
		GravityArea.BodyEntered += OnBodyEnteredGravityArea;
		GravityArea.Monitoring = true;
		//AddChild(GravityArea); disabling for now
	}

	private void OnBodyEnteredGravityArea(Node3D body)
	{

	}

	public override void _ExitTree()
	{
		QueueFree();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public float GetInfluenceRange()
	{
		return InfluenceRange;
	}

	public float GetIntensity()
	{
		return Intensity;
	}

}
