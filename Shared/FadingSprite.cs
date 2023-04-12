using Godot;

namespace Shared;

public partial class FadingSprite : Sprite2D
{
    [Export]
    private float duration = 0.5f;

    private Tween tween;

    public override void _Ready()
    {
        base._Ready();

        tween = CreateTween();
        tween.Finished += QueueFree;

        SelfModulate *= 0.5f;

        Fade();
    }

    private void Fade()
    {
        var transparent = SelfModulate;
        transparent.A = 0;
        tween.TweenProperty(this, "self_modulate", transparent, duration);
    }
}
