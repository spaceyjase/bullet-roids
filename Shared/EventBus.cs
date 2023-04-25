using System;
using Godot;

namespace Shared;

public partial class EventBus : Node
{
    [Signal]
    public delegate void RoidExplodedEventHandler(
        int size,
        int radius,
        Vector2 position,
        Vector2 linearVelocity
    );

    [Signal]
    public delegate void ScoreUpdatedEventHandler(int newScore);

    [Signal]
    public delegate void ImpactEventEventHandler(float intensity);

    public static EventBus Instance { get; } = new();
}
