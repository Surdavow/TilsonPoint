using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class MainMenuControl : Control
{
	private SettingsMenuControl SettingsMenuControl;
	private MarginContainer MenuMargin;
	private TitleLabel MenuMarginTitle;
	private EffectsControl EffectsControl;
	private EffectsSoundPlayer EffectsSoundPlayer;
	private EffectsMusicPlayer EffectsMusicPlayer;
	private Color TransitionRectColor;
	public string sceneName;
	public Vector2 MarginTargetPos;
	public float lerpSpeed = 4f;
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };
	
	public override void _Ready()
	{
		// Get the essential nodes for the menu		
		MenuMargin = GetNode<MarginContainer>("MainMenuMargin");
		MenuMarginTitle = MenuMargin.GetNode<TitleLabel>("MainMenuContainer/TitleLabel");
		SettingsMenuControl = GetNode<SettingsMenuControl>("SettingsMenuControl");
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		EffectsSoundPlayer = EffectsControl.GetNode<EffectsSoundPlayer>("EffectsSoundPlayer");
		EffectsMusicPlayer = EffectsControl.GetNode<EffectsMusicPlayer>("EffectsMusicPlayer");
		
		// Set some initial values
		EffectsControl.TransitionTo = "start";
		MarginTargetPos = MenuMargin.Position;		
		
		// Perform some initial setup
		EffectsMusicPlayer.setMusic((AudioStream)GD.Load("res://asesets/audio/music/monsters-university-theme.mp3"),true);
		EffectsMusicPlayer.Play();
		EffectsControl.TransitionRect.fadeIn();
		MenuMarginTitle.setAlpha(0, true);
		MenuMarginTitle.fadeIn();

		// If the mouse is hidden, release it
		if(Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _Input(InputEvent @event)
	{
	    if (Input.IsActionJustPressed("pause"))
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
		EffectsControl.TransitionTo = "HostGame";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_quit_button_pressed()
	{
		EffectsSoundPlayer.playStream("submenu_dropdown_select");	
		EffectsControl.TransitionTo = "Quit";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_mouse_entered()
	{
		EffectsSoundPlayer.playStream("submenu_scroll");
	}
}