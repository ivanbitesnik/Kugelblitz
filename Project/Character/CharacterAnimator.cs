using Godot;
using System;
using System.Diagnostics;
using System.Numerics;

public partial class CharacterAnimator : AnimationPlayer
{
	[Export] private float LightHitStopTime = 0.1f;
	[Export] private float HeavyHitStopTime = 0.1f;
	[Export] private string LocomotionAnimation = "parameters/BaseLocomotion/conditions/Locomotion";
	[Export] private string CrouchLocomotionAnimation = "parameters/BaseLocomotion/conditions/CrouchLocomotion";
	[Export] private string JumpStartAnimation = "parameters/BaseLocomotion/conditions/Jumping";
	[Export] private string FallAnimation = "parameters/BaseLocomotion/conditions/Falling";
	[Export] private string LandingAnimation = "parameters/BaseLocomotion/conditions/Landing";
	[Export] private string GenericInteractionAnimation = "parameters/BaseLocomotion/conditions/Interacting";
	[Export] private string StateMachinePath = "parameters/BaseLocomotion/playback";
	[Export] private string BaseLocomotionBlend = "parameters/BaseLocomotion/Locomotion/blend_position";
	[Export] private string CrouchLocomotionBlend = "parameters/BaseLocomotion/CrouchLocomotion/blend_position";
	[Export] private string LightAttackRequest = "parameters/JabOneShot/request";
	[Export] private string LightAttackActive = "parameters/JabOneShot/active";
	[Export] private string LightAttackHitStop = "parameters/LightAttackHitStop/scale";
	[Export] private string LightAttackTimeSeek = "parameters/LightAttackTimeSeek/seek_request";
	[Export] private string HeavyAttackRequest = "parameters/CrossOneShot/request";
	[Export] private string HeavyAttackActive = "parameters/CrossOneShot/active";
	[Export] private string HeavyAttackHitStop = "parameters/HeavyAttackHitStop/scale";
	[Export] private string HeavyAttackTimeSeek = "parameters/HeavyAttackTimeSeek/seek_request";
	[Export] private string HitChestRequest = "parameters/HitChestOneShot/request";

	[Export] private string CurrentAnim = "";
	[Export] private float TargetLocomotionSpeed = 0.0f;
	private float CurrentLocomotionSpeed = 0.0f;
	private float CurrentHitStopTime = 0.0f;
	private string SimulatedAnim = "";
	private bool bWaitingForLanding = false;
	private bool bLightHitStopOn = false;
	private bool bHeavyHitStopOn = false;
	private bool bLightAttackFadingOut = false;
	private bool bHeavyAttackFadingOut = false;

	private Character character;
	private AnimationTree AnimTree;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		character = GetParent<Character>();
		SetMultiplayerAuthority(int.Parse(character.Name));

		AnimTree = FindChild("AnimationTree") as AnimationTree;
		CurrentAnim = LocomotionAnimation;
		SimulatedAnim = CurrentAnim;

		character.OnJump += OnCharacterJumped;
		character.OnLanded += OnCharacterLanded;
		character.OnHitReceived += OnCharacterHitReceived;
		if (character is PlayableCharacter playableChar)
		{
			playableChar.OnInteracted += OnCharacterInteracted;
			playableChar.OnLightAttack += OnLightAttackTriggered;
			playableChar.OnHeavyAttack += OnHeavyAttackTriggered;
			playableChar.OnLightAttackHit += OnLightAttackHitLanded;
			playableChar.OnHeavyAttackHit += OnHeavyAttackHitLanded;
		}
	}

	private void SetAnimation(string animation)
	{
		if (CurrentAnim == animation)
		{
			return;
		}

		AnimTree.Set(CurrentAnim, false);
		CurrentAnim = animation;
		AnimTree.Set(CurrentAnim, true);
	}

	private void OnCharacterJumped()
    {
        SetAnimation(JumpStartAnimation);
    }

	private void OnCharacterLanded()
    {
		// For some reason looping animations are not ending by themselves, so we force a jump in the state machine
		AnimationNodeStateMachinePlayback stateMachine = (AnimationNodeStateMachinePlayback)AnimTree.Get(StateMachinePath);
		stateMachine.Travel("Landing");
		SetAnimation(LandingAnimation);	// For replication
		bWaitingForLanding = false;
    }
	
	private void OnCharacterInteracted(string interactionAnimation)
	{
		SetAnimation(interactionAnimation);
    }

	public override void _Process(double delta)
	{
		if (CurrentLocomotionSpeed != TargetLocomotionSpeed)
		{
			UpdateLocomotionSpeed(delta);
		}

		if (bLightHitStopOn || bHeavyHitStopOn)
		{
			UpdateHitStop(delta);
		}

		if (!IsMultiplayerAuthority())
		{
			SimulatedClientTick(delta);
			return;
		}

		if (!character.IsOnFloor())
		{
			if (character.Velocity.Y < 0.0f)
			{
				SetAnimation(FallAnimation);
				bWaitingForLanding = true;
			}
			return;
		}

		if (!bWaitingForLanding)
		{
			SetAnimation(ResolveLocomotionAnimation());
		}
	}

	private void SimulatedClientTick(double delta)
	{
		// Update the simulated client's animation whenever it replicates
		if (CurrentAnim != SimulatedAnim)
		{
			AnimTree.Set(SimulatedAnim, false);
			AnimTree.Set(CurrentAnim, true);
			SimulatedAnim = CurrentAnim;

			if (CurrentAnim == LandingAnimation)
			{
				if (!bWaitingForLanding)
				{
					AnimationNodeStateMachinePlayback stateMachine = (AnimationNodeStateMachinePlayback)AnimTree.Get(StateMachinePath);
					stateMachine.Travel("Landing");
					bWaitingForLanding = true;
				}
			}
			else
			{
				bWaitingForLanding = false;
			}
		}
	}

	private void UpdateLocomotionSpeed(double delta)
	{
		CurrentLocomotionSpeed = Mathf.MoveToward(CurrentLocomotionSpeed, TargetLocomotionSpeed, (float)delta * 3.0f);
		AnimTree.Set(BaseLocomotionBlend, CurrentLocomotionSpeed);
		AnimTree.Set(CrouchLocomotionBlend, CurrentLocomotionSpeed);
	}

	private string ResolveLocomotionAnimation()
	{
		bool bIsCrouchPressed = Input.IsActionPressed("Crouch");
		if (character is PlayableCharacter playableChar)
		{
			Godot.Vector2 InputVector = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
			float speed = playableChar.GetSpeed(InputVector);
			if (InputVector.Y > 0.0f)
			{
				speed *= -1.0f;
			}
			float UpperBound = bIsCrouchPressed ? playableChar.GetCrouchSpeed() : playableChar.GetSprintSpeed();
			TargetLocomotionSpeed = speed / UpperBound;
		}
		else
		{
			TargetLocomotionSpeed = Character.Speed;
		}

		return bIsCrouchPressed ? CrouchLocomotionAnimation : LocomotionAnimation;
	}

	public bool IsLightAttackActive()
	{
		return (bool)AnimTree.Get(LightAttackActive) && !bLightAttackFadingOut;
	}

	public bool IsHeavyAttackActive()
	{
		return (bool)AnimTree.Get(HeavyAttackActive) && !bHeavyAttackFadingOut;
	}

	private void OnLightAttackTriggered()
	{
		if (IsHeavyAttackActive())
		{
			AnimTree.Set(HeavyAttackRequest, (int)AnimationNodeOneShot.OneShotRequest.Abort);
		}

		if (bLightAttackFadingOut)
		{
			// TODO: Get the time we should start the animation at to match frames somewhat accurately
			AnimTree.Set(LightAttackTimeSeek, 0.0f);
		}
		else
		{
			AnimTree.Set(LightAttackRequest, (int)AnimationNodeOneShot.OneShotRequest.Fire);
		}

		bLightAttackFadingOut = false;
	}

	private void OnHeavyAttackTriggered()
	{
		if (IsLightAttackActive())
		{
			AnimTree.Set(LightAttackRequest, (int)AnimationNodeOneShot.OneShotRequest.Abort);
		}

		if (bHeavyAttackFadingOut)
		{
			// TODO: Get the time we should start the animation at to match frames somewhat accurately
			AnimTree.Set(HeavyAttackTimeSeek, 0.0f);
		}
		else
		{
			AnimTree.Set(HeavyAttackRequest, (int)AnimationNodeOneShot.OneShotRequest.Fire);
		}

		bHeavyAttackFadingOut = false;
	}

	private void OnLightAttackHitLanded(Node3D ObjectHit)
	{
		AnimTree.Set(LightAttackHitStop, 0.0f);
		bLightHitStopOn = true;
	}

	private void OnHeavyAttackHitLanded(Node3D ObjectHit)
	{
		AnimTree.Set(HeavyAttackHitStop, 0.0f);
		bHeavyHitStopOn = true;
	}

	private void UpdateHitStop(double delta)
	{
		CurrentHitStopTime += (float)delta;
		if (bLightHitStopOn && CurrentHitStopTime > LightHitStopTime)
		{
			bLightHitStopOn = false;
			CurrentHitStopTime = 0.0f;
			AnimTree.Set(LightAttackHitStop, 1.2f);
			AnimTree.Set(LightAttackRequest, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
			bLightAttackFadingOut = true;
		}
		else if (bHeavyHitStopOn && CurrentHitStopTime > HeavyHitStopTime)
		{
			bHeavyHitStopOn = false;
			CurrentHitStopTime = 0.0f;
			AnimTree.Set(HeavyAttackHitStop, 1.0f);
			AnimTree.Set(HeavyAttackRequest, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
			bHeavyAttackFadingOut = true;
		}
	}

	private void OnCharacterHitReceived(Godot.Collections.Dictionary HitResult)
	{
		AnimTree.Set(HitChestRequest, (int)AnimationNodeOneShot.OneShotRequest.Fire);
	}
}