using Godot;
using Microsoft.VisualBasic;
using System;

public partial class Printdebug : Control
{
	public Godot.Collections.Dictionary<string, double> currentDebugDurations = new Godot.Collections.Dictionary<string, double>();
	public Godot.Collections.Dictionary<string, double> currentDebugTimestamps = new Godot.Collections.Dictionary<string, double>();
	[Export] private Font font = ThemeDB.FallbackFont;
	[Export] private Vector2 drawPos = new Vector2(20.0f, 50.0f);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (var debugString in currentDebugTimestamps)
		{
			double duration = currentDebugDurations[debugString.Key];
			if (Time.GetUnixTimeFromSystem() - debugString.Value >= duration)
			{
				currentDebugTimestamps.Remove(debugString.Key);
				currentDebugDurations.Remove(debugString.Key);
				QueueRedraw();
			}
		}
	}

	public void PrintString(string String, double Duration)
	{
		currentDebugTimestamps.Add(String, Time.GetUnixTimeFromSystem());
		currentDebugDurations.Add(String, Duration);

		QueueRedraw();
	}

	public void _on_draw()
	{
		float verticalPos = drawPos.Y;
		foreach (var debugString in currentDebugTimestamps)
		{
			Vector2 pos = new Vector2(drawPos.X, verticalPos);
			DrawString(font, pos, debugString.Key, HorizontalAlignment.Left, 1920.0f, 15);

			verticalPos += 15.0f;
		}
	}
}
