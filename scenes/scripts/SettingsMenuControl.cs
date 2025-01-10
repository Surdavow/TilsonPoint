using Godot;
using System;

public partial class SettingsMenuControl : MarginContainer
{
	private MainMenu MainMenu;
	private MainMenuControl MainMenuControl;
	public Vector2 TargetPosition;
	public float lerpSpeed = 4f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MainMenu = GetParent<MainMenu>();  // Assuming MainMenu is the parent node
		MainMenuControl = MainMenu.GetNode<MainMenuControl>("MainMenuControl");
		Position = new Vector2(0, -960);
		TargetPosition = Position;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Position != TargetPosition)
		{
			Position = new Vector2(Position.X, Mathf.Lerp(Position.Y, TargetPosition.Y, lerpSpeed * (float)delta));
		}
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
		MainMenu.playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), value <= -40);
	}

	public void _on_music_volume_slider_value_changed(float value)
	{
		MainMenu.playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), value <= -40);
	}
	public void _on_back_button_pressed()
	{
		MainMenu.playStream("submenu_dropdown_select");
		MainMenu.Set("AudioLowPassTarget",20500);
		TargetPosition = new Vector2(0, -960);
		MainMenuControl.TargetPosition = new Vector2(0, 0);
	}

	public void _on_settings_tab_tab_changed(int tab)	
	{
		MainMenu.playStream("submenu_select");
	}
	public void _on_window_mode_button_item_selected(int index)
	{
		switch(index)
		{
			// Windowed
			case 0: 
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); 
				break;
			//Fullscreen
			case 1: 
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen); 
				break;
		}
	}
}
