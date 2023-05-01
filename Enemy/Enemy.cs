using System;
using Damageable;
using Godot;
using Shared;

namespace Enemy;

public partial class Enemy : Area2D, IDamageable
{
    [Export]
    private uint score = 100;

    [Export]
    private PackedScene BulletScene { get; set; }

    [Export]
    private int speed = 150;

    [Export]
    private int pulseAmount = 5;

    [Export]
    private float shootDelay = 0.15f;

    [Export]
    private int health = 15;

    private Sprite2D sprite;
    private Sprite2D explosion;
    private AnimationPlayer explosionAnimationPlayer;
    private AnimationPlayer animationPlayer;
    private PathFollow2D follow;
    private int rotateScale;
    private Timer gunTimer;
    private Timer flipTimer;
    private readonly Random random = new();

    public Player.Player Target { get; set; }

    public override void _Ready()
    {
        base._Ready();

        explosion = GetNode<Sprite2D>("Explosion");
        explosionAnimationPlayer = explosion.GetNode<AnimationPlayer>("AnimationPlayer");

        ConfigureAnimationPlayer();

        sprite = GetNode<Sprite2D>("Sprite2D");
        sprite.Frame = GD.RandRange(21, 23);

        gunTimer = GetNode<Timer>("GunTimer");
        gunTimer.Timeout += Shoot;

        flipTimer = GetNode<Timer>("FlipTimer");
        flipTimer.Timeout += Flip;
        Flip();

        // TODO: optimise this; every enemy has paths - could be assigned by GM? (also gamejam)
        var paths = GetNode<Node>("EnemyPaths");
        var path = paths.GetChildren()[GD.RandRange(0, paths.GetChildCount() - 1)];
        follow = new PathFollow2D();
        path.AddChild(follow);
        follow.Loop = false;
    }

    private void Flip()
    {
        flipTimer.WaitTime = GD.RandRange(10, 30);
        flipTimer.Start();
        rotateScale = random.Next(0, 2) * 2 - 1;
        if (animationPlayer.CurrentAnimation == "rotate")
        {
            animationPlayer.SpeedScale = rotateScale;
        }
    }

    private void ConfigureAnimationPlayer()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.AnimationFinished += _ =>
        {
            animationPlayer.SpeedScale = rotateScale;
            animationPlayer.Play("rotate");
        };
        rotateScale = random.Next(0, 2) * 2 - 1;
        animationPlayer.SpeedScale = rotateScale;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        follow.Progress += speed * (float)delta;
        Position = follow.GlobalPosition;
        if (follow.ProgressRatio > 0.99f)
        {
            QueueFree();
        }
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

    private void GameOver()
    {
        gunTimer.Stop();
    }

    private void Explode()
    {
        gunTimer.Stop();
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

    private void Shoot()
    {
        ShootPulse(pulseAmount, shootDelay);
    }

    private async void ShootPulse(int max, double delay)
    {
        for (var i = 0; i < max; ++i)
        {
            foreach (var spawn in sprite.GetChildren())
            {
                if (spawn is not Node2D node)
                    continue;

                EventBus.Instance.EmitSignal(
                    EventBus.SignalName.Shoot,
                    BulletScene,
                    node.GlobalPosition,
                    node.GlobalRotation
                );
            }
            await ToSignal(GetTree().CreateTimer(delay), "timeout");
        }
    }
}
