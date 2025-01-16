using System;
using Godot;

public partial class PlayerController : CharacterBody3D
{
	private Vector3 direction;
	private const float maxSpeed = 5.0f;
	private const float accelerateSpeed = 15.0f;
	private const float jumpForce = 4.5f;
	private const float turnSpeed = 3.75f;
	public Node3D cameraTarget;
	private float cameraRotationY;
	private bool strafe = true;
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public override void _Ready()
	{
		cameraTarget = GetNode<Node3D>("CameraControl/CameraTarget");
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("strafe"))
		{
			strafe = !strafe;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if(Input.MouseMode != Input.MouseModeEnum.Visible)
		{
			Vector2 inputDir = Input.GetVector("right", "left", "backward", "forward");
			direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
		}
		else 
		{
			direction = Vector3.Zero;
		}

		if (cameraTarget != null)
		{
			cameraRotationY = cameraTarget.GlobalTransform.Basis.GetEuler().Y;
		}

		direction = direction.Rotated(Vector3.Up, cameraRotationY);

		if (!IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, Velocity.Y - gravity * (float)delta, Velocity.Z);
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, jumpForce, Velocity.Z);
		}

		Vector3 targetVelocity = direction * maxSpeed;

		// Smooth accelerateSpeed and deceleration
		Velocity = new Vector3(
			Mathf.MoveToward(Velocity.X, targetVelocity.X, accelerateSpeed * (float)delta),
			Velocity.Y,
			Mathf.MoveToward(Velocity.Z, targetVelocity.Z, accelerateSpeed * (float)delta)
		);

		if (direction != Vector3.Zero)
		{
			float targetRotation = strafe ? cameraRotationY : Mathf.Atan2(direction.X, direction.Z);
			Rotation = new Vector3(Rotation.X, Mathf.LerpAngle(Rotation.Y, targetRotation, turnSpeed * (float)delta), Rotation.Z);
		}

		// Move and slide using updated velocity
		MoveAndSlide();
	}
}
