using System.Threading.Tasks;
using Godot;

namespace Shared;

public partial class EffectsManager : Node
{
    [Export]
    private double freezeSlow = 0.07f;

    [Export]
    private double freezeTime = 0.3f;

    public override void _Ready()
    {
        base._Ready();

        EventBus.Instance.ImpactEvent += OnImpactEvent;
    }

    private async void OnImpactEvent(float intensity)
    {
        await ChangeEngineTimeScale(intensity >= 1 ? 0.6 : freezeTime);
    }

    private async Task ChangeEngineTimeScale(double delay)
    {
        Engine.TimeScale = freezeSlow;
        await ToSignal(GetTree().CreateTimer(delay * freezeSlow), "timeout");
        Engine.TimeScale = 1;
    }
}
