using System.Threading.Tasks;
using Damageable;
using Godot;
using Shared;

namespace Player.PlayerBullet;

public partial class PlayerBullet : BaseBullet
{
    [Export]
    private PackedScene hitParticle;

    [Export]
    private int speed = 1000;

    private Vector2 velocity;
    private AudioStreamPlayer2D audioStreamPlayer;
    private bool active = true;

    public override void _Ready()
    {
        base._Ready();
        audioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer");
    }

    public override void Start(Vector2 position, float direction)
    {
        Position = position;
        Rotation = direction;
        velocity = new Vector2(0, -speed).Rotated(direction);
    }

    public override void _Process(double delta)
    {
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

    private async Task AudioCheck()
    {
        if (audioStreamPlayer.Playing)
        {
            await ToSignal(audioStreamPlayer, "finished");
        }
    }

    private async void OnArea2d_AreaEntered(Node2D area)
    {
        await CollideWith(area);
    }

    private async Task CollideWith(Node2D other)
    {
        if (!active)
            return;
        active = false;
        if (other is IDamageable damageable)
        {
            damageable.Damage();
        }
        Visible = false;
        SetProcess(false);
        EventBus.Instance.EmitSignal(EventBus.SignalName.BulletHit, hitParticle, GlobalPosition);
        await AudioCheck();
        QueueFree();
    }

    private async void OnArea2d_BodyEntered(Node2D body)
    {
        await CollideWith(body);
    }
}
