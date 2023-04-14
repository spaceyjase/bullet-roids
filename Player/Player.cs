using Godot;

namespace Player;

public partial class Player : Moveable.Moveable
{
    [Signal]
    public delegate void ShootEventHandler(
        PackedScene bulletScene,
        Vector2 position,
        float direction
    );

    [Export]
    private int MovementSpeed { get; set; } = 350;

    [Export]
    private int RotationSpeed { get; set; } = 5;

    [Export]
    private PackedScene BulletScene { get; set; }

    [Export]
    private float FireRate { get; set; } = 0.25f;

    private int collisionCount;
    private bool active;
    private Node2D bulletSpawn;
    private Timer bulletCooldownTimer;
    private bool canShoot = true;
    private GhostTrail sprite;
    private bool isMoving;

    public bool IsMoving
    {
        get => isMoving;
        set
        {
            isMoving = value;
            sprite.IsEmitting = value;
        }
    }

    private int CollisionCount
    {
        get => collisionCount;
        set
        {
            // clamp to 0
            if (value < 0)
            {
                value = 0;
            }

            collisionCount = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        IsActive = true;

        bulletCooldownTimer = GetNode<Timer>("BulletCooldownTimer");
        bulletCooldownTimer.WaitTime = FireRate;

        bulletSpawn = GetNode<Node2D>("BulletSpawn");
        sprite = GetNode<GhostTrail>("Sprite2D");
    }

    private void OnArea2d_Area_Entered(Area2D area)
    {
        // TODO: Tot!
        GD.Print($"{area} CollisionCount: {CollisionCount}");
    }

    private void OnArea2d_Body_Entered(Node body)
    {
        // TODO: Tot!
        GD.Print($"{body} CollisionCount: {CollisionCount}");
    }

    public bool Colliding => CollisionCount > 0;

    public bool IsActive
    {
        get => active;
        private set
        {
            active = value;
            Visible = active;
        }
    }

    public bool CanShoot
    {
        get => canShoot;
        private set
        {
            canShoot = value;
            bulletCooldownTimer.Start();
        }
    }

    public void Move(Vector2 movement, float rotation, double delta)
    {
        if (Mathf.IsZeroApprox(movement.X) && Mathf.IsZeroApprox(movement.Y))
        {
            IsMoving = false;
        }
        else if (!IsMoving)
        {
            IsMoving = true;
        }
        Position += movement * MovementSpeed * (float)delta;
        Rotation += rotation * RotationSpeed * (float)delta;
    }

    public void Fire()
    {
        EmitSignal(SignalName.Shoot, BulletScene, bulletSpawn.GlobalPosition, Rotation);
        CanShoot = false;
    }

    private void OnBulletCooldownTimer_Timeout()
    {
        CanShoot = true;
    }

    public void Start()
    {
        throw new System.NotImplementedException();
    }
}
