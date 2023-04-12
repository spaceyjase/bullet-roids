using System;
using Godot;
using Player.PlayerBullet;

namespace GameManager;

public partial class GameManager : Node
{
    [Export]
    private NodePath viewportPath;

    [Export]
    private PackedScene roidScene;

    [Export]
    private NodePath roidParentPath;

    [Export]
    private NodePath bulletParentPath;

    [Export]
    private NodePath particleParentPath;

    [Export]
    private int initialRoids = 3;

    private Camera2D camera;
    private Player.Player player;
    private Vector2 screenSize;
    private PathFollow2D roidSpawner;

    private Node roids;
    private Node bullets;
    private Node particles;
    private SubViewport viewport;

    public override void _Ready()
    {
        base._Ready();

        camera = GetTree().GetNodesInGroup("Camera")[0] as Camera2D;
        if (camera is null)
        {
            throw new NullReferenceException("No camera found in group 'Camera'");
        }

        screenSize = camera!.GetViewportRect().Size / camera.Zoom;
        roids = GetNode<Node>(roidParentPath);
        bullets = GetNode<Node>(bulletParentPath);
        particles = GetNode<Node>(particleParentPath);

        viewport = GetNode<SubViewport>(viewportPath);

        ConfigureRoids();
        ConfigurePlayer();

        Start();
    }

    private void Start()
    {
        GD.Randomize();
        for (var i = 0; i < initialRoids; ++i)
        {
            SpawnRoid(initialRoids);
        }
    }

    private void SpawnRoid(int size, Vector2? position = null, Vector2? velocity = null)
    {
        if (position is null)
        {
            roidSpawner.HOffset = GD.Randi();
            position = roidSpawner.Position;
        }

        velocity ??=
            Vector2.Right.Rotated((float)GD.RandRange(0, 2 * Mathf.Pi)) * GD.RandRange(100, 150);

        var roid = roidScene.Instantiate<Roid.Roid>();
        roids.AddChild(roid);

        roid.Start(screenSize, position.Value, velocity.Value, size);
    }

    private void ConfigurePlayer()
    {
        player = viewport.GetNode<Player.Player>(nameof(Player));
        player.ScreenSize = screenSize;

        player.Position = screenSize / 2;
    }

    private void ConfigureRoids()
    {
        // Configure the roid spawn curve
        var curve = new Curve2D();
        curve.AddPoint(Vector2.Zero);
        curve.AddPoint(new Vector2(screenSize.X, 0f));
        curve.AddPoint(screenSize);
        curve.AddPoint(new Vector2(0f, screenSize.Y));
        curve.AddPoint(Vector2.Zero);

        // Add it to the path
        var path = new Path2D();
        path.Curve = curve;
        viewport.AddChild(path);

        // And the spawner itself
        roidSpawner = new PathFollow2D();
        roidSpawner.Loop = true;
        path.AddChild(roidSpawner);
    }

    private void OnPlayer_Shoot(PackedScene bulletScene, Vector2 position, float direction)
    {
        var bulletInstance = bulletScene.Instantiate<PlayerBullet>();
        bulletInstance.BulletHit += OnBulletHit;
        bulletInstance.Start(position, direction);
        bullets.AddChild(bulletInstance);
    }

    private void OnBulletHit(PackedScene hitParticle, Vector2 position)
    {
        var instance = hitParticle.Instantiate<Node2D>();
        particles.AddChild(instance);
        instance.GlobalPosition = position;
    }
}
