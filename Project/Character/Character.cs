using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Character : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float SprintSpeed = 20.0f;
	public const float CrouchSpeed = 2.0f;
	public const float JumpVelocity = 4.5f;

	[Signal]
	public delegate void OnPlayerDeadEventHandler();

	[Signal]
	public delegate void OnJumpEventHandler();

	[Signal]
	public delegate void OnLandedEventHandler();

	[Signal]
	public delegate void OnHitReceivedEventHandler(Godot.Collections.Dictionary hitResult);

	public float Health { get; private set; } = 100.0f;

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	virtual public void ReceiveHit(Godot.Collections.Dictionary hitResult)
	{
		EmitSignal(nameof(OnHitReceived), hitResult);
	}

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
