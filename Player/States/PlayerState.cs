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
    if (!player.Colliding) return;
    //if (!(collision.Collider is KinematicBody2D other) || !other.IsInGroup("Enemies")) continue;
    
    StateMachine?.ChangeState("Dead");
  }
}