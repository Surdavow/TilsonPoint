using Godot;
using Godot.Collections;
using System;

public partial class EffectsSoundPlayer : AudioStreamPlayer2D
{
	private Dictionary<string, AudioStream> audioStreams;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		audioStreams = new Dictionary<string, AudioStream>
		{
			{ "submenu_dropdown_select", (AudioStream)GD.Load("res://audio/menu/submenu_dropdown_select_01.wav") },
			{ "submenu_scroll", (AudioStream)GD.Load("res://audio/menu/submenu_scroll_01.wav") },
			{ "submenu_select", (AudioStream)GD.Load("res://audio/menu/submenu_select_01.wav") },
			{ "submenu_slidein", (AudioStream)GD.Load("res://audio/menu/submenu_slidein_01.wav") }
		};
	}

	public void playStream(string sound)
	{
		if (audioStreams.ContainsKey(sound) && audioStreams[sound] != null)
		{
			Stream = audioStreams[sound];
			Play();
		}
		else return;
	}
}
