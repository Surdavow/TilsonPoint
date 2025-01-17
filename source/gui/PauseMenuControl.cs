using Godot;
using Godot.Collections;
using System;

public partial class PauseMenuControl : Control
{
	private MarginContainer MenuMargin;
	private SettingsMenuControl SettingsMenuControl;
	private EffectsControl EffectsControl;
	private Color TransitionRectColor;
	public Vector2 MarginTargetPos;
	public float lerpSpeed = 5f;
	public override void _Ready()
	{		
		EffectsControl = GetParent().GetNode<EffectsControl>("EffectsControl");
		SettingsMenuControl = GetNode<SettingsMenuControl>("SettingsMenuControl");
		MenuMargin = GetNode<MarginContainer>("PauseMenuMargin");
		MenuMargin.Position = new Vector2(0, -1000);
		MarginTargetPos = MenuMargin.Position;	

		// If the mouse is visible, hide it
		if (Input.MouseMode == Input.MouseModeEnum.Visible)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	public override void _Process(double delta)
	{
		if (MenuMargin.Position != MarginTargetPos)
	    {
        	MenuMargin.Position = MenuMargin.Position.Lerp(MarginTargetPos, lerpSpeed * (float)delta);
    	}
	}

	public override void _Input(InputEvent @event)
	{		
		if (Input.IsActionJustPressed("pause"))
		{
			// Open the menu
			if (MenuMargin.Position.Y <= -100)
	        {
				Input.MouseMode = Input.MouseModeEnum.Visible;
				EffectsControl.SoundPlayer.playStream("submenu_slidein");
				MarginTargetPos = Vector2.Zero;		
	        }
			// Attempt to close the menu
			else
			{
				// If the settings menu is open, close it first, then return to the main menu
				if(SettingsMenuControl.GetNode<MarginContainer>("SettingsMenuMargin").Position.Y > -720)
				{
					SettingsMenuControl._on_done_button_pressed();
				}
				// If the settings menu is closed, close the pause menu
				else 
				{	
					Input.MouseMode = Input.MouseModeEnum.Captured;
					EffectsControl.SoundPlayer.playStream("submenu_slidein");
					MarginTargetPos = new Vector2(0, -1000);
				}
			}
	    }
	}

	public void _on_options_button_pressed()
	{		
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");
		EffectsControl.Set("AudioLowPassTarget", 2000);
		MarginTargetPos = new Vector2(0, 1000); // Set the target position
		SettingsMenuControl.MarginTargetPos = Vector2.Zero; // Set the target position
	}

	public void _on_disconnect_button_pressed()
	{
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");	
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
		EffectsControl.TransitionTo = "MainMenu";
	}

	public void _on_mouse_entered()
	{
		EffectsControl.SoundPlayer.playStream("submenu_scroll");
	}

	public void _on_quit_button_pressed()
	{
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");	
		EffectsControl.TransitionTo = "Quit";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}
}
