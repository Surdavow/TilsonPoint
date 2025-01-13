using Godot;
using System;

public partial class TransitionRect : ColorRect
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

	public override void _Ready()
	{
		// Always start with a fully opaque screen, it is transparent in the editor for convenience
		setAlpha(1, true);
	}

	public void setAlpha(float alpha, bool instant)
	{
		if(instant)
		{
			Color = new Color(Color.R, Color.G, Color.B, alpha);
			alphaTarget = alpha;
		}
		else
		{
			alphaTarget = alpha;
		}
	}

	public void fadeOut()
	{
		setAlpha(1, false);
	}

	public void fadeIn()
	{
		setAlpha(0, false);
	}
}