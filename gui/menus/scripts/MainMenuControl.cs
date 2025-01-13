using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class MainMenuControl : Control
{
	private SettingsMenuControl SettingsMenuControl;
	private MarginContainer MenuMargin;
	private Label MainMenuTitle;
	private EffectsControl EffectsControl;
	private MenuSoundPlayer MenuSoundPlayer;
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
		MainMenuTitle = MenuMargin.GetNode<Label>("MainMenuContainer/TitleLabel");
		MarginTargetPos = MenuMargin.Position;		
		SettingsMenuControl = GetNode<SettingsMenuControl>("SettingsMenuControl");
					
		// Hide the title label
		foreach (string property in titleFontProperties)
		{
			Color currentColor = (Color)MainMenuTitle.Get($"theme_override_colors/{property}");
			MainMenuTitle.Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, 0));
		}

		// Load audio streams to be used in the menu
		MenuSoundPlayer = GetNode<MenuSoundPlayer>("MenuSoundPlayer");
		EffectsControl.TransitionRect.fadeIn();		
	}

	public override void _Input(InputEvent @event)
	{
	    if (Input.IsActionJustPressed("Pause"))
	    {	        	        
			// If the settings menu is open, close it first, then return to the main menu
			if(SettingsMenuControl.GetNode<MarginContainer>("SettingsMenuMargin").Position.Y > -720)
			{
				SettingsMenuControl.MarginTargetPos = new Vector2(0, -1000);
				MarginTargetPos = Vector2.Zero;
				MenuSoundPlayer.playStream("submenu_slidein");
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

		if(TransitionTo != null)
		{
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), -TransitionRectColor.A*50);
		}
		
		foreach (string property in titleFontProperties)
		{			
			Color currentColor = (Color)MainMenuTitle.Get($"theme_override_colors/{property}");
			if (currentColor.A < 0.99)
			{
				float currentAlpha = currentColor.A;
				currentAlpha = Mathf.Lerp(currentAlpha, 1, 0.5f * (float)delta);
				MainMenuTitle.Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, currentAlpha));		
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
	// Example button press handlers using the dictionary directly
	public void _on_host_game_button_pressed()
	{		
		MenuSoundPlayer.playStream("submenu_dropdown_select");		
		TransitionTo = "HostGame";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_quit_button_pressed()
	{
		MenuSoundPlayer.playStream("submenu_dropdown_select");	
		TransitionTo = "Quit";
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.TransitionRect.fadeOut();
	}

	public void _on_mouse_entered()
	{
		MenuSoundPlayer.playStream("submenu_scroll");
	}	
}
