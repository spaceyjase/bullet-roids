using Damageable;
using Godot;
using Shared;

namespace Player;

public partial class Player : Moveable.Moveable
{
    [Signal]
    public delegate void LivesChangedEventHandler(int lives);

    [Signal]
    public delegate void DeadEventHandler();

    [Export]
    private int startingLives = 3;

    [Export]
    private int defaultAmmo = 100;

    [Export]
    private int MovementSpeed { get; set; } = 350;

    [Export]
    private int RotationSpeed { get; set; } = 5;

    [Export]
    private PackedScene BulletScene { get; set; }

    [Export]
    private float FireRate { get; set; } = 0.25f;

    private int collisionCount;
    private Node2D bulletSpawn;
    private Timer bulletCooldownTimer;
    private Timer reloadTimer;
    private bool canShoot = true;
    private GhostTrail sprite;
    private bool isMoving;
    private int lives;
    private bool isDead;
    private bool isInvincible;
    private Timer invincibleTimer;
    private Sprite2D explosion;
    private AnimationPlayer explosionAnimationPlayer;
    private GpuParticles2D hitParticle;
    private AudioStreamPlayer engineSound;
    private int ammo;
    private AudioStreamPlayer reloadSound;

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
            if (value < 0)
            {
                value = 0;
            }

            collisionCount = value;
        }
    }

    private int Lives
    {
        get => lives;
        set
        {
            lives = value;
            EmitSignal(SignalName.LivesChanged, lives);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        bulletCooldownTimer = GetNode<Timer>("BulletCooldownTimer");
        bulletCooldownTimer.WaitTime = FireRate;

        reloadTimer = GetNode<Timer>("ReloadTimer");

        bulletSpawn = GetNode<Node2D>("BulletSpawn");
        sprite = GetNode<GhostTrail>("Sprite2D");

        invincibleTimer = GetNode<Timer>("InvincibleTimer");
        invincibleTimer.Timeout += () =>
        {
            IsActive = true;
            IsInvincible = false;
        };

        explosion = GetNode<Sprite2D>("Explosion");
        explosionAnimationPlayer = explosion.GetNode<AnimationPlayer>("AnimationPlayer");

        hitParticle = GetNode<GpuParticles2D>("HitParticle");

        sprite.Visible = false;

        engineSound = GetNode<AudioStreamPlayer>("EngineSound");
        reloadSound = GetNode<AudioStreamPlayer>("ReloadSound");
    }

    private void OnArea2d_Area_Entered(Node area)
    {
        CollisionCount++;
        HandleCollision(area);
    }

    private void OnArea2d_Body_Entered(Node body)
    {
        CollisionCount++;
        HandleCollision(body);
    }

    private void OnArea2d_Area_Exited(Node area)
    {
        CollisionCount--;
        HandleCollision(area);
    }

    private void OnArea2d_Body_Exited(Node body)
    {
        CollisionCount--;
        HandleCollision(body);
    }

    private void HandleCollision(Node area)
    {
        if (!IsActive)
            return;
        if (IsInvincible || CollisionCount <= 0)
            return;
        if (area is IDamageable damageable)
        {
            damageable.Damage();
        }

        Explode();

        Lives -= 1;
        if (Lives <= 0)
        {
            IsDead = true;
        }
        else
        {
            IsInvincible = true;
        }
    }

    private void Explode()
    {
        engineSound.Stop();
        explosion.Show();
        explosion.GlobalPosition = GlobalPosition;
        explosionAnimationPlayer.Play("explosion");
        EventBus.Instance.EmitSignal(EventBus.SignalName.ImpactEvent, 1);

        hitParticle.GlobalPosition = GlobalPosition;
        hitParticle.Emitting = true;
    }

    public bool IsColliding => CollisionCount > 0;

    public bool IsActive { get; private set; }

    public bool CanShoot
    {
        get => canShoot && HasAmmo;
        private set
        {
            canShoot = value;
            bulletCooldownTimer.Start();
        }
    }

    public bool HasAmmo => Ammo > 0;

    public void Move(Vector2 movement, float rotation, double delta)
    {
        if (Mathf.IsZeroApprox(movement.X) && Mathf.IsZeroApprox(movement.Y))
        {
            IsMoving = false;
            engineSound.Stop();
        }
        else if (!IsMoving)
        {
            IsMoving = true;
            if (!engineSound.Playing)
            {
                engineSound.Play();
            }
        }
        Position += movement * MovementSpeed * (float)delta;
        Rotation += rotation * RotationSpeed * (float)delta;
    }

    public void Fire()
    {
        EventBus.Instance.EmitSignal(
            EventBus.SignalName.Shoot,
            BulletScene,
            bulletSpawn.GlobalPosition,
            Rotation
        );
        CanShoot = false;
        Ammo--;
    }

    private void OnBulletCooldownTimer_Timeout()
    {
        CanShoot = true;
    }

    private void OnReloadCooldownTimer_Timeout()
    {
        CanReload = true;
    }

    public void Start()
    {
        sprite.Show();
        Lives = startingLives;
        IsActive = true;
        IsDead = false;
        IsInvincible = false;
        IsMoving = false;
        CollisionCount = 0;
        Ammo = defaultAmmo;
        CanReload = true;
    }

    private int Ammo
    {
        get => ammo;
        set
        {
            ammo = value;
            if (ammo > 0)
                return;
            ammo = 0;
            reloadSound.Play();
        }
    }

    public bool IsDead
    {
        get => isDead;
        private set
        {
            isDead = value;
            GetNode<CollisionShape2D>("Area2D/CollisionShape2D")
                .CallDeferred("set_disabled", value);
            if (!isDead)
                return;
            sprite.Hide();
            IsActive = false;
            IsMoving = false;
            EmitSignal(SignalName.Dead);
        }
    }

    private bool IsInvincible
    {
        get => isInvincible;
        set
        {
            Ammo = defaultAmmo;
            isInvincible = value;
            GetNode<CollisionShape2D>("Area2D/CollisionShape2D")
                .CallDeferred("set_disabled", value);
            if (!value)
                return;
            GetNode<AnimationPlayer>("AnimationPlayer").Play("invincible");
            IsActive = false;
            invincibleTimer.Start();
        }
    }

    public bool CanReload { get; private set; } = true;

    private void OnAnimation_Player_Animation_Finished(StringName name)
    {
        explosion.Hide();
    }
}
