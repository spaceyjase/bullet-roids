using Godot;

namespace Player.States;

public partial class Idle : PlayerState
{
  public override void _Ready()
  {
    base._Ready();

    OnEnter += () => { };
    OnProcess += DoProcess;
  }

  private void DoProcess(double delta)
  {
    if (!player.IsActive) return;

    var left = Input.IsActionPressed("left");
    var right = Input.IsActionPressed("right");
    var up = Input.IsActionPressed("up");
    var down = Input.IsActionPressed("down");
    var rotate_left = Input.IsActionPressed("rotate_left");
    var rotate_right = Input.IsActionPressed("rotate_right");

    if (left || right || up || down || rotate_left || rotate_right)
    {
      StateMachine?.ChangeState("Move");
    }
  }
}