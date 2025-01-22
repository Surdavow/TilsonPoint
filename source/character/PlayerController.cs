using Godot;
using System;
using System.Net.Http;

public partial class PlayerController : CharacterBody3D
{
	private Vector3 direction;
	private AnimationTree animationTree;
	private Skeleton3D skeleton;
	private bool grounded;
	private const int accelerateSpeed = 4;
	private const int jumpForce = 4;
	private float lastLandTime;
	private float LandBlendTarget;
	private float landImpactForce;
	private Node3D cameraTarget;
	private float cameraRotationY;
	private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
	private bool isQuitting = false;

	public override void _Ready()
	{
		// Set multiplayer authority to the peer ID
		SetMultiplayerAuthority(int.Parse(Name.ToString().Replace("Player", "")));

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
		if (!IsMultiplayerAuthority() || isQuitting || !IsInsideTree() || Multiplayer.MultiplayerPeer == null)
		{
			return;
		}

		if(lastLandTime == 0)
		{
			lastLandTime = Engine.GetPhysicsFrames() * (float)delta;
		}

		// Handle movement and jumping logic for the player with authority
		Vector2 inputDir = Input.MouseMode != Input.MouseModeEnum.Visible ? new Vector2(
			Input.GetActionStrength("left") - Input.GetActionStrength("right"),
			Input.GetActionStrength("forward") - Input.GetActionStrength("backward")
			) : Vector2.Zero;
		
		Vector3 velocity = Velocity;
		Vector3 rotation = Rotation;

		if (!IsOnFloor())
		{
			landImpactForce = velocity.Y;
		}		

		rotation = UpdateRotation(rotation, inputDir, delta);		
		velocity = UpdateVelocity(velocity, delta);

		float LandBlend = (float)animationTree.Get("parameters/LandBlend/blend_amount");				
		bool wasGrounded = grounded;
		grounded = IsOnFloor();

		if (!wasGrounded && grounded)
		{			
			lastLandTime = Engine.GetPhysicsFrames() * (float)delta;			
			animationTree.Set("parameters/LandShot/request", true);
			animationTree.Set("parameters/JumpTransition/transition_request","Move");
			LandBlendTarget = Mathf.Clamp(Math.Abs(landImpactForce) / 5, 0.05f, 0.95f);			
		}
		else if (Engine.GetPhysicsFrames() * (float)delta - lastLandTime >= 0.5f)
		{
			animationTree.Set("parameters/LandBlend/blend_amount", Mathf.Lerp(LandBlend, 0, (float)delta));	
			LandBlendTarget	= 0;			
		}

		if(Input.IsActionJustPressed("jump"))
		{
			string jumpType = grounded && direction != Vector3.Zero ? "Run Jump" : "Jump";
			animationTree.Set("parameters/JumpTransition/transition_request",jumpType);
		}

		animationTree.Set("parameters/FallingBlend/blend_amount", Mathf.Lerp((float)animationTree.Get("parameters/FallingBlend/blend_amount"), grounded ? 0 : 1, (float)delta * 5));
		animationTree.Set("parameters/LandBlend/blend_amount", Mathf.Lerp(LandBlend, LandBlendTarget, (float)delta * 3));

		Rotation = rotation;
		Velocity = velocity;

		MoveAndSlide();
	}

	public void Jump()
	{
		if (IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, jumpForce, Velocity.Z);
		}
	}

	private Vector3 UpdateVelocity(Vector3 currentVelocity, double delta)
	{
		// Get the current blend position and timescale from the animation tree
		Vector2 currentBlendPosition = (Vector2)animationTree.Get("parameters/Movement/blend_position");
		float currentTimescale = (float)animationTree.Get("parameters/TimeScale/scale");
		
		// Determine if the player is sprinting
		int sprintDivider = Input.IsActionPressed("sprint") ? 1 : 2;		
		Vector2 targetBlendPosition = Vector2.Zero;
		float targetTimescale = 1.0f;		

		if (direction != Vector3.Zero)
		{			
			// Calculate the local direction based on the player's rotation
			Basis characterBasis = Basis.FromEuler(new Vector3(0, -Rotation.Y, 0));			
			Vector3 localDir = characterBasis * direction / sprintDivider;
			// Smoothly interpolate the blend position towards the target
			targetBlendPosition = new Vector2(-localDir.X, localDir.Z).Lerp(currentBlendPosition, (float)delta * 2);

			// Calculate the slope angle and direction
			float inclineDotProduct = GetFloorNormal().Dot(Vector3.Up);
			float slopeAngle = Mathf.Acos(inclineDotProduct);
			Vector3 slopeDirection = GetFloorNormal().Cross(Vector3.Up).Cross(GetFloorNormal()).Normalized();
			float slopeDotProduct = slopeDirection.Dot(direction.Normalized());
			inclineDotProduct = slopeDotProduct * Mathf.Sin(slopeAngle);
			
			if (slopeDotProduct < -0.5f) // If the player is moving downhill, give them a slight speed boost
			{
				targetTimescale = 1.0f + Mathf.Abs(slopeDotProduct) * 0.375f;

				// Adjust the player's velocity to stick to the floor normal
				if (IsOnFloor())
				{
					Vector3 floorPoint = GlobalTransform.Origin - GetFloorNormal() * (GlobalTransform.Origin.Y - GetFloorNormal().Y);
					Velocity = new Vector3(Velocity.X, floorPoint.Y, Velocity.Z);
				}
			}
			else // Otherwise, adjust the timescale based on the incline if the player is moving uphill
			{
				// If the slope is too steep, reset the timescale immediately, otherwise smoothly interpolate it on flatter surfaces
				targetTimescale = inclineDotProduct > 0.5 ? 1.0f : Mathf.Lerp(currentTimescale, 1.0f, (float)delta * accelerateSpeed);
			}

			// Reduce the target blend position based on the incline
			targetBlendPosition *= Mathf.Clamp(1.0f - inclineDotProduct*0.5f, 0.1f, 1.0f);

			// Stop the player from moving if they're against a wall, use the wall dot product and direction to adjust the blend position
			// This will make the player slide along the wall instead of stopping abruptly, so they can still move if they're not directly running into it
			if (IsOnWall())
			{					
				float wallDotProduct = GetWallNormal().Dot(direction);
				targetBlendPosition *= Mathf.Clamp(1 + wallDotProduct, 0, 1);
				targetTimescale = 1.0f;
			}
		}
		
		// Set the updated blend position and timescale in the animation tree
		animationTree.Set("parameters/Movement/blend_position", currentBlendPosition.Lerp(targetBlendPosition, (float)delta * accelerateSpeed));
		animationTree.Set("parameters/TimeScale/scale", targetTimescale);

		Vector3 rootMotion = animationTree.GetRootMotionPosition() / (float)delta * 3;
		Vector3 horizontalVelocity = Transform.Basis.GetRotationQuaternion() * rootMotion;

		if (!IsOnFloor())
		{
			horizontalVelocity = new Vector3(currentVelocity.X + (direction.X*0.05f), currentVelocity.Y, currentVelocity.Z + (direction.Z*0.05f));
		}

		currentVelocity.Y += -gravity * (float)delta;
		currentVelocity.X = horizontalVelocity.X;
		currentVelocity.Z = horizontalVelocity.Z;

		return currentVelocity;		
	}

	private Vector3 UpdateRotation(Vector3 currentRotation, Vector2 inputDir, double delta)
	{        		
		Vector3 inputDirNormalized = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
		cameraRotationY = cameraTarget != null ? cameraTarget.GlobalTransform.Basis.GetEuler().Y : GlobalTransform.Basis.GetEuler().Y;
		direction = inputDirNormalized.Rotated(Vector3.Up, cameraRotationY);
		float targetRotation = direction == Vector3.Zero ? Rotation.Y : Mathf.Atan2(direction.X, direction.Z);
		float turnSpeed = IsOnFloor() ? 4 : 2;
		bool isAiming = Input.IsActionPressed("aim");		
		
		if (direction != Vector3.Zero || isAiming)
		{
			currentRotation = new Vector3(
				currentRotation.X,
				Mathf.LerpAngle(currentRotation.Y, isAiming ? cameraRotationY : targetRotation, turnSpeed * (float)delta),
				currentRotation.Z
			);
		}

		return currentRotation;
	}
}