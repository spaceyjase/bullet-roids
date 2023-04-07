using Godot;

namespace Player;

public partial class Player : Moveable.Moveable
{
  [Signal]
  public delegate void ShootEventHandler(PackedScene bulletScene, Vector2 position, float direction);

  [Export] private int MovementSpeed { get; set; } = 350;
  [Export] private int RotationSpeed { get; set; } = 5;
  [Export] private PackedScene BulletScene { get; set; }

  [Export] private float FireRate { get; set; } = 0.25f;

  private int collisionCount;
  private bool active;
  private Node2D bulletSpawn;
  private Timer bulletCooldownTimer;
  private bool canShoot = true;

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

    Position = screenSize / 2;

    bulletCooldownTimer = GetNode<Timer>("BulletCooldownTimer");
    bulletCooldownTimer.WaitTime = FireRate;

    bulletSpawn = GetNode<Node2D>("BulletSpawn");
  }

  private void OnArea2d_Area_Entered(Area2D area)
  {
    // TODO: bullet check, set property if true
    ++CollisionCount;
    GD.Print($"{area} CollisionCount: {CollisionCount}");
  }

  private void OnArea2d_Area_Exited(Area2D area)
  {
    // TODO: bullet check, set property if true
    --CollisionCount;
    GD.Print($"{area} CollisionCount: {CollisionCount}");
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
}