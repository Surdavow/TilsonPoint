using Godot;
using System;
using System.Collections.Generic;
public partial class SettingsMenuControl : Control
{
	private Control MenuControl;
	private EffectsControl EffectsControl;
	private MarginContainer MenuMargin;
	private float lerpSpeed = 4f;
	public Vector2 MarginTargetPos;

	public override void _Ready()
	{	
		MenuControl = GetParent<Control>();
		MenuMargin = GetNode<MarginContainer>("SettingsMenuMargin");		

		var parentControl = (MenuControl.Name == "MainMenuControl") ? MenuControl : MenuControl.GetParent<Control>();
		EffectsControl = parentControl.GetNode<EffectsControl>("EffectsControl");

		MenuMargin.Position = new Vector2(0, -1000);
		MarginTargetPos = MenuMargin.Position;		
	}

	public override void _Process(double delta)
	{		
		Visible = MenuMargin.Position.Y > -648;		
		
		if(MenuMargin.Position != MarginTargetPos)
		{
			MenuMargin.Position = MenuMargin.Position.Lerp(MarginTargetPos, lerpSpeed * (float)delta);
		}
	}

	//Universal script for hovering over a mouse button
	public void _on_mouse_entered()
	{
		EffectsControl.SoundPlayer.playStream("submenu_scroll");
	}

	public void _on_master_volume_slider_value_changed(float value)
	{
		setChannelVolume("Master", value);
	}

	public void _on_music_volume_slider_value_changed(float value)
	{
		setChannelVolume("Music", value);
	}

	public void setChannelVolume(string channel, float value)
	{
		if(EffectsControl.TransitionTo == "start")
		{
			return;
		}

		EffectsControl.SoundPlayer.playStream("submenu_slidein");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(channel), value);
		AudioServer.SetBusMute(AudioServer.GetBusIndex(channel), value <= -25);
	}

	public void _on_done_button_pressed()
	{
		EffectsControl.SoundPlayer.playStream("submenu_dropdown_select");
		EffectsControl.SoundPlayer.playStream("submenu_slidein");
		MenuControl.Set("MarginTargetPos",Vector2.Zero);
		MarginTargetPos = new Vector2(0, -1000);
		EffectsControl.AudioLowPassTarget = 20500;
	}

	public void _on_settings_tab_changed(int tab)	
	{
		EffectsControl.SoundPlayer.playStream("submenu_select");
	}

	public void _on_resolution_slider_value_changed(float value)
	{
		EffectsControl.SoundPlayer.playStream("submenu_slidein");
		GetViewport().Scaling3DScale = (float)value;
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
