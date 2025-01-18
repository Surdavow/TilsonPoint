using Godot;
using System;

public partial class CameraControl : Node3D
{
	public Node3D CameraTarget;
	public Node3D FollowTarget;
	public int CameraLerpSpeed = 10;
	public int LookPitch = 85;
	private float yaw = 0;
	private float pitch = 0;
	private float mouseSensitivity = 0.003f;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraTarget = GetNode<Node3D>("CameraTarget");
		FollowTarget = (Node3D)GetParent();
		TopLevel = true;
	}

	public override void _Input(InputEvent @event)
	{
		// Do not process mouse input if the mouse is visible
		if (@event is InputEventMouseMotion mouseEvent && Input.MouseMode != Input.MouseModeEnum.Visible)
		{
			yaw += -mouseEvent.Relative.X * mouseSensitivity;
			pitch += mouseEvent.Relative.Y * mouseSensitivity;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		CameraTarget.Rotation = new Vector3
		(
			Mathf.LerpAngle(CameraTarget.Rotation.X, pitch, (float)delta * CameraLerpSpeed),
			Mathf.LerpAngle(CameraTarget.Rotation.Y, yaw, (float)delta * CameraLerpSpeed),
			CameraTarget.Rotation.Z
		);

		pitch = Mathf.Clamp(pitch, Mathf.DegToRad(-LookPitch), Mathf.DegToRad(LookPitch));

		if (FollowTarget != null)
		{
			GlobalPosition = GlobalPosition.Lerp(FollowTarget.GlobalPosition + new Vector3(0, 1.5f, 0), (float)delta * (CameraLerpSpeed)*1.5f);
		}
	}
}
