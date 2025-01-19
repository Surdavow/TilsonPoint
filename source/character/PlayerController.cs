using Godot;
using System;
using System.Net.Http;

public partial class PlayerController : CharacterBody3D
{
	private Vector3 direction;
	private AnimationTree animationTree;
	private Skeleton3D skeleton;
	private bool jumping;
	private bool grounded;
	private const int accelerateSpeed = 4;
	private const int turnSpeed = 4;
	private const int jumpForce = 3;
	private Node3D cameraTarget;
	private float cameraRotationY;
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
	private bool isQuitting = false;
	private Vector3 networkPosition;

	public override void _EnterTree()
	{
		// Set multiplayer authority to the peer ID
		SetMultiplayerAuthority(int.Parse(Name.ToString().Replace("Player", "")));
		GD.Print($"PlayerController entered tree, name: {Name}, authority: {GetMultiplayerAuthority()}");

		// Initialize network position
		networkPosition = GlobalTransform.Origin;
	}

	public override void _Ready()
	{
		if (IsMultiplayerAuthority())
		{
			// Initialize camera control and animation tree for the local player (with authority)
			cameraTarget = GetNode<Node3D>("CameraControl/CameraTarget");
			animationTree = GetNode<AnimationTree>("AnimationTree");
			skeleton = GetNode<Skeleton3D>("RootMotion3D/character_alpha/alpha_rig/Skeleton3D");
			GD.Print($"Initialized player {Name} with authority");
		}
	}

	public override void _ExitTree()
	{
		isQuitting = true;
		base._ExitTree();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Skip processing if we're quitting or multiplayer is not available
		if (isQuitting || !IsInsideTree() || Multiplayer.MultiplayerPeer == null)
		{
			return;
		}

		try
		{
			if (!IsMultiplayerAuthority())
			{
				// If not the authority, just sync position and return
				SyncPlayerPosition();
				return;
			}

			// Handle movement and jumping logic for the player with authority
			Vector2 inputDir = Input.MouseMode != Input.MouseModeEnum.Visible ? new Vector2(
				Input.GetActionStrength("left") - Input.GetActionStrength("right"),
				Input.GetActionStrength("forward") - Input.GetActionStrength("backward")
				) : Vector2.Zero;
			
			Vector3 velocity = Velocity;
			Vector3 rotation = Rotation;

			rotation = UpdateRotation(rotation, inputDir, delta);
			velocity = UpdateVelocity(velocity, delta);

			jumping = Input.IsActionJustPressed("jump");
			grounded = IsOnFloor();

			Rotation = rotation;
			Velocity = velocity;

			MoveAndSlide();

			// After moving, update the network position
			networkPosition = GlobalTransform.Origin;

			// Send position updates to other peers
			Rpc(nameof(UpdatePlayerPosition), networkPosition);
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error in PlayerController physics process: {e.Message}");
		}
	}

	public void jump()
	{
		if (IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, jumpForce, Velocity.Z);
		}
	}

	private Vector3 UpdateVelocity(Vector3 currentVelocity, double delta)
	{
		UpdateAnimation(delta);

		Vector3 rootMotion = animationTree.GetRootMotionPosition() / (float)delta * 2.5f;
		Vector3 horizontalVelocity;

		if (IsOnFloor())
		{
			horizontalVelocity = Transform.Basis.GetRotationQuaternion() * rootMotion;
		}
		else
		{
			// Allow slight air control using the direction vector
			Vector3 airControl = direction * rootMotion.Length() * 0.15f;
			horizontalVelocity = new Vector3(currentVelocity.X + airControl.X, currentVelocity.Y, currentVelocity.Z + airControl.Z);
		}

		currentVelocity.Y += -gravity * (float)delta;
		currentVelocity.X = horizontalVelocity.X;
		currentVelocity.Z = horizontalVelocity.Z;

		return currentVelocity;
	}

	private void UpdateAnimation(double delta)
	{
		Vector2 currentBlendPosition = (Vector2)animationTree.Get("parameters/Locomotion/Main/blend_position");
		Vector2 targetBlendPosition = Vector2.Zero;
		int sprintDivider = Input.IsActionPressed("sprint") ? 1 : 2;

		if (direction != Vector3.Zero)
		{
			Basis characterBasis = Basis.FromEuler(new Vector3(0, -Rotation.Y, 0));
			Vector3 localDir = characterBasis * direction / sprintDivider;
			targetBlendPosition = new Vector2(-localDir.X, localDir.Z).Lerp(currentBlendPosition, (float)delta * 2);
		}
		
		Vector2 newBlendPosition = currentBlendPosition.Lerp(targetBlendPosition, (float)delta * accelerateSpeed);
		animationTree.Set("parameters/Locomotion/Main/blend_position", newBlendPosition);

		int head_bone_index = skeleton.FindBone("mixamorig_Head");
		skeleton.SetBonePoseRotation(head_bone_index, Quaternion.FromEuler(cameraTarget.GlobalTransform.Basis.GetEuler()));
	}

	private Vector3 UpdateRotation(Vector3 currentRotation, Vector2 inputDir, double delta)
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

	// Sync position across all peers
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void UpdatePlayerPosition(Vector3 newPosition)
	{
		if (!IsMultiplayerAuthority())
		{
			// Only update the position if we're not the authority (i.e., client)
			GlobalTransform = new Transform3D(GlobalTransform.Basis, newPosition);
		}
	}

	// Sync player position with all peers, send when authority player moves
	private void SyncPlayerPosition()
	{
		// Send position update
		Rpc(nameof(UpdatePlayerPosition), networkPosition);
	}
}
