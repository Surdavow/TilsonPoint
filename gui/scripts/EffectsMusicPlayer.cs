using Godot;
using System;

public partial class EffectsMusicPlayer : AudioStreamPlayer2D
{	
	public void setMusic(AudioStream stream, bool looping)
	{
		if(stream == null)
		{
			return;
		}

		Stream = stream;
		Set("parameters/looping",looping);
	}
}
