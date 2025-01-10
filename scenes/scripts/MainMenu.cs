using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class MainMenu : Control
{
	private AudioStreamPlayer2D menuSoundPlayer;
	private ColorRect BlackRect;
	private Dictionary<string, AudioStream> audioStreams;
	public string Transition;
	public float AudioLowPassTarget = 20500;		
	public override void _Ready()
	{
		BlackRect = GetNode<ColorRect>("BlackRect");
		fadeIn();
		
		//Load the sound player and initialize the dictionary for the sounds
		menuSoundPlayer = GetNode<AudioStreamPlayer2D>("MenuSoundPlayer");				
		audioStreams = new Dictionary<string, AudioStream>
		{
			{ "submenu_dropdown_select", (AudioStream)GD.Load("res://audio/menu/submenu_dropdown_select_01.wav") },
			{ "submenu_scroll", (AudioStream)GD.Load("res://audio/menu/submenu_scroll_01.wav") },
			{ "submenu_select", (AudioStream)GD.Load("res://audio/menu/submenu_select_01.wav") },
			{ "submenu_slidein", (AudioStream)GD.Load("res://audio/menu/submenu_slidein_01.wav") }
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
		AudioEffect LowPass = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Music"), 0);
		
		if((float)LowPass.Get("cutoff_hz") != AudioLowPassTarget)
		{
			LowPass.Set("cutoff_hz",Mathf.Lerp((float)LowPass.Get("cutoff_hz"), AudioLowPassTarget, (float)delta*2));
		}	

		if(Transition != null)
		{
			Color BlackscreenColor = (Color)BlackRect.Get("color");
			BlackRect.Set("alphaTarget",1);

			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), -BlackscreenColor.A*50);
			if(BlackscreenColor.A > 0.99)
			{
				switch(Transition)
				{
					case "HostGame":
						GetTree().ChangeSceneToFile("res://scenes/Test.tscn");
						break;
					case "Quit":
						GetTree().Quit();
						break;
				}
			}				
		}		
	}
	
	public void playStream(string sound)
	{
		if (audioStreams.ContainsKey(sound) && audioStreams[sound] != null)
		{
			menuSoundPlayer.Stream = audioStreams[sound];
			menuSoundPlayer.Play();
		}
		else return;
	}

	//Universal script for hovering over a mouse button
	public void _on_mouse_entered()
	{
		playStream("submenu_scroll");
	}
	public void fadeOut()
	{
		BlackRect.Set("alphaTarget",1);
	}
	public void fadeIn()
	{
		BlackRect.Set("alphaTarget",0);
	}
}
