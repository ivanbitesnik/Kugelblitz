using Godot;
using System;
using Kugelblitz.GravityHandler;

public partial class Graviton : RigidBody3D
{
	[Export] private bool isAttractor;
	[Export] private bool isAttractee;

	[Export] private float InfluenceRange;

	[Export] private float Intensity;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetAxisLock(PhysicsServer3D.BodyAxis.AngularX, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.AngularY, true);
		SetAxisLock(PhysicsServer3D.BodyAxis.AngularZ, true);
		SetFreezeMode(FreezeModeEnum.Kinematic);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GD.Print("delta: " + delta);
		GD.Print(Mass);
		GD.Print(Position);
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
