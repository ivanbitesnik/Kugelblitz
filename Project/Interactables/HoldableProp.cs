using Godot;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Runtime.CompilerServices;

public partial class HoldableProp : InteractableProp
{
	[Export] private string attachBone = "LeftHand";
	[Export] private Vector3 positionOnAttach = Vector3.Zero;
	[Export] private Vector3 rotationOnAttach = Vector3.Zero;
	[Export] private bool updateInRT = false;

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (updateInRT && owner != null)
        {
			Position = positionOnAttach;
			Rotation = rotationOnAttach;
        }
	}
	
	public override void OnInteracted(Character instigator)
	{
		base.OnInteracted(instigator);

		List<BoneAttachment3D> boneAttachments = Globals.GlobalHelpers.GetChildrenOfType<BoneAttachment3D>(instigator);
		foreach (BoneAttachment3D boneAttachment in boneAttachments)
		{
			if (boneAttachment.BoneName == attachBone)
			{
				Reparent(boneAttachment, false);
				Rotation = rotationOnAttach;
				Position = positionOnAttach;
			}
		}
	}
}
