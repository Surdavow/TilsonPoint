using Godot;
using System;
using System.Collections.Generic;
public partial class SettingsMenuMargin : MarginContainer
{
	private MasterControl MasterControl;
	private MainMenuMargin MainMenuControl;
	private AudioStreamPlayer2D SettingsMenuSoundPlayer;
	private Dictionary<string, AudioStream> audioStreams;
	public Vector2 TargetPosition;
	public float lerpSpeed = 4f;
	public override void _Ready()
	{
		MasterControl = GetParent<MasterControl>();
		MainMenuControl = MasterControl.GetNode<MainMenuMargin>("MainMenuMargin");
		SettingsMenuSoundPlayer = GetNode<AudioStreamPlayer2D>("SettingsMenuSoundPlayer");

		audioStreams = new Dictionary<string, AudioStream>
		{
			{ "submenu_dropdown_select", (AudioStream)GD.Load("res://audio/menu/submenu_dropdown_select_01.wav") },
			{ "submenu_scroll", (AudioStream)GD.Load("res://audio/menu/submenu_scroll_01.wav") },
			{ "submenu_select", (AudioStream)GD.Load("res://audio/menu/submenu_select_01.wav") },
			{ "submenu_slidein", (AudioStream)GD.Load("res://audio/menu/submenu_slidein_01.wav") }
		};

		Position = new Vector2(0, -960);
		TargetPosition = Position;
	}

	public override void _Process(double delta)
	{
		if(Position != TargetPosition)
		{
			Position = new Vector2(Position.X, Mathf.Lerp(Position.Y, TargetPosition.Y, lerpSpeed * (float)delta));
		}
	}

	//Universal script for hovering over a mouse button
	public void _on_mouse_entered()
	{
		playStream("submenu_scroll");
	}

	public void _on_master_volume_slider_value_changed(float value)
	{
		playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), value <= -40);
	}

	public void _on_music_volume_slider_value_changed(float value)
	{
		playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), value <= -40);
	}

	public void _on_done_button_pressed()
	{
		playStream("submenu_dropdown_select");
		MasterControl.Set("AudioLowPassTarget",20500);
		MainMenuControl.TargetPosition = new Vector2(0, 0);
		TargetPosition = new Vector2(0, -960);
	}

	public void _on_settings_tab_changed(int tab)	
	{
		playStream("submenu_select");
	}

	public void _on_display_driver_button_item_selected(int index)
	{
		playStream("submenu_select");
	}

	public void _on_resolution_button_item_selected(int index)
	{
		playStream("submenu_select");
		switch(index)
		{
			// 1280x720
			case 0: 
				DisplayServer.WindowSetSize(new Vector2I(1280, 720)); 
				break;
			// 1920x1080
			case 1: 
				DisplayServer.WindowSetSize(new Vector2I(1920, 1080)); 
				break;
		}
	}
	
	public void _on_window_mode_button_item_selected(int index)
	{
		switch(index)
		{
			// Windowed
			case 0: 
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); 
				break;
			//Fullscreen
			case 1: 
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen); 
				break;
		}
	}

	public void playStream(string sound)
	{
		if (audioStreams.ContainsKey(sound) && audioStreams[sound] != null)
		{
			SettingsMenuSoundPlayer.Stream = audioStreams[sound];
			SettingsMenuSoundPlayer.Play();
		}
		else return;
	}
}
