using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenuMargin : MarginContainer
{
	private MasterControl MasterControl;
	private AudioStreamPlayer2D MainMenuSoundPlayer;
	private Dictionary<string, AudioStream> audioStreams;	
	private SettingsMenuMargin SettingsMenuMargin;
	private Label titleLabel;
	public Vector2 TargetPosition;
	public float lerpSpeed = 4f;
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MasterControl = GetTree().Root.GetNode<MasterControl>("MasterControl");
		SettingsMenuMargin = MasterControl.GetNode<SettingsMenuMargin>("SettingsMenuMargin");

		//Load the sound player and initialize the dictionary for the sounds
		MainMenuSoundPlayer = GetNode<AudioStreamPlayer2D>("MainMenuSoundPlayer");
		audioStreams = new Dictionary<string, AudioStream>
		{
			{ "submenu_dropdown_select", (AudioStream)GD.Load("res://audio/menu/submenu_dropdown_select_01.wav") },
			{ "submenu_scroll", (AudioStream)GD.Load("res://audio/menu/submenu_scroll_01.wav") },
			{ "submenu_select", (AudioStream)GD.Load("res://audio/menu/submenu_select_01.wav") },
			{ "submenu_slidein", (AudioStream)GD.Load("res://audio/menu/submenu_slidein_01.wav") }
		};
		
		// Adjust the label, make it transparent
		titleLabel = GetNode<Label>("MainMenuContainer/TitleLabel");	
		foreach (string property in titleFontProperties)
		{
			Color currentColor = (Color)titleLabel.Get($"theme_override_colors/{property}");
			titleLabel.Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, 0));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Position != TargetPosition)
	    {
	        Vector2 currentPosition = Position;
	        currentPosition = currentPosition.Lerp(TargetPosition, lerpSpeed * (float)delta); // Adjust speed factor as needed
        	Position = currentPosition;
    	}
		
		foreach (string property in titleFontProperties)
		{			
			Color currentColor = (Color)titleLabel.Get($"theme_override_colors/{property}");
			if (currentColor.A < 0.99)
			{
				float currentAlpha = currentColor.A;
				currentAlpha = Mathf.Lerp(currentAlpha, 1, 0.5f * (float)delta);
				titleLabel.Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, currentAlpha));		
			}
		}
	}
	public void _on_options_button_pressed()
	{		
		playStream("submenu_dropdown_select");
		MasterControl.Set("AudioLowPassTarget",2000);
		TargetPosition = new Vector2(0, 960); // Set the target position
		SettingsMenuMargin.Set("TargetPosition", new Vector2(0, 0));
	}
	// Example button press handlers using the dictionary directly
	public void _on_host_game_button_pressed()
	{		
		playStream("submenu_dropdown_select");		
		MasterControl.Set("TransitionTo","HostGame");
	}

	public void _on_quit_button_pressed()
	{
		playStream("submenu_dropdown_select");	
		MasterControl.Set("TransitionTo","Quit");
		TargetPosition = new Vector2(0, -960); // Set the target position		
	}

	public void _on_mouse_entered()
	{
		playStream("submenu_scroll");
	}

	public void playStream(string sound)
	{
		if (audioStreams.ContainsKey(sound) && audioStreams[sound] != null)
		{
			MainMenuSoundPlayer.Stream = audioStreams[sound];
			MainMenuSoundPlayer.Play();
		}
		else return;
	}
	
}
