using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kugelblitz.GravityHandler;

public class GravityHandler
{
	[Export] public static float Gravity = 9.8f;
	private IEnumerable<RigidBody3D> Attractors;
	private IEnumerable<RigidBody3D> Attractees;


	public static void AddGravityForce(RigidBody3D target)
	{
		var attractorMass = 50000;
		var pos = Vector3.Zero;
		float massProduct = attractorMass * target.Mass;
		
		float distanceSquared = target.Position.DistanceSquaredTo(pos);

		float unScaledforceMagnitude = massProduct / distanceSquared;
		float forceMagnitude = Gravity * unScaledforceMagnitude;
		
		Vector3 forceDirection = target.Position.DirectionTo(pos);
		Vector3 forceVector = forceDirection * forceMagnitude;
		
		//target.AddConstantForce(forceVector);
	}
}
