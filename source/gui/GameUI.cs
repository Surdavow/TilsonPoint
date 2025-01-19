using Godot;
using System;
using System.IO;

public partial class GameUI : Control
{
	private EffectsControl EffectsControl;
	public override void _Ready()
	{
		// Get the current scene name				
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		EffectsControl.TransitionTo = "start";
		EffectsControl.TransitionRect.fadeIn();		
	}
}
