using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Signal]
    public delegate void HitEventHandler();

    // How fast the player moves in meters per second.
    [Export]
    public int Speed { get; set; } = 14;
    // The downward acceleration when in the air, in meters per second squared.
    [Export]
    public int FallAcceleration { get; set; } = 75;
    // Vertical impulse applied to the character upon jumping in meters per second.
    [Export]
    public int JumpImpulse { get; set; } = 20;
    // Vertical impulse applied to the character upon bouncing over a mob in meters per second.
    [Export]
    public int BounceImpulse { get; set; } = 16;

    private Vector3 _targetVelocity = Vector3.Zero;

    private Node3D _pivot;
    private AnimationPlayer _animationPlayer;
    private Area3D _mobDetector;

    public override void _Ready()
    {
        _pivot = GetNode<Node3D>("Pivot");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        // TODO: Handle when GetNode returns null.
    }


    public override void _PhysicsProcess(double delta)
    {
        // Store input direction. Default to (0, 0, 0) beginning per frame.
        Vector3 direction = Vector3.Zero;

        // Check move input. Update vector accordingly.
        if (Input.IsActionPressed("MoveRight"))
            direction.X += 1.0f;
        if (Input.IsActionPressed("MoveLeft"))
            direction.X -= 1.0f;
        if (Input.IsActionPressed("MoveForward"))
            direction.Z -= 1.0f;
        if (Input.IsActionPressed("MoveBack"))
            direction.Z += 1.0f;

        if (direction != Vector3.Zero)
        {
            direction = direction.Normalized();
            _pivot.LookAt(Position + direction, Vector3.Up);

            _animationPlayer.SpeedScale = 4;
        }
        else
        {
            _animationPlayer.SpeedScale = 1;
        }

        _targetVelocity.X = direction.X * Speed;
        _targetVelocity.Z = direction.Z * Speed;

        if (!IsOnFloor())
        {
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }

        if (IsOnFloor() && Input.IsActionJustPressed("Jump"))
            _targetVelocity.Y = JumpImpulse;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);

            if (collision.GetCollider() is Mob mob)
            {
                if (Vector3.Up.Dot(collision.GetNormal()) > 0.1f)
                {
                    mob.Squash();
                    _targetVelocity.Y = BounceImpulse;
                }
            }
        }

        Velocity = _targetVelocity;

        MoveAndSlide();

        // _pivot.Rotation.X = Mathf.Pi/6.0f * Velocity.Y / JumpImpulse;
        // Rotation.X is not a variable, it's a property. Therefore it's not modifyable.
        _pivot.Rotation = new Vector3(Mathf.Pi / 6.0f * Velocity.Y / JumpImpulse, _pivot.Rotation.Y, _pivot.Rotation.Z);
    }

    private void Die()
    {
        EmitSignal(SignalName.Hit);
        QueueFree();
    }

    private void OnMobDetectorBodyEntered(Node body)
    {
        Die();
    }
}
