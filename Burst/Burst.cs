using Godot;
using Shared;

public partial class Burst : Node2D
{
    [Export]
    private PackedScene BulletScene { get; set; }

    public void Start(Vector2 position)
    {
        Position = position;
        foreach (var spawn in GetChildren())
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

        QueueFree();
    }
}
