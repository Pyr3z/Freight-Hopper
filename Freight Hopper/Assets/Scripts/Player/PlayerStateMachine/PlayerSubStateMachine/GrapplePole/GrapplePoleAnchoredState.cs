using System;
using System.Collections.Generic;

public class GrapplePoleAnchoredState : PlayerState
{
    public GrapplePoleAnchoredState(PlayerMachineCenter playerMachineCenter, List<Func<BasicState>> myTransitions) : base(playerMachineCenter, myTransitions)
    {
    }

    public override void EntryState()
    {
    }

    public override void ExitState()
    {
        if (
            playerMachineCenter.currentState == playerMachineCenter.grapplePoleFullStopState ||
            playerMachineCenter.currentState == playerMachineCenter.grapplePoleGroundPoundState)
        {
            return;
        }
        playerMachineCenter.abilities.grapplePoleBehavior.ExitAction();
    }

    public override BasicState TransitionState()
    {
        return CheckTransitions();
    }

    public override void PerformBehavior()
    {
        playerMachineCenter.abilities.grapplePoleBehavior.Grapple(UserInput.Instance.Move());
        if (UserInput.Instance.GrappleHeld)
        {
            playerMachineCenter.abilities.grapplePoleBehavior.Pull();
        }
        if (playerMachineCenter.collisionManagement.IsGrounded.current && !playerMachineCenter.collisionManagement.IsGrounded.old)
        {
            playerMachineCenter.abilities.Recharge();
            playerMachineCenter.abilities.grapplePoleBehavior.PreventConsumption();
        }
    }
}