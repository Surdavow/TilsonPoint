using Godot;
using GodotPlugins.Game;
using System;

public partial class EffectsControl : Control
{	
	public float MasterVolumeTarget;
	public float MusicVolumeTarget;
	public string TransitionTo;
	public TransitionRect TransitionRect;
	public float AudioLowPassTarget = 20500;

	public override void _Ready()
	{		
		TransitionRect = GetNode<TransitionRect>("TransitionRect");

		// Set initial volume targets
		MasterVolumeTarget = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
		MusicVolumeTarget = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
	}

	public override void _Process(double delta)
	{
	    // Handle low pass filter interpolation for music
		AudioEffect MusicLowPass = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Music"), 0);
	    float currentCutoffHz = (float)MusicLowPass.Get("cutoff_hz");
	    if (currentCutoffHz != AudioLowPassTarget)
	    {
	        float interpolatedCutoff = Mathf.Lerp(currentCutoffHz, AudioLowPassTarget, (float)delta * 2);
	        MusicLowPass.Set("cutoff_hz", interpolatedCutoff);
	    }
	
	    // Handle master volume amplify effect during transitions or normal state
		AudioEffect MasterAmplifyFilter = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Master"), 0);
	    float TransitionRectAlphaTarget = -TransitionRect.Color.A * 40;    
		float currentMasterAmplifyValue = (float)MasterAmplifyFilter.Get("volume_db");
	    if (currentMasterAmplifyValue != TransitionRectAlphaTarget)
	    {
	        MasterAmplifyFilter.Set("volume_db",TransitionRectAlphaTarget);
	    }

		// Handle master volume updates
		float currentMasterVolume = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
		if (currentMasterVolume != MasterVolumeTarget)
		{
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), MasterVolumeTarget);
		}
	
	    // Handle music volume updates
	    float currentMusicVolume = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
	    if (currentMusicVolume != MusicVolumeTarget)
	    {
	        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), MusicVolumeTarget);
	    }

		if(TransitionRect.Color.A < 0.05 && TransitionTo == "start")
		{
			TransitionTo = "";
			TransitionRect.setAlpha(0, true);
		}
		else if(TransitionRect.Color.A > 0.995)
		{
			switch(TransitionTo.ToLower())
			{
				case "hostgame":
					GetTree().ChangeSceneToFile("res://resource/scenes/Game.tscn");			
					break;
				case "mainmenu":					
					GetTree().ChangeSceneToFile("res://resource/scenes/MainMenu.tscn");
					break;
				case "start":
					break;
				case "quit":
					GetTree().Quit();
					break;					
			}
		}
	}
}