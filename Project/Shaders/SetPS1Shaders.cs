using Godot;
using System;
using System.Collections.Generic;

[Tool, GlobalClass]
public partial class SetPS1Shaders : EditorScript
{
	public override void _Run()
	{
		Shader ShaderToApply = GD.Load<Shader>("res://Shaders/ps1_visuals.gdshader");
		var nodes = GetScene().GetChildren();
		foreach (var node in nodes)
        {
			if (node is MeshInstance3D)
			{
				MeshInstance3D mesh = (MeshInstance3D)node;

				for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
				{
					StandardMaterial3D surfaceMaterial = (StandardMaterial3D)mesh.Mesh.SurfaceGetMaterial(i);
					if (surfaceMaterial != null)
					{
						Color surfaceColor = surfaceMaterial.AlbedoColor;
						CurveXyzTexture texture = new CurveXyzTexture();
						texture.CurveX = new Curve();
						texture.CurveX.AddPoint(new Vector2(0.0f, surfaceColor.R));
						texture.CurveY = new Curve();
						texture.CurveY.AddPoint(new Vector2(0.0f, surfaceColor.G));
						texture.CurveZ = new Curve();
						texture.CurveZ.AddPoint(new Vector2(0.0f, surfaceColor.B));

						// Shader material is empty for some reason
						ShaderMaterial shaderMaterial = new ShaderMaterial();
						shaderMaterial.Shader = ShaderToApply;
						mesh.SetSurfaceOverrideMaterial(i, shaderMaterial);
						shaderMaterial.SetShaderParameter("jitter", 0.8f);
						shaderMaterial.SetShaderParameter("resolution", new Vector2I(80, 60));
						shaderMaterial.SetShaderParameter("albedo", texture);
					}
				}
            }
        }
    }
}
