using Godot;
using System;

public partial class InteractableProp : Node3D
{
	[Export] private PackedScene InteractionPrompt = GD.Load<PackedScene>("res://Interactables/InteractionPrompt.tscn");
	[Export] public float InteractionRadius = 1.0f;
	[Export] private string animationOnInteract = "PickUp_Table";
	public Node3D owner;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Interactables");
    }

	public virtual bool CanBeInteractedBy(Character instigator)
	{
		if (instigator == owner)
		{
			return false;
		}

		if (Position.DistanceSquaredTo(instigator.Position) > (InteractionRadius * InteractionRadius))
		{
			return false;
		}

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D queryParams = PhysicsRayQueryParameters3D.Create(Position, instigator.Position);
		Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid> { new Rid(this) };
		queryParams.SetExclude(exclude);
		Godot.Collections.Dictionary hit = spaceState.IntersectRay(queryParams);
		if (hit.Count != 0)
		{
			if (hit["collider"].AsGodotObject() is Character charHit)
			{
				return charHit == instigator;
			}
		}

		return false;
	}

	public virtual void OnInteracted(Character instigator)
	{
		owner = instigator;
		instigator.EmitSignal(nameof(OnInteracted), animationOnInteract);
	}
	
	public virtual PackedScene GetInteractionPrompt()
    {
		return InteractionPrompt;
    }
}
