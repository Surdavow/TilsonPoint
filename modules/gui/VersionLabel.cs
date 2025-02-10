using Godot;
using System;

public partial class VersionLabel : Label
{
	public override void _Ready()
	{
		Text = "Version: " + ProjectSettings.GetSetting("application/config/version").ToString();
	}
}
