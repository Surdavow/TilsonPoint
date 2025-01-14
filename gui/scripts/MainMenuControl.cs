using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class MainMenuControl : Control
{
	private SettingsMenuControl SettingsMenuControl;
	private MarginContainer MenuMargin;
	private EffectsControl EffectsControl;
	private EffectsSoundPlayer EffectsSoundPlayer;
	private AudioStreamPlayer2D EffectsMusicPlayer;
	private Color TransitionRectColor;
	public string TransitionTo;
	public string sceneName;
	public Vector2 MarginTargetPos;
	public float lerpSpeed = 4f;
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };
	
	public override void _Ready()
	{
		// Get the current scene name
		sceneName = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);

		// Get the essential nodes for the menu
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		MenuMargin = GetNode<MarginContainer>("MainMenuMargin");
		MenuMargin.GetNode<TitleLabel>("MainMenuContainer/TitleLabel").setAlpha(0,true);
		MenuMargin.GetNode<TitleLabel>("MainMenuContainer/TitleLabel").fadeIn();
		MarginTargetPos = MenuMargin.Position;
		SettingsMenuControl = GetNode<SettingsMenuControl>("SettingsMenuControl");	

		EffectsSoundPlayer = EffectsControl.GetNode<EffectsSoundPlayer>("EffectsSoundPlayer");
		EffectsMusicPlayer = EffectsControl.GetNode<AudioStreamPlayer2D>("EffectsMusicPlayer");
		EffectsMusicPlayer.Set("stream", (AudioStream)GD.Load("res://audio/music/monsters-university-theme.mp3"));
		EffectsMusicPlayer.Set("parameters/looping", true);
		EffectsMusicPlayer.Play();
		EffectsControl.TransitionRect.fadeIn();
	}

	public override void _Input(InputEvent @event)
	{
	    if (Input.IsActionJustPressed("Pause") && TransitionTo == null)
	    {	        	
			if(SettingsMenuControl.GetNode<MarginContainer>("SettingsMenuMargin").Position.Y > -720)
			{
				SettingsMenuControl._on_done_button_pressed();
			}			
		}
	}	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (MenuMargin.Position != MarginTargetPos)
	    {
        	MenuMargin.Position = MenuMargin.Position.Lerp(MarginTargetPos, lerpSpeed * (float)delta);
    	}

		TransitionRectColor = (Color)EffectsControl.TransitionRect.Get("color");
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
	public void _on_options_button_pressed()
	{				
		EffectsSoundPlayer.playStream("submenu_slidein");
		EffectsSoundPlayer.playStream("submenu_dropdown_select");
		EffectsControl.Set("AudioLowPassTarget", 2000);
		MarginTargetPos = new Vector2(0, 1000); // Set the target position
		SettingsMenuControl.MarginTargetPos = Vector2.Zero; // Set the target position
	}
	// Example button press handlers using the dictionary directly
	public void _on_host_game_button_pressed()
	{		
		EffectsSoundPlayer.playStream("submenu_dropdown_select");
		TransitionTo = "HostGame";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_quit_button_pressed()
	{
		EffectsSoundPlayer.playStream("submenu_dropdown_select");	
		TransitionTo = "Quit";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_mouse_entered()
	{
		EffectsSoundPlayer.playStream("submenu_scroll");
	}
}