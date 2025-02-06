using Godot;
using System;

public partial class Graviton : RigidBody3D
{
	[Export] private float InfluenceRange = 100.0f;
	[Export] private float Intensity = 9.8f;

    // The distance at which the gravity strength is equal to Intensity.
	[Export] private float PointUnitDistance = 50.0f;

	[Export] private float ProximityInfluenceRange = 10.0f;
	[Export] private float ProximityIntensity = 5.0f;
	[Export] private float ProximityPointUnitDistance = 25.0f;
	
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

		Area3D GravityArea = FindChild("GravityArea") as Area3D;
		if (GravityArea is not null)
		{
			GravityArea.GravitySpaceOverride = Area3D.SpaceOverride.Replace;
			GravityArea.GravityPoint = true;
			GravityArea.GravityPointUnitDistance = PointUnitDistance;
			GravityArea.Gravity = Intensity;
			GravityArea.GravityPointCenter = Position;
			(GravityArea.FindChild("CollisionShape3D") as CollisionShape3D).Scale = new Vector3(InfluenceRange, InfluenceRange, InfluenceRange);
		}
		Area3D ProximityGravityArea = FindChild("ProximityGravityArea") as Area3D;
		if (ProximityGravityArea is not null)
		{
			ProximityGravityArea.GravitySpaceOverride = Area3D.SpaceOverride.Replace;
			ProximityGravityArea.GravityPoint = true;
			ProximityGravityArea.GravityPointUnitDistance = ProximityPointUnitDistance;
			ProximityGravityArea.Gravity = ProximityIntensity;
			ProximityGravityArea.GravityPointCenter = Position;
			(ProximityGravityArea.FindChild("CollisionShape3D") as CollisionShape3D).Scale = new Vector3(ProximityInfluenceRange, ProximityInfluenceRange, ProximityInfluenceRange);
		}
	}

	public override void _ExitTree()
	{
		QueueFree();
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
