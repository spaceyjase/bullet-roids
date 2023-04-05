using Godot;

namespace Player.States;

public partial class Move : PlayerState
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

    var movement = new Vector2
    {
      X = Input.GetActionStrength("right") - Input.GetActionStrength("left"),
      Y = Input.GetActionStrength("down") - Input.GetActionStrength("up")
    };

    var rotation = Input.GetActionStrength("rotate_right") - Input.GetActionStrength("rotate_left");

    player.Move(movement, rotation, delta);

    CheckForCollisions();

    if ((movement == Vector2.Zero || (Mathf.IsZeroApprox(movement.X) && Mathf.IsZeroApprox(movement.Y))) &&
        Mathf.IsZeroApprox(rotation))
    {
      StateMachine?.ChangeState("Idle");
    }
  }
}