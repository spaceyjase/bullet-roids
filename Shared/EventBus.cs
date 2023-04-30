using System;
using Godot;

namespace Shared;

public partial class EventBus : Node
{
    [Signal]
    public delegate void RoidExplodedEventHandler(
        uint size,
        int radius,
        Vector2 position,
        Vector2 linearVelocity
    );

    [Signal]
    public delegate void ShootEventHandler(
        PackedScene bulletScene,
        Vector2 position,
        float direction
    );

    [Signal]
    public delegate void BulletHitEventHandler(PackedScene hitParticle, Vector2 position);

    [Signal]
    public delegate void EnemyExplodedEventHandler(uint score, Vector2 position);

    [Signal]
    public delegate void ScoreUpdatedEventHandler(uint newScore);

    [Signal]
    public delegate void AmmoUpdatedEventHandler(uint newAmmo);

    [Signal]
    public delegate void HighScoreUpdatedEventHandler(uint newScore);

    [Signal]
    public delegate void ImpactEventEventHandler(float intensity);

    public static EventBus Instance { get; } = new();
}
