using Godot;
using GodotPlugins.Game;
using System;

public partial class EffectsControl : Control
{
	public float AudioLowPassTarget = 20500;
	public string TransitionTo;
	public TransitionRect TransitionRect;
	public EffectsSoundPlayer SoundPlayer;
	public EffectsMusicPlayer MusicPlayer;

	public override void _Ready()
	{		
		TransitionRect = GetNode<TransitionRect>("TransitionRect");
		SoundPlayer = GetNode<EffectsSoundPlayer>("EffectsSoundPlayer");
		MusicPlayer = GetNode<EffectsMusicPlayer>("EffectsMusicPlayer");
	}

	public override void _Process(double delta)
	{
		HandleAudioEffects(delta);
		HandleSceneTransitions();
	}

	private void HandleAudioEffects(double delta)
	{
		// Handle music low pass filter interpolation
		AudioEffect musicLowPass = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Music"), 0);
		float currentCutoffHz = (float)musicLowPass.Get("cutoff_hz");
		if (currentCutoffHz != AudioLowPassTarget)
		{
			float interpolatedCutoff = Mathf.Lerp(currentCutoffHz, AudioLowPassTarget, (float)delta * 2);
			musicLowPass.Set("cutoff_hz", interpolatedCutoff);
		}

		// Handle master amplify effect
		AudioEffect masterAmplifyFilter = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Master"), 0);
		float transitionRectAlphaTarget = -TransitionRect.Color.A * 40;    
		float currentMasterAmplifyValue = (float)masterAmplifyFilter.Get("volume_db");
		if (currentMasterAmplifyValue != transitionRectAlphaTarget)
		{
			masterAmplifyFilter.Set("volume_db", transitionRectAlphaTarget);
		}
	}

	private void HandleSceneTransitions()
	{
		if (TransitionRect.Color.A < 0.05 && TransitionTo == "start")
		{
			TransitionTo = "";
			TransitionRect.setAlpha(0, true);
		}
		else if (TransitionRect.Color.A > 0.9975)
		{
			switch (TransitionTo.ToLower())
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