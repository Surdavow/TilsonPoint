using Godot;
using System;
using System.Collections.Generic;
public partial class SettingsMenuControl : Control
{
	private Control MenuControl;
	private EffectsControl EffectsControl;
	private EffectsSoundPlayer EffectsSoundPlayer;
	private MarginContainer MenuMargin;
	public Vector2 MarginTargetPos;
	public float lerpSpeed = 4f;
	public override void _Ready()
	{
		MenuControl = GetParent<Control>();
		MenuMargin = GetNode<MarginContainer>("SettingsMenuMargin");		

		if(MenuControl.Name == "MainMenuControl")
		{
			EffectsControl = MenuControl.GetNode<EffectsControl>("EffectsControl");
		}
		else
		{			
			EffectsControl = MenuControl.GetParent().GetNode<EffectsControl>("EffectsControl");
		}

		MenuMargin.Position = new Vector2(0, -1000);
		MarginTargetPos = MenuMargin.Position;
		EffectsSoundPlayer = EffectsControl.GetNode<EffectsSoundPlayer>("EffectsSoundPlayer");
	}

	public override void _Process(double delta)
	{
		if(MenuMargin.Position != MarginTargetPos)
		{
			MenuMargin.Position = MenuMargin.Position.Lerp(MarginTargetPos, lerpSpeed * (float)delta);
		}
	}

	//Universal script for hovering over a mouse button
	public void _on_mouse_entered()
	{
		EffectsSoundPlayer.playStream("submenu_scroll");
	}

	public void _on_master_volume_slider_value_changed(float value)
	{
		EffectsSoundPlayer.playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), value <= -40);
	}

	public void _on_music_volume_slider_value_changed(float value)
	{
		EffectsSoundPlayer.playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), value <= -40);
	}

	public void _on_done_button_pressed()
	{
		EffectsSoundPlayer.playStream("submenu_dropdown_select");
		EffectsSoundPlayer.playStream("submenu_slidein");
		MenuControl.Set("MarginTargetPos",Vector2.Zero);
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.Set("AudioLowPassTarget", 20500);
	}

	public void _on_settings_tab_changed(int tab)	
	{
		EffectsSoundPlayer.playStream("submenu_select");
	}

	public void _on_display_driver_button_item_selected(int index)
	{
		EffectsSoundPlayer.playStream("submenu_select");
	}

	public void _on_resolution_button_item_selected(int index)
	{
		EffectsSoundPlayer.playStream("submenu_select");
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
