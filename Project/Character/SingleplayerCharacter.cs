using Godot;
using System;
using System.Diagnostics;

public partial class SingleplayerCharacter : Character
{
	[Export] private float cameraSensitivity = 0.05f;
	[Export] private float cameraLowerLimit = -45.0f;
	[Export] private float cameraUpperLimit = 45.0f;
	[Export] private float cameraResetSpeed = 1.0f;

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
		camera = (Camera3D)FindChild("Camera3D");
		camera.Current = true;
		cameraInitialRotation = camera.Rotation;
		cameraPivot = (Node3D)FindChild("CameraPivot");
    }

	private void CalculateVelocity(double delta, Vector3 direction)
	{
		Vector3 velocity = Velocity;
		float currentSpeed = Input.IsActionPressed("Sprint") ? SprintSpeed : Speed;

		if (IsOnFloor())
		{
			if (bLanding)
			{
				bLanding = false;
				EmitSignal(nameof(OnLanded));
            }
			else if (Input.IsActionJustPressed("Jump"))
			{
				velocity.Y = JumpVelocity;
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
			if (bIsGrabbingCameraRight)
			{
				velocity.X = -direction.X * currentSpeed;
			}

			velocity.Z = -direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
	}

	private void CalculateRotation(Vector3 direction)
	{
		if (direction.X == 0.0f) return;

		if (bIsGrabbingCameraLeft)
		{
			armature.RotateY(armature.Rotation.AngleTo(direction));
		}
		else
		{
			RotateY(-direction.X);
		}
	}

	private void AdjustCameraPosition(Vector2 inputDir, double delta)
	{
		if (IsGrabbingCamera() || inputDir.IsZeroApprox()) return;

		// figure out the math for this lol
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
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		CalculateVelocity(delta, direction);
		CalculateRotation(direction);
		AdjustCameraPosition(inputDir, delta);
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
		camera.LookAt(Position);
	}
}
