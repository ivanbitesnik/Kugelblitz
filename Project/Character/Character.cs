using Godot;
using System;
using System.Threading.Tasks;

public partial class Character : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float BackwardsSpeed = 2.5f;
	public const float SprintSpeed = 10.0f;
	public const float CrouchSpeed = 2.0f;
	public const float BackwardsCrouchSpeed = 1.0f;
	public const float JumpVelocity = 4.5f;

	[Signal]
	public delegate void OnPlayerDeadEventHandler();

	[Signal]
	public delegate void OnJumpEventHandler();

	[Signal]
	public delegate void OnLandedEventHandler();

	public float Health { get; private set; } = 100.0f;

	virtual public void ReceiveDamage(float damage)
	{
		ReceiveDamageInternal(damage);	
	}

	private void ReceiveDamageInternal(float damage)
    {
        Health -= damage;
		if (Health <= 0)
		{
			QueueFree();
			EmitSignal(nameof(OnPlayerDead));
		}
    }
}
