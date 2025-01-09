using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{	    
	private AudioStreamPlayer2D menuSoundPlayer;
	private Label titleLabel;
	private bool isTitleVisible = false;
	private int titleTransitionCount = 0;
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };	
	private Dictionary<string, AudioStream> audioStreams;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Load the sound player and initialize the dictionary for the sounds
		menuSoundPlayer = GetNode<AudioStreamPlayer2D>("MenuSoundPlayer");				
		audioStreams = new Dictionary<string, AudioStream>
		{
			{ "submenu_dropdown_select", (AudioStream)GD.Load("res://audio/menu/submenu_dropdown_select_01.wav") },
			{ "submenu_scroll", (AudioStream)GD.Load("res://audio/menu/submenu_scroll_01.wav") },
			{ "submenu_select", (AudioStream)GD.Load("res://audio/menu/submenu_select_01.wav") },
			{ "submenu_slidein", (AudioStream)GD.Load("res://audio/menu/submenu_slidein_01.wav") }
		};
		
		// Adjust the label, make it transparent
		titleLabel = GetNode<Label>("MainMenuControl/MainMenuContainer/TitleLabel");	
		foreach (string property in titleFontProperties)
		{
			Color currentColor = (Color)titleLabel.Get($"theme_override_colors/{property}");
			titleLabel.Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, 0));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{			
		
		
		if (isTitleVisible) return;
		// Lerp towards the target alpha for all relevant colors
		foreach (string property in titleFontProperties)
		{			
			Color currentColor = (Color)titleLabel.Get($"theme_override_colors/{property}");
			if (currentColor.A < 0.99)
			{
				float currentAlpha = currentColor.A;
				currentAlpha = Mathf.Lerp(currentAlpha, 1, 0.5f * (float)delta);
				titleLabel.Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, currentAlpha));		
			}
			else titleTransitionCount++;
		}

		if (titleTransitionCount == titleFontProperties.Length)
		{
			isTitleVisible = true;			
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

	// Example button press handlers using the dictionary directly
	public void _on_host_game_button_pressed()
	{
		playStream("submenu_dropdown_select");
	}

	public void _on_quit_button_pressed()
	{
		playStream("submenu_dropdown_select");
		GetTree().Quit();
	}

	public void _on_master_volume_slider_value_changed(float value)
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), value <= -40);
	}

	public void _on_music_volume_slider_value_changed(float value)
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), value <= -40);
	}

	public void _on_options_button_pressed()
	{
		playStream("submenu_dropdown_select");
		AudioServer.SetBusEffectEnabled(AudioServer.GetBusIndex("Music"), 0, true);
		GetNode<BoxContainer>("MainMenuControl/MainMenuContainer").Set("visible", false);
		GetNode<BoxContainer>("MainMenuControl/SettingsMenuContainer").Set("visible", true);
	}

	public void _on_back_button_pressed()
	{
		playStream("submenu_dropdown_select");
		AudioServer.SetBusEffectEnabled(AudioServer.GetBusIndex("Music"), 0, false);
		GetNode<BoxContainer>("MainMenuControl/MainMenuContainer").Set("visible", true);
		GetNode<BoxContainer>("MainMenuControl/SettingsMenuContainer").Set("visible", false);
	}
	
	//Universal script for hovering over a mouse button
	public void _on_mouse_entered()
	{
		playStream("submenu_scroll");
	}
}
