namespace Player.States;

public partial class Shoot : PlayerState
{
  public override void _Ready()
  {
    base._Ready();

    OnProcess += DoProcess;
  }

  private void DoProcess(double obj)
  {
    if (!player.IsActive) return;

    if (player.CanShoot)
    {
      player.Fire();
    }

    StateMachine?.ChangeState(MoveCheck() ? nameof(Move) : nameof(Idle));
  }
}