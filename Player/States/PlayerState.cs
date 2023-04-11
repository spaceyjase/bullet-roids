using Godot;
using State = StateMachine.State;

namespace Player.States;

public partial class PlayerState : State
{
    protected Player player;

    public override void _Ready()
    {
        base._Ready();

        player = Owner as Player;
    }

    protected void CheckForCollisions()
    {
        if (!player.Colliding)
            return;
        //if (!(collision.Collider is KinematicBody2D other) || !other.IsInGroup("Enemies")) continue;

        StateMachine?.ChangeState("Dead");
    }

    protected static bool MoveCheck()
    {
        var left = Input.IsActionPressed("left");
        var right = Input.IsActionPressed("right");
        var up = Input.IsActionPressed("up");
        var down = Input.IsActionPressed("down");
        var rotate_left = Input.IsActionPressed("rotate_left");
        var rotate_right = Input.IsActionPressed("rotate_right");

        return left || right || up || down || rotate_left || rotate_right;
    }
}
