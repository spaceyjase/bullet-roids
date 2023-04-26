using System;
using Godot;
using Shared;
using UI;

namespace GameManager;

public partial class GameManager : Node
{
    [Export]
    private NodePath viewportPath;

    [Export]
    private PackedScene roidScene;

    [Export]
    private PackedScene enemyScene;

    [Export]
    private NodePath roidParentPath;

    [Export]
    private NodePath enemyParentPath;

    [Export]
    private NodePath bulletParentPath;

    [Export]
    private NodePath particleParentPath;

    [Export]
    private int initialRoids = 3;

    [Export]
    private int enemySpawnLevel = 3;

    [Export]
    private int baseScore = 10;

    [Export]
    private int maxRoidSize = 5;

    private const int minRoidSize = 3;

    private Camera2D camera;
    private Player.Player player;
    private Vector2 screenSize;
    private PathFollow2D roidSpawner;

    private Node roids;
    private Node enemies;
    private Node bullets;
    private Node particles;
    private SubViewport viewport;

    private int level;

    private int score;
    private int Score
    {
        get => score;
        set
        {
            score = value;
            EventBus.Instance.EmitSignal(EventBus.SignalName.ScoreUpdated, score);
        }
    }

    private bool playing;
    private HUD hud;
    private AudioStreamPlayer levelUpSound;
    private AudioStreamPlayer backgroundMusic;
    private AudioStreamPlayer readyPlayer;
    private AudioStreamPlayer gameOverPlayer;
    private Timer enemyTimer;

    public override void _Ready()
    {
        base._Ready();

        camera = GetTree().GetNodesInGroup("Camera")[0] as Camera2D;
        if (camera is null)
        {
            throw new NullReferenceException("No camera found in group 'Camera'");
        }

        screenSize = camera.GetViewportRect().Size / camera.Zoom;
        roids = GetNode<Node>(roidParentPath);
        enemies = GetNode<Node>(enemyParentPath);
        bullets = GetNode<Node>(bulletParentPath);
        particles = GetNode<Node>(particleParentPath);
        hud = GetNode<HUD>("HUD");
        hud.StartGame += NewGame;

        levelUpSound = GetNode<AudioStreamPlayer>("LevelUpSound");
        backgroundMusic = GetNode<AudioStreamPlayer>("BackgroundMusic");
        readyPlayer = GetNode<AudioStreamPlayer>("ReadyPlayer");
        gameOverPlayer = GetNode<AudioStreamPlayer>("GameOverPlayer");

        enemyTimer = GetNode<Timer>("EnemyTimer");
        enemyTimer.Timeout += SpawnEnemy;

        viewport = GetNode<SubViewport>(viewportPath);

        ConfigureEvents();
        ConfigureRoids();
        ConfigurePlayer();

        Start();
    }

    private void SpawnEnemy()
    {
        var enemy = enemyScene.Instantiate<Enemy.Enemy>();
        enemies.AddChild(enemy);
        enemy.Target = player;
        enemyTimer.WaitTime = GD.RandRange(20, 40); // TODO: balance, tougher enemies later?
        enemyTimer.Start();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!playing)
            return;
        if (roids.GetChildCount() == 0)
        {
            NewLevel();
        }
    }

    private void ConfigureEvents()
    {
        EventBus.Instance.RoidExploded += OnRoid_Exploded;
        EventBus.Instance.EnemyExploded += OnEnemy_Exploded;
        EventBus.Instance.BulletHit += OnBulletHit;
        EventBus.Instance.Shoot += OnShoot;
    }

    private void OnEnemy_Exploded(int enemyScore, Vector2 position)
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.ImpactEvent, 0.5);
        Score += enemyScore * baseScore;
    }

    private void Start()
    {
        GD.Randomize();
        for (var i = 0; i < initialRoids; ++i)
        {
            SpawnRoid();
        }
    }

    private void SpawnRoid(
        int size = minRoidSize,
        Vector2? position = null,
        Vector2? velocity = null
    )
    {
        if (position is null)
        {
            roidSpawner.Progress = GD.Randi();
            position = roidSpawner.Position;
        }

        velocity ??=
            Vector2.Right.Rotated((float)GD.RandRange(0, 2 * Mathf.Pi)) * GD.RandRange(100, 150);

        var roid = roidScene.Instantiate<Roid.Roid>();
        roids.AddChild(roid);

        roid.Start(screenSize, position.Value, velocity.Value, size);
    }

    private void OnRoid_Exploded(int size, int radius, Vector2 position, Vector2 velocity)
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.ImpactEvent, 0.5);

        Score += size * baseScore;
        if (size <= 1)
            return;

        foreach (var offset in new[] { -1, 1 })
        {
            var direction = (position - player.Position).Normalized();
            direction = new Vector2(direction.Y, -direction.X) * offset;
            var newPosition = position + direction * radius;
            var newVelocity = direction * velocity.Length() * 1.1f;
            SpawnRoid(size - 1, newPosition, newVelocity);
        }
    }

    private void ConfigurePlayer()
    {
        player = viewport.GetNode<Player.Player>(nameof(Player));
        player.ScreenSize = screenSize;
        player.LivesChanged += OnPlayer_LivesChanged;
    }

    private void OnPlayer_LivesChanged(int lives)
    {
        if (lives <= 0 || !playing)
            return;

        EventBus.Instance.EmitSignal(EventBus.SignalName.ImpactEvent, 2);
        PlayerReset();
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

    private void OnShoot(PackedScene bulletScene, Vector2 position, float direction)
    {
        var bulletInstance = bulletScene.Instantiate<BaseBullet>();
        bulletInstance.Start(position, direction);
        bullets.AddChild(bulletInstance);
    }

    private void OnBulletHit(PackedScene hitParticle, Vector2 position)
    {
        var instance = hitParticle.Instantiate<Node2D>();
        particles.AddChild(instance);
        instance.GlobalPosition = position;
    }

    private async void NewGame()
    {
        foreach (var roid in roids.GetChildren())
        {
            roid.QueueFree();
        }
        foreach (var bullet in bullets.GetChildren())
        {
            bullet.QueueFree();
        }
        foreach (var particle in particles.GetChildren())
        {
            particle.QueueFree();
        }
        foreach (var enemy in enemies.GetChildren())
        {
            enemy.QueueFree();
        }

        level = 0;
        Score = 0;
        PlayerReset();
        player.Start();
        await ToSignal(hud.MessageTimer, "timeout");
        playing = true;
        backgroundMusic.Play();
        NewLevel();
    }

    private void PlayerReset()
    {
        player.Position = new Vector2(screenSize.X / 2, screenSize.Y * 0.66f);
        player.Rotation = 0;
        hud.ShowMessage("Ready!");
        readyPlayer.Play();
    }

    private void NewLevel()
    {
        DestroyEnemies();

        level++;
        hud.ShowMessage($"Wave {level}");
        for (var i = 0; i < level; ++i)
        {
            var size = minRoidSize;
            if (level > minRoidSize)
            {
                size = GD.RandRange(minRoidSize, level);
                size = Mathf.Clamp(size, minRoidSize, maxRoidSize);
            }
            SpawnRoid(size);
        }
        levelUpSound.Play();
        if (level <= enemySpawnLevel)
            return;
        enemyTimer.WaitTime = GD.RandRange(5, 10);
        enemyTimer.Start();
    }

    private void DestroyEnemies()
    {
        GetTree().CallGroup("enemies", "Explode");
    }

    private void GameOver()
    {
        playing = false;
        backgroundMusic.Stop();
        enemyTimer.Stop();
        hud.GameOver();
        gameOverPlayer.Play();
        GetTree().CallGroup("enemies", "GameOver");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (!playing)
            return;
        if (!@event.IsActionPressed("pause_game"))
            return;

        GetTree().Paused = !GetTree().Paused;
        backgroundMusic.StreamPaused = GetTree().Paused;
        hud.ShowMessage(GetTree().Paused ? "Paused" : string.Empty);
    }
}
