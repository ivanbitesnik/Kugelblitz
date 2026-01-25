using Godot;
using System;
using System.Diagnostics;

public partial class SingleplayerCharacter : Character
{
	[Export] private float SpeedMultiplier = 1.0f;
	[Export] private float BackwardsSpeedMultiplier = 1.0f;
	[Export] private float SprintSpeedMultiplier = 1.0f;
	[Export] private float CrouchSpeedMultiplier = 1.0f;
	[Export] private float BackwardsCrouchSpeedMultiplier = 1.0f;
	[Export] private float JumpVelocityMultiplier = 1.0f;

	[Export] private float cameraSensitivity = 0.01f;
	[Export] private float cameraLowerLimit = -45.0f;
	[Export] private float cameraUpperLimit = 45.0f;
	[Export] private float cameraResetSpeed = 1.0f;
	[Export] private float rotationSpeed = 1.0f;

	public float GetWalkSpeed() => Speed * SpeedMultiplier;
	public float GetBackwardsWalkSpeed() => BackwardsSpeed * BackwardsSpeedMultiplier;
	public float GetSprintSpeed() => SprintSpeed * SprintSpeedMultiplier;
	public float GetCrouchSpeed() => CrouchSpeed * CrouchSpeedMultiplier;
	public float GetBackwardsCrouchSpeed() => BackwardsCrouchSpeed * BackwardsCrouchSpeedMultiplier;
	public float GetJumpVelocity() => JumpVelocity * JumpVelocityMultiplier;
	private Camera3D camera;
	private Vector3 cameraInitialRotation = new Vector3();
	private Node3D cameraPivot;
	private Node3D armature;
	private bool bIsGrabbingCameraLeft = false;
	private bool bIsGrabbingCameraRight = false;
	private bool IsGrabbingCamera() => bIsGrabbingCameraLeft || bIsGrabbingCameraRight;
	private float cameraAngleX = 0.0f;
	private bool bLanding = false;

	public override void _ExitTree()
	{
		QueueFree();
	}

	public override void _Ready()
	{
		armature = (Node3D)FindChild("Armature");
		cameraPivot = (Node3D)FindChild("CameraPivot");
		camera = (Camera3D)FindChild("Camera3D");
		camera.Current = true;
		cameraInitialRotation = camera.Rotation;
	}
	
	private float GetSpeed(Vector2 inputDir)
	{
		bool bWalkingForward = inputDir.Y < 0.0f;	// forwards is negative input for some reason idk
		float currentSpeed;
		if (Input.IsActionPressed("Crouch"))
		{
			currentSpeed = bWalkingForward ? GetCrouchSpeed() : GetBackwardsCrouchSpeed();
		}
		else if (bWalkingForward && Input.IsActionPressed("Sprint"))
		{
			currentSpeed = GetSprintSpeed();
		}
		else
		{
			currentSpeed = bWalkingForward ? GetWalkSpeed() : GetBackwardsWalkSpeed();
		}

		return currentSpeed;
    }

	private void CalculateVelocity(double delta, Vector2 inputDir)
	{
		Vector3 velocity = Velocity;
		float currentSpeed = GetSpeed(inputDir);

		if (!bIsGrabbingCameraRight)
		{
			// Null velocity left/right (this input is handled in CalculateRotation)
			inputDir.X = 0.0f;
		}

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

	private void CalculateRotation(double delta, Vector2 inputDir)
	{
		if (bIsGrabbingCameraRight) return;

		RotateY(-inputDir.X * rotationSpeed * (float)delta);
		if (bIsGrabbingCameraLeft)
		{
			cameraPivot.RotateY(inputDir.X * rotationSpeed * (float)delta);
		}
	}

	private void AdjustCameraPosition(double delta, Vector2 inputDir)
	{
		if (IsGrabbingCamera() || inputDir.IsZeroApprox()) return;

		cameraPivot.Rotation = Vector3.Zero;
		camera.Rotation = cameraInitialRotation;
		cameraAngleX = 0.0f;

		// TODO: transition
		//float totalTransitionTime = 1.0f / cameraResetSpeed;
		//Vector3 newPivotRotation = -cameraPivot.Rotation / totalTransitionTime * (float)delta;
		//cameraPivot.Rotation = newPivotRotation;
		//Vector3 newCameraRotation = (camera.Rotation - cameraInitialRotation) / totalTransitionTime * (float)delta;
		//camera.Rotation = newCameraRotation;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		CalculateVelocity(delta, inputDir);
		CalculateRotation(delta, inputDir);
		AdjustCameraPosition(delta, inputDir);
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			HandleGrabCamera(eventMouseButton);
		}
		if (@event is InputEventMouseMotion eventMouseMotion && IsGrabbingCamera())
		{
			RotateCamera(eventMouseMotion);
		}
	}
	
	private void HandleGrabCamera(InputEventMouseButton eventMouseButton)
    {
		if (eventMouseButton.IsActionPressed("GrabCameraRight"))
		{
			bIsGrabbingCameraRight = true;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else if (eventMouseButton.IsActionReleased("GrabCameraRight"))
		{
			bIsGrabbingCameraRight = false;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		
		if (eventMouseButton.IsActionPressed("GrabCameraLeft"))
		{
			bIsGrabbingCameraLeft = true;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else if (eventMouseButton.IsActionReleased("GrabCameraLeft"))
		{
			bIsGrabbingCameraLeft = false;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
    }

	private void RotateCamera(InputEventMouseMotion mouseEvent)
	{
		if (bIsGrabbingCameraLeft)
		{
			cameraPivot.RotateY(-mouseEvent.Relative.X * cameraSensitivity);
		}
		else
		{
			RotateY(-mouseEvent.Relative.X * cameraSensitivity);
		}

		cameraAngleX -= mouseEvent.Relative.Y * cameraSensitivity;
		cameraAngleX = Math.Clamp(cameraAngleX, Mathf.DegToRad(cameraLowerLimit), Mathf.DegToRad(cameraUpperLimit));
		cameraPivot.Rotation = new Vector3(cameraAngleX, cameraPivot.Rotation.Y, cameraPivot.Rotation.Z);
		camera.Rotation = new Vector3(-cameraAngleX, camera.Rotation.Y, camera.Rotation.Z);
	}
}
