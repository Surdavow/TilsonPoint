using Godot;
using System;
using System.Diagnostics;
using System.IO;

public partial class GameControl : Control
{
	private PlayerController PlayerController;
	private EffectsControl EffectsControl;
	private Label DebugLabel;
	public override void _Ready()
	{
		// Get the current scene name				
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		PlayerController = GetParent<PlayerController>();
		EffectsControl.TransitionTo = "start";
		EffectsControl.TransitionRect.fadeIn();	
		DebugLabel = GetNode<Label>("DebugLabel");
	}

    public override void _Process(double delta)
    {
		DebugLabel.Text = "FPS: " + Engine.GetFramesPerSecond() + "\n" + "Player Velocity: " + Math.Ceiling(PlayerController.Velocity.Length());
    }
}
