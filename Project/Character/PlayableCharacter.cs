using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class PlayableCharacter : Character
{
	[Export] private float WalkSpeedMultiplier = 1.0f;
	[Export] private float SprintSpeedMultiplier = 1.0f;
	[Export] private float CrouchSpeedMultiplier = 1.0f;
	[Export] private float JumpVelocityMultiplier = 1.0f;

	[Export] private float cameraSensitivity = 0.01f;
	[Export] private float cameraLowerLimit = -45.0f;
	[Export] private float cameraUpperLimit = 45.0f;
	[Export] private float cameraResetSpeed = 1.0f;
	[Export] private float rotationSpeed = 1.0f;

	public float GetWalkSpeed() => Speed * WalkSpeedMultiplier;
	public float GetSprintSpeed() => SprintSpeed * SprintSpeedMultiplier;
	public float GetCrouchSpeed() => CrouchSpeed * CrouchSpeedMultiplier;
	public float GetJumpVelocity() => JumpVelocity * JumpVelocityMultiplier;
	private Camera3D camera;
	private Vector3 cameraInitialRotation = new Vector3();
	private Node3D cameraPivot;
	private CharacterAnimator characterAnimator;
	private Skeleton3D characterSkeleton;
	private float cameraAngleX = 0.0f;
	private bool bLanding = false;
	private InteractableProp availableInteractable;
	private Node InteractionPromptInstance;
	[Signal]
	public delegate void OnInteractedEventHandler(string interactionAnimation);
	[Signal]
	public delegate void OnLightAttackEventHandler();
	[Signal]
	public delegate void OnHeavyAttackEventHandler();
	[Signal]
	public delegate void OnLightAttackHitEventHandler(Node3D ObjectHit);
	[Signal]
	public delegate void OnHeavyAttackHitEventHandler(Node3D ObjectHit);

	public int PlayerId { get; private set; } = 0;
	public bool IsServer() => PlayerId == 1;

	private bool bQueryLightAttack = false;
	private bool bQueryHeavyAttack = false;

	public override void _EnterTree()
	{
		PlayerId = int.Parse(Name);
		SetMultiplayerAuthority(PlayerId);

		Position = IsServer() ? GetTree().Root.GetNode<Marker3D>("World/Map/PlayerOneSpawnPoint").Position 
							  : GetTree().Root.GetNode<Marker3D>("World/Map/PlayerTwoSpawnPoint").Position;
	}
	
	public override void _ExitTree()
	{
		QueueFree();
	}

	public override void _Ready()
	{
		bool bAuthority = IsMultiplayerAuthority();
		SetProcessInput(bAuthority);
		SetProcess(bAuthority);
		SetPhysicsProcess(bAuthority);

		camera = (Camera3D)FindChild("Camera3D");
		camera.Current = bAuthority;
		if (!bAuthority) return;

		characterAnimator = (CharacterAnimator)FindChild("AnimationPlayer");
		characterSkeleton = (Skeleton3D)FindChild("GeneralSkeleton");
		cameraPivot = (Node3D)FindChild("CameraPivot");
		cameraInitialRotation = camera.Rotation;

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	
	public float GetSpeed(Vector2 inputDir)
	{
		if (Input.IsActionPressed("Crouch"))
		{
			return GetCrouchSpeed();
		}
		else if (inputDir.Y < 0.0f && Input.IsActionPressed("Sprint") && CanSprint())
		{
			return GetSprintSpeed();
		}
		else if (!inputDir.IsZeroApprox())
		{
			return GetWalkSpeed();
		}

		return 0.0f;
    }

	public bool CanSprint()
	{
		return !characterAnimator.IsHeavyAttackActive() && !characterAnimator.IsLightAttackActive();
	}

	private void CalculateVelocity(double delta, Vector2 inputDir)
	{
		Vector3 velocity = Velocity;
		float currentSpeed = GetSpeed(inputDir);

		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		if (IsOnFloor())
		{
			if (bLanding)
			{
				bLanding = false;
				EmitSignal(nameof(OnLanded));
            }
			else if (Input.IsActionJustPressed("Jump"))
			{
				velocity.Y = GetJumpVelocity();
				EmitSignal(nameof(OnJump));
			}
		}
		else
		{
			velocity += GetGravity() * (float)delta;
			if (!bLanding)
            {
				bLanding = true;
            }
        }

		if (direction != Vector3.Zero)
		{
			velocity.X = -direction.X * currentSpeed * (float)delta;
			velocity.Z = -direction.Z * currentSpeed * (float)delta;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
		
		Velocity = velocity;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		CalculateVelocity(delta, inputDir);
		MoveAndSlide();

		if (bQueryLightAttack || bQueryHeavyAttack)
		{
			// Collision check with fists, Light is left fist, Heavy is right fist
			string handBone = bQueryLightAttack ? "LeftHand" : "RightHand";
			Transform3D boneTransform = characterSkeleton.GetBoneGlobalPose(characterSkeleton.FindBone(handBone));
			Vector3 bonePosition = characterSkeleton.ToGlobal(boneTransform.Origin);
			// TODO: Make this a box that follows the rotation of the wrist
            SphereShape3D sphere = new SphereShape3D
            {
                Radius = 0.3f
            };

            Array<Rid> excludes = new Array<Rid>
            {
                GetRid()
            };
            var query = new PhysicsShapeQueryParameters3D
            {
                Shape = sphere,
                Transform = new Transform3D(Vector3.Zero, Vector3.Zero, Vector3.Zero, bonePosition),
                CollideWithBodies = true,
                CollideWithAreas = true,
				Exclude = excludes
            };
			
			//DebugDraw3D.DrawSphere(bonePosition, sphere.Radius, Colors.Red, (float)delta);
			Array<Dictionary> results = GetWorld3D().DirectSpaceState.IntersectShape(query);
			StringName EventName = bQueryLightAttack ? nameof(OnLightAttackHit) : nameof(OnHeavyAttackHit);
			foreach (Dictionary result in results)
			{
				// TODO: filter hits per importance, the query is pretty big
				Node3D collider = (Node3D)result["collider"];
				EmitSignal(EventName, collider);
				if (collider is Character character)
				{
					character.Rpc(nameof(ReceiveHit), result);
				}
				// TODO: Once we hit something, we should consider the attack as finished so we can reset it, leading to combos
				StopLightAttackQuery();
				StopHeavyAttackQuery();
				break;
			}
		}
	}

	public bool CanInteract()
    {
		return true;
    }

	public override void _Process(double delta)
	{
		if (!CanInteract())
		{
			return;
		}

		FindAvailableInteractions();
		if (availableInteractable != null)
		{
			if (InteractionPromptInstance is null)
			{
				InteractionPromptInstance = availableInteractable.GetInteractionPrompt().Instantiate();
				AddChild(InteractionPromptInstance);
			}

			if (Input.IsActionJustPressed("Interact"))
			{
				Rpc(nameof(Interact));
			}
		}
		else if (InteractionPromptInstance != null)
		{
			InteractionPromptInstance.QueueFree();
			InteractionPromptInstance = null;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void Interact()
	{
		if (availableInteractable != null)
        {
			availableInteractable.OnInteracted(this);
        }
    }

	private void FindAvailableInteractions()
	{
		foreach (Node node in GetTree().GetNodesInGroup("Interactables"))
		{
			if (node is not InteractableProp)
			{
				continue;
			}

			InteractableProp interactable = (InteractableProp)node;
			if (interactable.CanBeInteractedBy(this))
			{
				availableInteractable = interactable;
				return;
			}
		}

		availableInteractable = null;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.IsAction("LightAttack") && !characterAnimator.IsLightAttackActive())
			{
				Rpc(nameof(NotifyLightAttack));
			}
			else if (eventMouseButton.IsAction("HeavyAttack") && !characterAnimator.IsHeavyAttackActive())
			{
				Rpc(nameof(NotifyHeavyAttack));
			}
		}
		if (@event is InputEventMouseMotion eventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			RotateCamera(eventMouseMotion);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void NotifyLightAttack()
	{
		EmitSignal(nameof(OnLightAttack));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void NotifyHeavyAttack()
	{
		EmitSignal(nameof(OnHeavyAttack));
	}

	public void StartLightAttackQuery()
	{
		bQueryLightAttack = true;
	}

	public void StopLightAttackQuery()
	{
		bQueryLightAttack = false;
	}

	public void StartHeavyAttackQuery()
	{
		bQueryHeavyAttack = true;
	}

	public void StopHeavyAttackQuery()
	{
		bQueryHeavyAttack = false;
	}

	private void RotateCamera(InputEventMouseMotion mouseEvent)
	{
		RotateY(-mouseEvent.Relative.X * cameraSensitivity);

		cameraAngleX += mouseEvent.Relative.Y * cameraSensitivity;
		cameraAngleX = Math.Clamp(cameraAngleX, Mathf.DegToRad(cameraLowerLimit), Mathf.DegToRad(cameraUpperLimit));
		cameraPivot.Rotation = new Vector3(cameraAngleX, cameraPivot.Rotation.Y, cameraPivot.Rotation.Z);
		camera.Rotation = new Vector3(-cameraAngleX, camera.Rotation.Y, camera.Rotation.Z);
	}
}
