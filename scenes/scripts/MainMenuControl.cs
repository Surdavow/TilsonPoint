using Godot;
using System;

public partial class MainMenuControl : MarginContainer
{
	private MainMenu MainMenu;
	private SettingsMenuControl SettingsMenuControl;
	private Label titleLabel;
	private bool isTitleVisible = false;
	private int titleTransitionCount = 0;
	public Vector2 TargetPosition;
	public float lerpSpeed = 4f;
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MainMenu = GetParent<MainMenu>();
		SettingsMenuControl = MainMenu.GetNode<SettingsMenuControl>("SettingsMenuControl");
		
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
	public void _on_options_button_pressed()
	{		
		MainMenu.playStream("submenu_dropdown_select");
		MainMenu.Set("AudioLowPassTarget",2000);
		TargetPosition = new Vector2(0, 960); // Set the target position
		SettingsMenuControl.Set("TargetPosition", new Vector2(0, 0));
	}
	// Example button press handlers using the dictionary directly
	public void _on_host_game_button_pressed()
	{		
		MainMenu.playStream("submenu_dropdown_select");		
		MainMenu.fadeOut();
		MainMenu.Set("Transition","HostGame");
	}

	public void _on_quit_button_pressed()
	{
		MainMenu.playStream("submenu_dropdown_select");	
		MainMenu.fadeOut();
		MainMenu.Set("Transition","Quit");
		TargetPosition = new Vector2(0, -960); // Set the target position		
	}	
}
