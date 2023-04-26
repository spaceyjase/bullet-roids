using Godot;

namespace Shared;

public abstract partial class BaseBullet : Node2D
{
    public abstract void Start(Vector2 position, float direction);
}
