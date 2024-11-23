using Godot;
using System;

public partial class Printdebug : Control
{
	public double DebugDurationLeft = 0.0f;
	public string currentDebugString = "";
	[Export] private Font font = ThemeDB.FallbackFont;
	[Export] private Vector2 drawPos = new Vector2(20.0f, 50.0f);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DebugDurationLeft -= delta;
		if (DebugDurationLeft < 0.0f)
		{
			DebugDurationLeft = 0.0f;
			currentDebugString = "";
			QueueRedraw();
		}
	}

	public void PrintString(string String, double Duration)
	{
		currentDebugString = String;
		DebugDurationLeft = Duration;

		QueueRedraw();
	}

	public void _on_draw()
	{
		DrawString(font, drawPos, currentDebugString, HorizontalAlignment.Left, 1920.0f, 15);
	}
}
