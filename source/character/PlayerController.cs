using System;
using Godot;

public partial class PlayerController : CharacterBody3D
{
	private Vector3 direction;
	private AnimationTree animationTree;
	private const int accelerateSpeed = 4;
	private const int turnSpeed = 2;
	private const float jumpForce = 4.5f;
	private Node3D cameraTarget;
	private float cameraRotationY;
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public override void _Ready()
	{
		cameraTarget = GetNode<Node3D>("CameraControl/CameraTarget");
		animationTree = GetNode<AnimationTree>("AnimationTree");
	}

	public override void _Input(InputEvent @event)
	{

	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 inputDir = Input.MouseMode != Input.MouseModeEnum.Visible ? Input.GetVector("right", "left", "backward", "forward") : Vector2.Zero;
		
		Vector3 velocity = Velocity;
		Vector3 rotation = Rotation;

		rotation = UpdateRotation(rotation, inputDir, delta);
		velocity = UpdateVelocity(velocity, delta);

		Rotation = rotation;
		Velocity = velocity;

		MoveAndSlide();
	}

	private Vector3 UpdateVelocity(Vector3 currentVelocity, double delta)
	{
		UpdateAnimation(delta);

		Vector3 rootMotion = animationTree.GetRootMotionPosition() / (float)delta * 2;
		Vector3 horizontalVelocity = Transform.Basis.GetRotationQuaternion() * rootMotion;

		currentVelocity.Y += IsOnFloor() && Input.IsActionJustPressed("jump") ? jumpForce : -gravity * (float)delta;
		currentVelocity.X = horizontalVelocity.X;
		currentVelocity.Z = horizontalVelocity.Z;

		return currentVelocity;
	}

	private void UpdateAnimation(double delta)
	{
		Vector2 currentBlendPosition = (Vector2)animationTree.Get("parameters/BlendTree/StateMachine/Locomotion/blend_position");
		Vector2 targetBlendPosition = Vector2.Zero;
		int sprintDivider = Input.IsActionPressed("sprint") ? 1 : 2;

		if (direction != Vector3.Zero)
		{
			Basis characterBasis = Basis.FromEuler(new Vector3(0, -Rotation.Y, 0));
			Vector3 localDir = characterBasis * direction / sprintDivider;
			targetBlendPosition = new Vector2(-localDir.X, localDir.Z).Lerp(currentBlendPosition, (float)delta * 2);
		}
		
		Vector2 newBlendPosition = currentBlendPosition.Lerp(targetBlendPosition, (float)delta * accelerateSpeed);
		animationTree.Set("parameters/BlendTree/StateMachine/Locomotion/blend_position", newBlendPosition);
	}

	private Vector3 UpdateRotation(Vector3 currentRotation, Vector2 inputDir,double delta)
	{		
		bool isAiming = Input.IsActionPressed("aim");
		Vector3 inputDirNormalized = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
		cameraRotationY = cameraTarget != null ? cameraTarget.GlobalTransform.Basis.GetEuler().Y : GlobalTransform.Basis.GetEuler().Y;
		direction = inputDirNormalized.Rotated(Vector3.Up, cameraRotationY);		
		float targetRotation = direction == Vector3.Zero ? Rotation.Y : isAiming ? cameraRotationY : Mathf.Atan2(direction.X, direction.Z);
		
		if (direction != Vector3.Zero)
		{
			currentRotation = new Vector3(
				currentRotation.X,
				Mathf.LerpAngle(currentRotation.Y, targetRotation, turnSpeed * (float)delta),
				currentRotation.Z
			);
		}

		return currentRotation;
	}
}