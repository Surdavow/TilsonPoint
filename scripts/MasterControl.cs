using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public partial class MasterControl : Control
{	
	private TransitionRect TransitionRect;	
	public string TransitionTo;
	public string sceneName;
	public float AudioLowPassTarget = 20500;
	public override void _Ready()
	{
		sceneName = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
		TransitionRect = GetNode<TransitionRect>("TransitionRect");
		TransitionRect.fadeIn();		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
		AudioEffect LowPass = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Music"), 0);
		
		if((float)LowPass.Get("cutoff_hz") != AudioLowPassTarget)
		{
			LowPass.Set("cutoff_hz",Mathf.Lerp((float)LowPass.Get("cutoff_hz"), AudioLowPassTarget, (float)delta*2));
		}	

		if(TransitionTo != null)
		{
			Color TransitionRectColor = (Color)TransitionRect.Get("color");
			TransitionRect.fadeOut();

			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), -TransitionRectColor.A*40);
			if(TransitionRectColor.A > 0.995)
			{
				switch(TransitionTo)
				{
					case "HostGame":
						if(sceneName == "MainMenu")
						{
							GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
						}						
						break;
					case "Quit":
						GetTree().Quit();
						break;
				}
			}				
		}		
	}
}
