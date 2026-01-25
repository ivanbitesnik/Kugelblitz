using Godot;
using System;

public partial class World : Node3D
{
	[Export] public PackedScene MapScene = GD.Load<PackedScene>("res://Maps/Arena.tscn");
	[Export] public PackedScene PlayerScene = GD.Load<PackedScene>("res://Character/MultiplayerCharacter.tscn");
	public PackedScene MenuScene = GD.Load<PackedScene>("res://Levels/Lobby/menu.tscn");
	public PackedScene OptionsScene = GD.Load<PackedScene>("res://Levels/OptionsMenu.tscn");

	private Node OptionsMenuInstance;
	public Timer ResetGameTimer = new Timer();

    public override void _Ready()
    {
		base._Ready();

		Node map = MapScene.Instantiate();
		map.Name = "Map";
		AddChild(map);

		ResetGameTimer.OneShot = true;
		ResetGameTimer.Autostart = false;
		ResetGameTimer.WaitTime = 3.0f;
		AddChild(ResetGameTimer);
    }
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("OpenOptions"))
		{
			if (OptionsMenuInstance is null)
			{
				OptionsMenuInstance = OptionsScene.Instantiate();
				AddChild(OptionsMenuInstance);
			}
			else
			{
				OptionsMenuInstance.QueueFree();
				OptionsMenuInstance = null;
			}
		}
	}

	public void TravelToMenu()
	{
		GetTree().ChangeSceneToPacked(MenuScene);
	}
}
