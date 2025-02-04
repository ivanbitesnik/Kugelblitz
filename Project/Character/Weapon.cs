using Godot;
using System;
using Kugelblitz.Character;

public partial class Weapon : StaticBody3D
{
	[Export] private PackedScene projectileScene = GD.Load<PackedScene>("res://Projectile/projectile.tscn");
	[Export] public double FireRate = 0.33f;
	[Export] public float ProjectileSpeed = 500.0f;
	private Color ProjectileColor;

	private double LastShotTimestamp = -1.0f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProjectileColor = GetParent<Player>().Color;
	}
	
	public void PressTrigger()
	{
		if (CanFire())
		{
			Rpc(nameof(FireProjectile));

			LastShotTimestamp = Time.GetUnixTimeFromSystem();
		}
	}
	
	private bool CanFire()
	{
		double currentTime = Time.GetUnixTimeFromSystem();
		if (LastShotTimestamp >= 0.0f)
		{
			return (currentTime - LastShotTimestamp) >= FireRate;
		}
		
		return true;
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void FireProjectile()
	{
		Projectile projectile = (Projectile)projectileScene.Instantiate();
		Vector3 direction = -GetGlobalTransform().Basis.Y;
		Vector3 muzzlePos = GetNode<Marker3D>("Muzzle").GlobalPosition + (direction * 3.0f);
		projectile.Initialize(muzzlePos, direction, ProjectileSpeed, ProjectileColor);
		GetTree().Root.AddChild(projectile);
		projectile.SetMultiplayerAuthority(GetParent<Player>().GetPlayerId(), false);
	}
}
