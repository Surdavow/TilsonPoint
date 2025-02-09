using Godot;

public partial class VehicleController : VehicleBody3D
{
    [ExportGroup("Car Settings")]
    // Max steer in radians for the front wheels - defaults to 0.45
    [Export] public float MaxSteer { get; set; } = 0.45f;
    
    // The maximum torque that the engine will send to the rear wheels - defaults to 300
    [Export] public float MaxTorque { get; set; } = 300.0f;
    
    // The maximum amount of braking force applied to the wheel. Default is 1.0
    [Export] public float MaxBrakeForce { get; set; } = 1.0f;
    
    // The maximum rear wheel rpm. The actual engine torque is scaled in a linear vector 
    // to ensure the rear wheels will never go beyond this given rpm.
    // The default value is 600rpm
    [Export] public float MaxWheelRpm { get; set; } = 600.0f;
    
    // How quickly the wheel responds to player input - equates to seconds to reach maximum steer. Default is 2.0
    [Export] public float SteerDamping { get; set; } = 2.0f;
    
    // How sticky are the front wheels. Default is 5. 0 is frictionless
    [Export] public float FrontWheelGrip { get; set; } = 5.0f;
    
    // How sticky are the rear wheels. Default is 5. Try lower value for a more drift experience
    [Export] public float RearWheelGrip { get; set; } = 5.0f;

    // Local member variables
    private float _playerAcceleration = 0.0f;
    private float _playerBraking = 0.0f;
    private float _playerSteer = 0.0f;
    private Vector2 _playerInput = Vector2.Zero;

    // An array of driving wheels so we can limit rpm of each wheel when we process input
    private VehicleWheel3D[] _drivingWheels;
    private VehicleWheel3D[] _steeringWheels;

    public override void _Ready()
    {
        // Initialize wheel arrays
        _drivingWheels = new[] {
            GetNode<VehicleWheel3D>("WheelBackLeft"),
            GetNode<VehicleWheel3D>("WheelBackRight")
        };

        _steeringWheels = new[] {
            GetNode<VehicleWheel3D>("WheelFrontLeft"),
            GetNode<VehicleWheel3D>("WheelFrontRight")
        };

        // Set wheel friction slip
        foreach (var wheel in _steeringWheels)
        {
            wheel.WheelFrictionSlip = FrontWheelGrip;
        }

        foreach (var wheel in _drivingWheels)
        {
            wheel.WheelFrictionSlip = RearWheelGrip;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput(delta);
        
        // Process steering and braking
        Steering = _playerSteer;
        Brake = _playerBraking;
        
        // Control each driving wheel individually to limit rpm
        foreach (var wheel in _drivingWheels)
        {
            // Linearly reduce engine force based on the wheels current rpm and the player input
            float actualForce = _playerAcceleration * ((-MaxTorque/MaxWheelRpm) * Mathf.Abs(wheel.GetRpm()) + MaxTorque);
            wheel.EngineForce = actualForce;
        }
    }

    /// <summary>
    /// Sets the variables playerSteer, playerBrake and playerAcceleration based on the player input
    /// </summary>
    private void GetInput(double delta)
    {
        // Steer first
        _playerInput.X = Input.GetAxis("right", "left");
        _playerSteer = Mathf.MoveToward(_playerSteer, _playerInput.X * MaxSteer, (float)(SteerDamping * delta));
        
        // Now acceleration and/or braking
        _playerInput.Y = Input.GetAxis("backward", "forward");
        
        if (_playerInput.Y > 0.01f)
        {
            // Accelerating
            _playerAcceleration = _playerInput.Y;
            _playerBraking = 0.0f;
        }
        else if (_playerInput.Y < -0.01f)
        {
            // We are trying to brake or reverse
            if (GoingForward())
            {
                // Brake
                _playerBraking = -_playerInput.Y * MaxBrakeForce;
                _playerAcceleration = 0.0f;
            }
            else
            {
                // Reverse
                _playerBraking = 0.0f;
                _playerAcceleration = _playerInput.Y;
            }
        }
        else
        {
            _playerAcceleration = 0.0f;
            _playerBraking = 0.0f;
        }
    }

    /// <summary>
    /// Helper function to see if we are moving forward
    /// </summary>
    private bool GoingForward()
    {
        float relativeSpeed = Basis.Z.Dot(LinearVelocity.Normalized());
        return relativeSpeed > 0.01f;
    }
}