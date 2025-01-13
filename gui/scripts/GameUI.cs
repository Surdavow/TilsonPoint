using Godot;
using System;
using System.IO;

public partial class GameUI : Control
{
	private EffectsControl EffectsControl;
	public string TransitionTo;
	public string sceneName;	
	public override void _Ready()
	{
		// Get the current scene name
		sceneName = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
		
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		EffectsControl.TransitionRect.fadeIn();
	}

	public override void _Process(double delta)
	{
		if(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")) < -0.1)
		{
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), -EffectsControl.TransitionRect.Color.A*50);
		}
	}
}
