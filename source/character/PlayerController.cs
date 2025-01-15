using System;
using Godot;

public partial class PlayerController : CharacterBody3D
{
	private Vector3 direction;
	private const float MAX_SPEED = 5.0f;
	private const float ACCELERATION = 15.0f;
	private const float JUMP_VELOCITY = 4.5f;
	private const float TURN_SPEED = 5.0f;
	public Node3D camera_target;
	private float cameraRotationY;
	private bool strafe = true;
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public override void _Ready()
	{
		camera_target = GetNode<Node3D>("CameraControl/CameraTarget");
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
		Vector2 inputDir = Input.GetVector("right", "left", "backward", "forward");
		direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		if (camera_target != null)
		{
			cameraRotationY = camera_target.GlobalTransform.Basis.GetEuler().Y;
		}

		direction = direction.Rotated(Vector3.Up, cameraRotationY);

		if (!IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, Velocity.Y - gravity * (float)delta, Velocity.Z);
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, JUMP_VELOCITY, Velocity.Z);
		}

		Vector3 targetVelocity = direction * MAX_SPEED;

		// Smooth acceleration and deceleration
		Velocity = new Vector3(
			Mathf.MoveToward(Velocity.X, targetVelocity.X, ACCELERATION * (float)delta),
			Velocity.Y,
			Mathf.MoveToward(Velocity.Z, targetVelocity.Z, ACCELERATION * (float)delta)
		);

		if (direction != Vector3.Zero)
		{
			if (strafe)
			{
				Rotation = new Vector3(Rotation.X, Mathf.LerpAngle(Rotation.Y, cameraRotationY, TURN_SPEED * (float)delta), Rotation.Z);
			}
			else
			{
				float targetRotation = Mathf.Atan2(direction.X, direction.Z);
				Rotation = new Vector3(Rotation.X, Mathf.LerpAngle(Rotation.Y, targetRotation, TURN_SPEED * (float)delta), Rotation.Z);
			}
		}

		// Move and slide using updated velocity
		MoveAndSlide();
	}
}
