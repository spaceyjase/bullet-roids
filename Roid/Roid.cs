using Damageable;
using Godot;
using Moveable;

namespace Roid;

public partial class Roid : PhysicsMoveable, IDamageable
{
    [Signal]
    public delegate void ExplodedEventHandler(
        int size,
        int radius,
        Vector2 position,
        Vector2 linearVelocity
    );

    [Export]
    private int baseHealth = 3;

    [Export]
    private NodePath spritesPath;

    [Export]
    private float startMass = 1.5f;

    [Export]
    private double randomAngularVelocity = 1.5f;

    [Export]
    private float scaleFactor = 0.2f;

    private float size;
    private Sprite2D sprite;
    private int health;
    private Sprite2D explosion;
    private AnimationPlayer explosionAnimationPlayer;

    public override void _Ready()
    {
        base._Ready();

        var sprites = GetNode<Node2D>(spritesPath);
        sprite = sprites.GetChildren()[GD.RandRange(0, sprites.GetChildCount() - 1)] as Sprite2D;
        sprite!.Visible = true;

        explosion = GetNode<Sprite2D>("Explosion");
        explosionAnimationPlayer = explosion.GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Start(Vector2 screenSize, Vector2 position, Vector2 velocity, float startSize)
    {
        ScreenSize = screenSize;
        Position = position;
        size = startSize;
        Mass = startMass * size;
        sprite.Scale = Vector2.One * scaleFactor * size;
        radius = (int)(sprite.RegionRect.Size.X * scaleFactor * size / 2);
        explosion.Scale = new Vector2(0.75f, 0.75f) * size;

        health = (int)(baseHealth * startSize);

        CallDeferred(nameof(SetShape), velocity);
    }

    private void SetShape(Vector2 velocity)
    {
        var shape = new CircleShape2D();
        shape.Radius = radius;
        GetNode<CollisionShape2D>("CollisionShape2D").Shape = shape;
        LinearVelocity = velocity;
        var angularVelocity = (float)GD.RandRange(-randomAngularVelocity, randomAngularVelocity);
        AngularVelocity = GD.Randf() > 0.5f ? -angularVelocity : angularVelocity;
    }

    public void Damage()
    {
        if (health < 0)
            return; // I might already be dead; bullets move fast ;)

        health--;
        if (health < 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        CollisionLayer = 0;
        sprite.Hide();
        explosionAnimationPlayer.Play("explosion");

        EmitSignal(SignalName.Exploded, size, radius, Position, LinearVelocity);

        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0;
    }
}
