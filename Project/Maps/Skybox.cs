using Godot;
using System;

public partial class Skybox : Node3D
{
	[Export] public Vector3 BoxScale = new Vector3(50.0f, 50.0f, 50.0f);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Godot.Collections.Array<CollisionShape3D> skyboxParts = CreateSkybox();
		foreach (CollisionShape3D skyboxPart in skyboxParts)
		{
			GetParent().CallDeferred("add_child", skyboxPart);
		}
	}

	private Godot.Collections.Array<CollisionShape3D> CreateSkybox()
	{
		Godot.Collections.Array<CollisionShape3D> res = new Godot.Collections.Array<CollisionShape3D>
        {
            CreateSkyboxPart(new Vector3(Position.X, Position.Y - BoxScale.Y / 2.0f, Position.Z), new Vector3(BoxScale.X, 0.1f, BoxScale.Z)),
            CreateSkyboxPart(new Vector3(Position.X, Position.Y + BoxScale.Y / 2.0f, Position.Z), new Vector3(BoxScale.X, 0.1f, BoxScale.Z)),
            CreateSkyboxPart(new Vector3(Position.X - BoxScale.X / 2.0f, Position.Y, Position.Z), new Vector3(0.1f, BoxScale.Y, BoxScale.Z)),
            CreateSkyboxPart(new Vector3(Position.X + BoxScale.X / 2.0f, Position.Y, Position.Z), new Vector3(0.1f, BoxScale.Y, BoxScale.Z)),
            CreateSkyboxPart(new Vector3(Position.X, Position.Y, Position.Z - BoxScale.Z / 2.0f), new Vector3(BoxScale.X, BoxScale.Y, 0.1f)),
            CreateSkyboxPart(new Vector3(Position.X, Position.Y, Position.Z + BoxScale.Z / 2.0f), new Vector3(BoxScale.X, BoxScale.Y, 0.1f))
        };

		return res;
	}

	private CollisionShape3D CreateSkyboxPart(Vector3 position, Vector3 scale)
	{
		CollisionShape3D skyboxPart = new CollisionShape3D() 
		{
			Shape = new BoxShape3D() { Size = scale },
			Position = position
		};

		return skyboxPart;
	}
}
