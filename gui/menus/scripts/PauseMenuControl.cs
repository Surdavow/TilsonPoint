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
	private MenuSoundPlayer MenuSoundPlayer;
	public float lerpSpeed = 5f;
	public override void _Ready()
	{		
		EffectsControl = GetParent().GetNode<EffectsControl>("EffectsControl");
		SettingsMenuControl = GetNode<SettingsMenuControl>("SettingsMenuControl");
		MenuMargin = GetNode<MarginContainer>("PauseMenuMargin");		
		MenuMargin.Position = new Vector2(0, -1000);
		MarginTargetPos = MenuMargin.Position;
		MenuSoundPlayer = GetNode<MenuSoundPlayer>("MenuSoundPlayer");
	}

	public override void _Process(double delta)
	{
		if (MenuMargin.Position != MarginTargetPos)
	    {
        	MenuMargin.Position = MenuMargin.Position.Lerp(MarginTargetPos, lerpSpeed * (float)delta);
    	}

		TransitionRectColor = (Color)EffectsControl.TransitionRect.Get("color");
		if(TransitionRectColor.A > 0.995)
		{
			switch((string)GetParent().Get("TransitionTo"))
			{
				case "MainMenu":					
					GetTree().ChangeSceneToFile("res://gui/menus/MainMenu.tscn");
					break;
				case "Quit":
					GetTree().Quit();
					break;
			}			
		}
	}

	public override void _Input(InputEvent @event)
	{
	    if (Input.IsActionJustPressed("Pause"))
	    {
	        MenuSoundPlayer.playStream("submenu_slidein");
	        
			// Open the menu
			if (MenuMargin.Position.Y <= -100)
	        {
				MarginTargetPos = Vector2.Zero;		
	        }
			// Attempt to close the menu
			else
			{
				// If the settings menu is open, close it first, then return to the main menu
				if(SettingsMenuControl.GetNode<MarginContainer>("SettingsMenuMargin").Position.Y > -720)
				{
					SettingsMenuControl.MarginTargetPos = new Vector2(0, -1000);
					MarginTargetPos = Vector2.Zero;
				}
				// If the settings menu is closed, close the pause menu
				else 
				{
					MarginTargetPos = new Vector2(0, -1000);
				}
			}
	    }
	}

	public void _on_options_button_pressed()
	{		
		MenuSoundPlayer.playStream("submenu_dropdown_select");
		EffectsControl.AudioLowPassTarget = 2000;
		MarginTargetPos = new Vector2(0, 1000); // Set the target position
		SettingsMenuControl.MarginTargetPos = Vector2.Zero; // Set the target position
	}

	public void _on_quit_button_pressed()
	{
		MenuSoundPlayer.playStream("submenu_dropdown_select");	
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
		GetParent().Set("TransitionTo", "MainMenu");
	}

	public void _on_mouse_entered()
	{
		MenuSoundPlayer.playStream("submenu_scroll");
	}	
}
