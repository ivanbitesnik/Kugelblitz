using Godot;
using System;

public partial class Graviton : RigidBody3D
{
	[Export] private float InfluenceRange;
	[Export] private float Intensity;
	
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
