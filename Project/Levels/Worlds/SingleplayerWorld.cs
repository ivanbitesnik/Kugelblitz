using Godot;
using System;
using System.Threading.Tasks;

public partial class SingleplayerWorld : World
{	
	public override void _Ready()
	{
		base._Ready();
		SpawnPlayer();
	}

	private void SpawnPlayer()
	{
		Character playerInstance = (Character)PlayerScene.Instantiate();
		playerInstance.Name = "Player";
		playerInstance.OnPlayerDead += ResetGame;
		AddChild(playerInstance, true);
	}

	private void ResetGame()
	{
		if (ResetGameTimer.TimeLeft > 0.0f) return;
		ResetGameTask();
	}

	private async Task ResetGameTask()
	{
		ResetGameTimer.Start();
		await ToSignal(ResetGameTimer, SceneTreeTimer.SignalName.Timeout);
		RemovePlayer();
		SpawnPlayer();
	}

	private void RemovePlayer()
	{
		Node player = FindChild("Player", true, false);
		if (player is not null)
		{
			player.Name = "PendingDestroyPlayer"; // We change the name so that its not taken when spawning a new one
			player.QueueFree();
		}
	}

	public override void _Notification(int what)
	{
    	if (what == NotificationWMCloseRequest)
		{
        	GetTree().Quit(); // default behavior
		}
	}
}
