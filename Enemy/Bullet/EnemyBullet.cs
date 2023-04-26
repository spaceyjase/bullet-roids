using System.Threading.Tasks;
using Godot;
using Shared;

namespace Enemy.Bullet;

public partial class EnemyBullet : BaseBullet
{
    [Export]
    private int speed = 100;

    [Export]
    private PackedScene hitParticle;

    private Vector2 velocity;
    private bool active = true;
    private AudioStreamPlayer2D audioStreamPlayer;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Area2D>("Area2D").AreaEntered += OnArea2d_AreaEntered;
        GetNode<Area2D>("Area2D").BodyEntered += OnArea2d_BodyEntered;
        audioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer");
    }

    private async void OnArea2d_AreaEntered(Node2D area)
    {
        await CollideWith(area);
    }

    private async void OnArea2d_BodyEntered(Node2D body)
    {
        await CollideWith(body);
    }

    private async Task CollideWith(Node2D other)
    {
        if (!active)
            return;
        active = false;
        Visible = false;
        SetProcess(false);
        EventBus.Instance.EmitSignal(EventBus.SignalName.BulletHit, hitParticle, GlobalPosition);
        await AudioCheck();
        QueueFree();
    }

    private async Task AudioCheck()
    {
        if (audioStreamPlayer.Playing)
        {
            await ToSignal(audioStreamPlayer, "finished");
        }
    }

    public override void Start(Vector2 position, float direction)
    {
        Position = position;
        Rotation = direction;
        velocity = new Vector2(0, -speed).Rotated(direction);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Position += velocity * (float)delta;
    }

    private async void OnVisibleOnScreenNotifier2d_ScreenExited()
    {
        if (!active)
            return;
        active = false;
        await AudioCheck();
        QueueFree();
    }
}
