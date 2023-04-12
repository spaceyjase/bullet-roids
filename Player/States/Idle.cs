using Godot;

namespace Player.States;

public partial class Idle : PlayerState
{
    public override void _Ready()
    {
        base._Ready();

        OnProcess += DoProcess;
        OnEnter += () =>
        {
            player.IsMoving = false;
        };
    }

    private void DoProcess(double delta)
    {
        if (!player.IsActive)
            return;

        if (MoveCheck())
        {
            StateMachine?.ChangeState(nameof(Move));
        }
        if (Input.IsActionPressed("shoot"))
        {
            StateMachine?.ChangeState(nameof(Shoot));
        }
    }
}
