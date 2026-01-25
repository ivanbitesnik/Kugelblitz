using Godot;
using System;

public partial class CharacterAnimator : AnimationPlayer
{
	[Export] private string IdleAnimation = "Idle";
	[Export] private string WalkAnimation = "Walk";
	[Export] private string SprintAnimation = "Sprint";
	[Export] private string CrouchAnimation = "Crouch_Idle";
	[Export] private string CrouchMoveAnimation = "Crouch_Fwd";
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

	private void SetAnimation(string animation)
    {
        if (CurrentAnimation != animation)
        {
			CurrentAnimation = animation;
        }
    }

	private void OnAnimationFinished(StringName animation)
	{
		if (animation == JumpStartAnimation)
		{
			SetAnimation(FallAnimation);
		}
		else if (animation == JumpLandAnimation)
        {
			bIsJumping = false;
        }
	}
	
	private void OnCharacterJumped()
	{
		SetAnimation(JumpStartAnimation);
		bIsJumping = true;
	}
	
	private void OnCharacterLanded()
    {
		SetAnimation(JumpLandAnimation);
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
			SetAnimation(FallAnimation);
			return;
        }

		if (character.Velocity.X == 0.0f && character.Velocity.Z == 0.0f)
		{
			if (Input.IsActionPressed("Crouch"))
            {
				SetAnimation(CrouchAnimation);
            }
			else
			{
				SetAnimation(IdleAnimation);
			}
		}
		else
		{
			if (Input.IsActionPressed("Crouch"))
			{
				SetAnimation(CrouchMoveAnimation);
            }
			else if (Input.IsActionPressed("Sprint") && Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown").Y < 0.0f)
			{
				SetAnimation(SprintAnimation);
            }
			else
			{
				SetAnimation(WalkAnimation);
			}
		}
    }
}
