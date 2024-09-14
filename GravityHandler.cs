using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kugelblitz.GravityHandler;

public class GravityHandler
{
    [Export] public static float Gravity;
    private IEnumerable<RigidBody3D> Attractors;
    private IEnumerable<RigidBody3D> Attractees;


    public static void AddGravityForce(RigidBody3D attractor, RigidBody3D target)
    {
        float massProduct = attractor.Mass * target.Mass;
        
        Vector3 difference = attractor.Position - target.Position;
        
        //Estos X Y habría que chequear si son asi posta
        //prob x z o algo así
        float distance = Mathf.Sqrt((MathF.Pow(difference.X, 2)) + (MathF.Pow(difference.Y, 2)));
        
        float unScaledforceMagnitude = massProduct / MathF.Pow(distance, 2);
        float forceMagnitude = Gravity * unScaledforceMagnitude;
        
        Vector3 forceDirection = difference.Normalized();
        
        Vector3 forceVector = forceDirection * forceMagnitude;
        
        target.AddConstantForce(forceVector);
    }
}