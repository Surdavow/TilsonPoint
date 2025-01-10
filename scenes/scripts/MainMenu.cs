using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{	    
	private AudioStreamPlayer2D menuSoundPlayer;
	private Label titleLabel;
	private bool Quitting = false;
	private bool isTitleVisible = false;
	private MarginContainer MainMenuControl;
	public Vector2 MainMenuTargetPos;
	private MarginContainer SettingsMenuPos;
	public Vector2 SettingsMenuTargetPos;
	public ColorRect BlackRect;
	public float BlackColorTarget;
	public float MenuMoveSpeed = 4f;
	public float AudioLowPassTarget = 20500;
	private int titleTransitionCount = 0;	
	private MarginContainer SettingsMenuControl;
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };	
	private Dictionary<string, AudioStream> audioStreams;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BlackRect = GetNode<ColorRect>("BlackRect");
		BlackColorTarget = 0;
		MainMenuControl = GetNode<MarginContainer>("MainMenuControl");
		SettingsMenuControl = GetNode<MarginContainer>("SettingsMenuControl");
		SettingsMenuControl.Position = new Vector2(0, -960);
		MainMenuTargetPos = MainMenuControl.Position;
		SettingsMenuTargetPos = SettingsMenuControl.Position;		
		
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
		AudioEffect LowPass = AudioServer.GetBusEffect(AudioServer.GetBusIndex("Music"), 0);
		
		if((float)LowPass.Get("cutoff_hz") != AudioLowPassTarget)
		{
			LowPass.Set("cutoff_hz",Mathf.Lerp((float)LowPass.Get("cutoff_hz"), AudioLowPassTarget, (float)delta*2));
		}		
		
		if (MainMenuControl.Position != MainMenuTargetPos)
	    {
	        Vector2 currentPosition = MainMenuControl.Position;
	        currentPosition = currentPosition.Lerp(MainMenuTargetPos, MenuMoveSpeed * (float)delta); // Adjust speed factor as needed
        	MainMenuControl.Position = currentPosition;
    	}

		if(SettingsMenuControl.Position != SettingsMenuTargetPos)
		{
			Vector2 currentPosition = SettingsMenuControl.Position;
			currentPosition = currentPosition.Lerp(SettingsMenuTargetPos, MenuMoveSpeed * (float)delta);
			SettingsMenuControl.Position = currentPosition;
		}
	
		Color BlackscreenColor = (Color)BlackRect.Get("color");

		if(BlackscreenColor.A != BlackColorTarget)
		{
			BlackscreenColor.A = Mathf.Lerp(BlackscreenColor.A, BlackColorTarget, 2f * (float)delta);
			BlackRect.Set("color", BlackscreenColor);

			if(Quitting)
			{
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), -BlackscreenColor.A*50);
				if(BlackscreenColor.A > 0.99)
				{
					GetTree().Quit();
				}				
			}
		}
		
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
		GetTree().ChangeSceneToFile("res://scenes/Test.tscn");
		playStream("submenu_dropdown_select");
	}

	public void _on_quit_button_pressed()
	{
		playStream("submenu_dropdown_select");	
		
		MainMenuTargetPos = new Vector2(0, -960); // Set the target position
		BlackColorTarget = 1;
		Quitting = true;
	}

	public void _on_resolution_button_item_selected(int index)
	{
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

	public void _on_options_button_pressed()
	{		
		playStream("submenu_dropdown_select");
		MainMenuTargetPos = new Vector2(0, 960); // Set the target position
		SettingsMenuTargetPos = new Vector2(0, 0);
		AudioLowPassTarget = 2000;
	}	

	public void _on_back_button_pressed()
	{
		playStream("submenu_dropdown_select");
		MainMenuTargetPos = new Vector2(0, 0);
		SettingsMenuTargetPos = new Vector2(0,-960);
		AudioLowPassTarget = 20500;
	}	

	public void _on_settings_tab_tab_changed(int tab)
	{
		playStream("submenu_select");
	}
	
	//Universal script for hovering over a mouse button
	public void _on_mouse_entered()
	{
		playStream("submenu_scroll");
	}
}
