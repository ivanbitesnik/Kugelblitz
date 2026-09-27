using Godot;
using System;

public static class DebugManager
{
	public static void PrintString(Node caller, string String, double Duration)
	{
		if (caller == null) return;

		Printdebug print = (Printdebug)caller.GetTree().Root.FindChild("Printdebug");
		if (print != null)
        {
			print.PrintString(String, Duration);
        }
	}
}
