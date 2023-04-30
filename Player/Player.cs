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
    private uint defaultAmmo = 100;

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
    private uint ammo;
    private AudioStreamPlayer reloadSound;
    private Area2D rechargeArea;
    private int charging;
    private GpuParticles2D chargeParticle;
    private AudioStreamPlayer2D chargingSound;

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
        reloadTimer.WaitTime = FireRate;

        chargeParticle = GetNode<GpuParticles2D>("ChargeParticle");
        chargingSound = GetNode<AudioStreamPlayer2D>("ChargeSound");

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

        rechargeArea = GetNode<Area2D>("RechargeArea");
        rechargeArea.BodyEntered += OnRechargeArea_AreaEntered;
        rechargeArea.AreaEntered += OnRechargeArea_AreaEntered;
        rechargeArea.BodyExited += OnRechargeArea_AreaExited;
        rechargeArea.AreaExited += OnRechargeArea_AreaExited;
    }

    private void OnRechargeArea_AreaExited(Node2D node)
    {
        ChargingCount--;
    }

    private void OnRechargeArea_AreaEntered(Node2D node)
    {
        ChargingCount++;
    }

    private void DoCharging()
    {
        chargeParticle.Emitting = IsCharging;
        if (!IsActive)
            return;
        if (Ammo >= defaultAmmo)
        {
            reloadTimer.Stop();
            return;
        }

        if (IsCharging)
        {
            reloadTimer.Start();
        }
        else
        {
            reloadTimer.Stop();
        }
    }

    private bool IsCharging => ChargingCount > 0;

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
        ChargingCount = 0;
        reloadTimer.Stop();
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
        chargingSound.Play();
        Ammo++;
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
        ChargingCount = 0;
        Ammo = defaultAmmo;
        CanReload = true;
        chargeParticle.Emitting = false;
    }

    private int ChargingCount
    {
        get => charging;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            charging = value;
            DoCharging();
        }
    }

    public uint Ammo
    {
        get => ammo;
        private set
        {
            ammo = value;
            if (ammo >= defaultAmmo)
                ammo = defaultAmmo;
            if (ammo <= 0)
                ammo = 0;
            EventBus.Instance.EmitSignal(EventBus.SignalName.AmmoUpdated, Ammo);
            if (ammo > 0)
                return;
            reloadSound.Play();
        }
    }

    public bool IsDead
    {
        get => isDead;
        private set
        {
            isDead = value;
            ConfigureColliders(value);
            if (!isDead)
                return;
            sprite.Hide();
            IsActive = false;
            IsMoving = false;
            EmitSignal(SignalName.Dead);
        }
    }

    private void ConfigureColliders(bool disabled)
    {
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").CallDeferred("set_disabled", disabled);
        GetNode<CollisionShape2D>("RechargeArea/CollisionShape2D")
            .CallDeferred("set_disabled", disabled);
    }

    private bool IsInvincible
    {
        get => isInvincible;
        set
        {
            Ammo = defaultAmmo;
            isInvincible = value;
            ConfigureColliders(value);
            if (!value)
                return;
            GetNode<AnimationPlayer>("AnimationPlayer").Play("invincible");
            IsActive = false;
            invincibleTimer.Start();
        }
    }

    private bool CanReload { get; set; } = true;

    private void OnAnimation_Player_Animation_Finished(StringName name)
    {
        explosion.Hide();
    }
}
