using Godot;
using System;

public partial class CharacterAnimator : AnimationPlayer
{
	[Export] private string IdleAnimation = "Idle";
	[Export] private string WalkAnimation = "Walk";
	[Export] private string SprintAnimation = "Sprint";
	[Export] private string JumpStartAnimation = "Jump_Start";
	[Export] private string FallAnimation = "Jump";
	[Export] private string JumpLandAnimation = "Jump_Land";

	private Character character;
	private bool bIsJumping = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		character = GetParent<Character>();
		character.OnJump += OnCharacterJumped;
		character.OnLanded += OnCharacterLanded;
		AnimationFinished += OnAnimationFinished;
	}

	private void OnAnimationFinished(StringName animation)
	{
		if (animation == JumpStartAnimation)
		{
			CurrentAnimation = FallAnimation;
		}
		else if (animation == JumpLandAnimation)
        {
			bIsJumping = false;
        }
	}
	
	private void OnCharacterJumped()
	{
		CurrentAnimation = JumpStartAnimation;
		bIsJumping = true;
	}
	
	private void OnCharacterLanded()
    {
		CurrentAnimation = JumpLandAnimation;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MovementAnimationUpdate();
	}
	
	private void MovementAnimationUpdate()
	{
		if (bIsJumping) return;

		if (character.Velocity.Y < 0.0f)
        {
			CurrentAnimation = FallAnimation;
			return;
        }

		if (character.Velocity.X == 0.0f && character.Velocity.Z == 0.0f)
		{
			if (CurrentAnimation != IdleAnimation)
			{
				CurrentAnimation = IdleAnimation;
			}
		}
		else
		{
			if (Input.IsActionPressed("Sprint"))
			{
				if (CurrentAnimation != SprintAnimation)
                {
					CurrentAnimation = SprintAnimation;
                }
            }
			else if (CurrentAnimation != WalkAnimation)
			{
				CurrentAnimation = WalkAnimation;
			}
		}
    }
}
