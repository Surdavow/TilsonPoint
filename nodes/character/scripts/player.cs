using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private const float SPEED = 5.0f;
	private const float JUMP_VELOCITY = 4.5f;
	private float turn_speed = 15.0f;

	[Export]
	public Node3D camera_target;
	private float camera_T;
	private bool strafe = true;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("strafe"))
		{
			strafe = !strafe;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Add the gravity.
		if (!IsOnFloor())
		{
			Velocity = new Vector3(Velocity.x, Velocity.y - gravity * (float)delta, Velocity.z);
		}

		// Handle jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			Velocity = new Vector3(Velocity.x, JUMP_VELOCITY, Velocity.z);
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 input_dir = Input.GetVector("right", "left", "backward", "forward");
		Vector3 direction = new Vector3(input_dir.x, 0, input_dir.y).Rotated(Vector3.Up, camera_T).Normalized();
		if (direction != Vector3.Zero)
		{
			Velocity = new Vector3(direction.x * SPEED, Velocity.y, direction.z * SPEED);
		}
		else
		{
			Velocity = new Vector3(
				Mathf.MoveToward(Velocity.x, 0, SPEED),
				Velocity.y,
				Mathf.MoveToward(Velocity.z, 0, SPEED)
			);
		}

		if (camera_target != null)
		{
			camera_T = camera_target.GlobalTransform.basis.GetEuler().y;
		}
		else
		{
			return;
		}

		// Rotate player
		if (direction != Vector3.Zero)
		{
			if (strafe)
			{
				Rotation = new Vector3(Rotation.x, Mathf.LerpAngle(Rotation.y, camera_T, (float)delta * turn_speed), Rotation.z);
			}
			else
			{
				Rotation = new Vector3(Rotation.x, Mathf.LerpAngle(Rotation.y, Mathf.Atan2(direction.x, direction.z), turn_speed * (float)delta), Rotation.z);
			}
		}

		MoveAndSlide();
	}
}
