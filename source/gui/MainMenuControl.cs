using Godot;
using System;

public partial class MainMenuControl : Control
{
	private SettingsMenuControl SettingsMenuControl;
	private MarginContainer MenuMargin;
	private TitleLabel MenuMarginTitle;
	private EffectsControl EffectsControl;
	private float lerpSpeed = 4f;
	public Vector2 MarginTargetPos;	
	
	public override void _Ready()
	{		
		// Get the essential nodes for the menu
		MenuMargin = GetNode<MarginContainer>("MainMenuMargin");
		MenuMarginTitle = MenuMargin.GetNode<TitleLabel>("MainMenuContainer/TitleLabel");
		SettingsMenuControl = GetNode<SettingsMenuControl>("SettingsMenuControl");
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		
		// Set some initial values
		EffectsControl.TransitionTo = "start";
		MarginTargetPos = MenuMargin.Position;		
		
		// Perform some initial setup
		EffectsControl.MusicPlayer.setMusic((AudioStream)GD.Load("res://asesets/audio/music/monsters-university-theme.mp3"),true);
		EffectsControl.MusicPlayer.Play();
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
		EffectsControl.SoundPlayer.playStream("submenu_slidein");
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");
		EffectsControl.AudioLowPassTarget = 2000;
		MarginTargetPos = new Vector2(0, 1000); // Set the target position
		SettingsMenuControl.MarginTargetPos = Vector2.Zero; // Set the target position
	}
	// Example button press handlers using the dictionary directly
	public void _on_host_game_button_pressed()
	{		
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");
		EffectsControl.TransitionTo = "HostGame";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_quit_button_pressed()
	{
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");	
		EffectsControl.TransitionTo = "Quit";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_mouse_entered()
	{
		EffectsControl.SoundPlayer.playStream("submenu_scroll");
	}
}