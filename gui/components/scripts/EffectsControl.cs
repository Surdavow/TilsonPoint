using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public partial class EffectsControl : Control
{	
	public TransitionRect TransitionRect;
	public AudioEffect MusicLowPass;
	public float AudioLowPassTarget = 20500;

	public override void _Ready()
	{		
		TransitionRect = GetNode<TransitionRect>("TransitionRect");
		MusicLowPass = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Music"), 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{			
		if((float)MusicLowPass.Get("cutoff_hz") != AudioLowPassTarget)
		{
			float cutoffHz = (float)MusicLowPass.Get("cutoff_hz");
			MusicLowPass.Set("cutoff_hz",Mathf.Lerp(cutoffHz, AudioLowPassTarget, (float)delta*2));
		}
	}
}
