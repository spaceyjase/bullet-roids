using Godot;
using Moveable;

namespace Roid;

public partial class Roid : PhysicsMoveable
{
  [Export] private NodePath spritesPath;
  [Export] private float startMass = 1.5f;
  [Export] private double randomAngularVelocity = 1.5f;
  [Export] private float scaleFactor = 0.2f;

  private float size;
  private Sprite2D sprite;

  public override void _Ready()
  {
    base._Ready();

    var sprites = GetNode<Node2D>(spritesPath);
    sprite = sprites.GetChildren()[GD.RandRange(0, sprites.GetChildCount() - 1)] as Sprite2D;
    sprite!.Visible = true;
  }

  public void Start(Vector2 position, Vector2 velocity, float startSize)
  {
    Position = position;
    size = startSize;
    Mass = startMass * size;
    sprite.Scale = Vector2.One * scaleFactor * size;
    radius = (int)(sprite.RegionRect.Size.X * scaleFactor * size / 2);

    var shape = new CircleShape2D();
    shape.Radius = radius;
    GetNode<CollisionShape2D>("CollisionShape2D").Shape = shape;
    LinearVelocity = velocity;
    var angularVelocity = (float)GD.RandRange(-randomAngularVelocity, randomAngularVelocity);
    AngularVelocity = GD.Randf() > 0.5f ? -angularVelocity : angularVelocity;
  }
}