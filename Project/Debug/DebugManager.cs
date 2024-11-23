using Godot;
using System;

public static class DebugManager
{
	public static void PrintString(Node caller, string String, double Duration)
	{
		caller.GetTree().Root.GetChild(0).GetNode<Printdebug>("Printdebug").PrintString(String, Duration);
	}
}
