using System;
using Godot;

public partial class PlayerController : CharacterBody3D
{
	private Vector3 direction;	
	private const float accelerateSpeed = 15.0f;
	private const float jumpForce = 4.5f;
	private const float turnSpeed = 3.75f;
	private float cameraRotationY;
	private bool strafe = true;
	private Node3D cameraTarget;
	private AnimationTree animationTree;
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public override void _Ready()
	{
		cameraTarget = GetNode<Node3D>("CameraControl/CameraTarget");
		animationTree = GetNode<AnimationTree>("AnimationTree");
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
		Vector2 inputDir = (Input.MouseMode != Input.MouseModeEnum.Visible) ? Input.GetVector("right", "left", "backward", "forward") : Vector2.Zero;
		float rotationY = (cameraTarget != null) ? cameraTarget.GlobalTransform.Basis.GetEuler().Y : GlobalTransform.Basis.GetEuler().Y;
    
		Vector3 inputDirNormalized = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
		direction = inputDirNormalized.Rotated(Vector3.Up, rotationY);

		if(direction != Vector3.Zero)
		{
			var targetRotation = strafe ? rotationY : Mathf.Atan2(direction.X, direction.Z);
			Rotation = new Vector3(Rotation.X, Mathf.LerpAngle(Rotation.Y, targetRotation, turnSpeed * (float)delta), Rotation.Z);
		}			

		animationTree.Set("parameters/conditions/moving", direction != Vector3.Zero);
		animationTree.Set("parameters/conditions/idle", direction == Vector3.Zero);
		animationTree.Set("parameters/BlendSpace2D/blend_position",new Vector2(-direction.X, direction.Z));

		Velocity = Rotation.Normalized() * animationTree.GetRootMotionPosition()*2 / (float)delta;

		// Move and slide using updated velocity
		MoveAndSlide();
	}
}