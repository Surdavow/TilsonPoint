using Godot;
using System;
using System.IO;

public partial class GameUI : Control
{
	private EffectsControl EffectsControl;
	public string sceneName;	
	public override void _Ready()
	{
		// Get the current scene name
		sceneName = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
		
		EffectsControl = GetNode<EffectsControl>("EffectsControl");
		EffectsControl.TransitionRect.fadeIn();
		EffectsControl.TransitionTo = "start";
	}
}
