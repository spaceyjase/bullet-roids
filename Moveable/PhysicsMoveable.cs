using Godot;

namespace Moveable;

public partial class PhysicsMoveable : RigidBody2D
{
    public Vector2 ScreenSize { get; set; }
    protected int radius;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        ScreenWrapCheck(state);
    }

    private void ScreenWrapCheck(PhysicsDirectBodyState2D state)
    {
        var xform = state.Transform;
        if (xform.Origin.X > ScreenSize.X + radius)
        {
            xform.Origin.X = 0 - radius;
        }

        if (xform.Origin.X < 0 - radius)
        {
            xform.Origin.X = ScreenSize.X + radius;
        }

        if (xform.Origin.Y > ScreenSize.Y + radius)
        {
            xform.Origin.Y = 0 - radius;
        }

        if (xform.Origin.Y < 0 - radius)
        {
            xform.Origin.Y = ScreenSize.Y + radius;
        }

        state.Transform = xform;
    }
}
