using System;
using Godot;
using Shared;

namespace Camera;

public partial class CameraShake : Camera2D
{
    // The maximum offset applied to the camera in units.
    [Export]
    private float maxAmplitude = 0.3f;

    // We made our noise generator in the editor and saved it as a text resource
    // file. This allows us to edit it in the Inspector while the game runs,
    // with live reloading.
    [Export]
    private Resource noiseResource;

    private FastNoiseLite noise;
    private double shakeIntensity;

    // We turn processing on and off through this property's setter function.
    private double ShakeIntensity
    {
        get => shakeIntensity;
        set
        {
            shakeIntensity = Mathf.Clamp(value, 0.0, 1.0);
            SetProcess(!Mathf.IsZeroApprox(shakeIntensity));
        }
    }

    public override void _Ready()
    {
        base._Ready();

        noise = ResourceLoader.Load(noiseResource.ResourcePath) as FastNoiseLite;
        if (noise == null)
            throw new Exception("Failed to load noise resource");

        noise.Seed = (int)GD.Randi();

        EventBus.Instance.ImpactEvent += OnImpactEvent;
    }

    private void OnImpactEvent(float intensity)
    {
        ShakeIntensity = intensity;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // Every frame, we lower the intensity while the effect is active.
        // When the intensity reaches zero, the setter turns off processing.
        ShakeIntensity -= delta;
        // We use the time value to move along the noise generator's axes.
        // Using time gives us a smooth and continuous effect.
        var timeElapsed = Time.GetTicksMsec();
        // We calculate a direction by getting two values from the noise generator.
        var randomDirection = new Vector2(
            noise.GetNoise2D(timeElapsed, 0),
            noise.GetNoise2D(0, timeElapsed)
        ).Normalized();
        // And we apply the shake offset using the current intensity.
        var amplitude = maxAmplitude * Mathf.Pow(ShakeIntensity, 2);
        // Those properties offset the camera's viewport rather than moving the node in the world.
        Offset = new Vector2(
            (float)(randomDirection.X * amplitude),
            (float)(randomDirection.Y * amplitude)
        );
    }
}
