using Godot;
using Godot.Collections;
using System;

public partial class PlayerSound : AudioStreamPlayer3D
{
	private PlayerController PlayerController;
	private Dictionary<string, AudioStream> footsteps;
	public override void _Ready()
	{
		footsteps = new Dictionary<string, AudioStream>
		{
			{ "concrete_grit1", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step1.wav") },
			{ "concrete_grit2", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step2.wav") },
			{ "concrete_grit3", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step3.wav") },
			{ "concrete_grit4", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step4.wav") },
			{ "concrete_grit5", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step5.wav") },
			{ "concrete_grit6", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step6.wav") },
			{ "concrete_grit7", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step7.wav") },
			{ "concrete_grit8", (AudioStream)GD.Load("res://asesets/audio/foley/concrete_grit_step8.wav") },
		};

		PlayerController = GetParent<PlayerController>();
	}

	public void playFootstep()
	{
		if(PlayerController.Velocity.Length() < 0.25f || !PlayerController.IsOnFloor()) return;

		Random rand = new Random();
		int index = rand.Next(1, 8);
		string sound = "concrete_grit" + index;
		playStream(sound);
	}

	public void playStream(string sound)
	{
		if (footsteps.ContainsKey(sound) && footsteps[sound] != null)
		{
			Stream = footsteps[sound];
			Play();
		}
		else return;
	}
}
