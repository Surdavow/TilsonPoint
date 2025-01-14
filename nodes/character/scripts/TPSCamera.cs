using Godot;
using System;

public class Camera : Node3D
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
			yaw += -mouseEvent.Relative.x * yawSensitivity;
			pitch += mouseEvent.Relative.y * pitchSensitivity;
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		CameraTarget.Rotation = new Vector3(
			Mathf.LerpAngle(CameraTarget.Rotation.x, pitch, delta * 10),
			Mathf.LerpAngle(CameraTarget.Rotation.y, yaw, delta * 10),
			CameraTarget.Rotation.z
		);

		pitch = Mathf.Clamp(pitch, Mathf.Deg2Rad(PitchMin), Mathf.Deg2Rad(PitchMax));

		if (FollowTarget != null)
		{
			GlobalPosition = FollowTarget.GlobalPosition + new Vector3(0, FollowTargetHeightOffset, 0);
		}
	}
}
