using Damageable;
using Godot;
using Player;
using Shared;

namespace Enemy;

public partial class Chaser : Area2D, IDamageable
{
    [Export]
    private uint score = 250;

    [Export]
    private PackedScene BulletScene { get; set; }

    [Export]
    private uint speed = 200;

    [Export]
    private float steering = 50f;

    [Export]
    private int pulseAmount = 5;

    [Export]
    private float shootDelay = 0.15f;

    [Export]
    private int health = 20;

    private Sprite2D sprite;
    private Sprite2D explosion;
    private AnimationPlayer explosionAnimationPlayer;
    private AnimationPlayer animationPlayer;
    private PathFollow2D follow;
    private int rotateScale;
    private Timer gunTimer;
    private Timer flipTimer;
    private Vector2 velocity;
    private Vector2 acceleration;

    private GhostTrail trail;
    private AnimationPlayer turretAnimationPlayer;
    private Node2D turret;
    private AudioStreamPlayer2D chargePlayer;

    public Player.Player Target { get; set; }

    public override void _Ready()
    {
        base._Ready();

        explosion = GetNode<Sprite2D>("Explosion");
        explosionAnimationPlayer = explosion.GetNode<AnimationPlayer>("AnimationPlayer");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        sprite = GetNode<Sprite2D>("Sprite2D");

        gunTimer = GetNode<Timer>("GunTimer");
        gunTimer.Timeout += Shoot;

        trail = GetNode<GhostTrail>("Trail");
        trail.IsEmitting = true;

        turretAnimationPlayer = GetNode<AnimationPlayer>("TurretAnimationPlayer");
        turret = GetNode<Node2D>("Turret");

        chargePlayer = GetNode<AudioStreamPlayer2D>("ChargePlayer");
    }

    private async void Shoot()
    {
        if (Target.IsDead)
            return;

        var targetDirection = (Target.Position - Position).Normalized();
        var dot = targetDirection.Dot(velocity.Normalized());
        if (dot < 0.5f)
            return;

        turretAnimationPlayer.Play("oscillate");

        while (turretAnimationPlayer.IsPlaying())
        {
            await ToSignal(GetTree().CreateTimer(shootDelay), "timeout");
            //var rotation = node.GlobalRotation + GD.RandRange(-0.1f, 0.1f);
            EventBus.Instance.EmitSignal(
                EventBus.SignalName.Shoot,
                BulletScene,
                turret.GlobalPosition,
                turret.GlobalRotation
            );
        }
    }

    public override void _Process(double delta)
    {
        var _delta = (float)delta;
        acceleration += Seek();
        velocity += acceleration * _delta;
        velocity = velocity.LimitLength(speed);
        Rotation = velocity.Angle();
        Position += velocity * _delta;
    }

    private Vector2 Seek()
    {
        var steer = Vector2.Zero;
        if (Target is null || Target.IsDead)
            return steer;

        var desired = (Target.Position - Position).Normalized() * speed;
        steer = (desired - velocity).Normalized() * steering;

        return steer;
    }

    public void Damage()
    {
        if (health < 0)
            return;

        health--;
        if (health < 0)
        {
            Explode();
        }
        else
        {
            animationPlayer.SpeedScale = 1;
            animationPlayer.Play("flash");
        }
    }

    private void Explode()
    {
        trail.Visible = false;
        trail.IsEmitting = false;
        gunTimer.Stop();
        chargePlayer.Autoplay = false;
        chargePlayer.Stop();
        explosion.GlobalPosition = GlobalPosition;
        explosion.TopLevel = true;
        explosionAnimationPlayer.Play("explosion");

        EventBus.Instance.EmitSignal(EventBus.SignalName.EnemyExploded, score, Position);

        CollisionLayer = 0;
        sprite.Hide();
    }

    private void OnAnimation_Player_Animation_Finished(StringName name)
    {
        QueueFree();
    }

    private void OnVisibleOnScreenNotifier2d_ScreenExited()
    {
        if (!Target.IsDead)
            return;

        QueueFree();
    }

    private void GameOver()
    {
        chargePlayer.Autoplay = false;
        chargePlayer.Stop();
        gunTimer.Stop();
    }
}
