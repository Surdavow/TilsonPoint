using Godot;
using System;

public partial class CameraControl : Node3D
{
	[Export] public Node3D CameraTarget;
	[Export] public Node3D FollowTarget;
	[Export] public float FollowTargetHeightOffset = 1.5f;
	[Export] public float PitchMax = 50;
	[Export] public float PitchMin = -50;
	private float yaw = 0;
	private float pitch = 0;
	private float yawSensitivity = 0.002f;
	private float pitchSensitivity = 0.002f;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		TopLevel = true;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("pause") && @event.IsPressed())
		{
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		if (@event is InputEventMouseMotion mouseEvent && Input.MouseMode != Input.MouseModeEnum.Visible)
		{
			yaw += -mouseEvent.Relative.X * yawSensitivity;
			pitch += mouseEvent.Relative.Y * pitchSensitivity;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		CameraTarget.Rotation = new Vector3
		(
			Mathf.LerpAngle(CameraTarget.Rotation.X, pitch, (float)delta * 10),
			Mathf.LerpAngle(CameraTarget.Rotation.Y, yaw, (float)delta * 10),
			CameraTarget.Rotation.Z
		);

		pitch = Mathf.Clamp(pitch, Mathf.DegToRad(PitchMin), Mathf.DegToRad(PitchMax));

		if (FollowTarget != null)
		{
			GlobalPosition = FollowTarget.GlobalPosition + new Vector3(0, FollowTargetHeightOffset, 0);
		}
	}
}
