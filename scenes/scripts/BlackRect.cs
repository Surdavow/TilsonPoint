using Godot;
using System;

public partial class BlackRect : ColorRect
{
	public float alphaTarget = 1;
	private int lerpSpeed = 2;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Color.A != alphaTarget)
		{
			Color = new Color(Color.R, Color.G, Color.B, Mathf.Lerp(Color.A, alphaTarget, (float)delta * lerpSpeed));
		}
	}
}
