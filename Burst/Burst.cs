using Godot;
using Shared;

namespace Burst;

public partial class Burst : Node2D
{
    [Export]
    private int burstAmount = 10;

    [Export]
    private PackedScene BulletScene { get; set; }

    public void Start(Vector2 position)
    {
        Position = position;

        var step = 2 * Mathf.Pi / burstAmount;
        var angle = 0f;

        for (var i = 0; i < burstAmount; ++i)
        {
            EventBus.Instance.EmitSignal(EventBus.SignalName.Shoot, BulletScene, position, angle);
            angle += step;
        }

        QueueFree();
    }
}