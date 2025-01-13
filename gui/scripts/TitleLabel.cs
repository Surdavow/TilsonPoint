using Godot;
using System;

public partial class TitleLabel : Label
{
	private readonly string[] titleFontProperties = { "font_color", "font_shadow_color", "font_outline_color" };
	private float alphaTarget;

	public override void _Ready()
	{
		Text = ProjectSettings.GetSetting("application/config/name").ToString();			
	}

	public override void _Process(double delta)
	{
		foreach (string property in titleFontProperties)
		{						
			Color currentColor = (Color)Get($"theme_override_colors/{property}");
			float currentAlpha = currentColor.A;

			if (currentAlpha != alphaTarget)
			{
				currentAlpha = Mathf.Lerp(currentAlpha, 1, 0.75f * (float)delta);
				Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, currentAlpha));		
			}
		}
	}

	public void setAlpha(float alpha, bool instant)
	{
		if(instant)
		{
			foreach (string property in titleFontProperties)
			{	
				Color currentColor = (Color)Get($"theme_override_colors/{property}");		
				Set($"theme_override_colors/{property}", new Color(currentColor.R, currentColor.G, currentColor.B, alpha));
			}
		}
		
		alphaTarget = alpha;		
	}

	public void fadeOut()
	{
		setAlpha(0, false);
	}

	public void fadeIn()
	{
		setAlpha(1, false);
	}	
}
