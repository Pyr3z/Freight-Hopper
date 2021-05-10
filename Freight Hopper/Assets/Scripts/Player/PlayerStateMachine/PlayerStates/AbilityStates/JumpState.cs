using System.Collections.Generic;
using System;

public class JumpState : PlayerState
{
    public JumpState(PlayerMachineCenter playerMachineCenter, List<Func<BasicState>> myTransitions) : base(playerMachineCenter, myTransitions)
    {
    }

    public override void EnterState()
    {
        // reset jump hold timer
        playerMachineCenter.abilities.jumpBehavior.jumpHoldingTimer.ResetTimer();
        playerMachineCenter.abilities.jumpBehavior.EntryAction();
    }

    public override void ExitState()
    {
        // deactivate jump hold timer
        playerMachineCenter.abilities.jumpBehavior.jumpHoldingTimer.DeactivateTimer();

        playerMachineCenter.abilities.jumpBehavior.ExitAction();
        playerMachineCenter.pFSMTH.ResetInputs();
    }

    public override BasicState TransitionState()
    {
        foreach (Func<BasicState> stateCheck in this.stateTransitions)
        {
            BasicState tempState = stateCheck();
            if (tempState != null)
            {
                return tempState;
            }
        }

        playerMachineCenter.pFSMTH.ResetInputs();

        return this;
    }

    public override void PerformBehavior()
    {
        // each fixedupdate the jump button is pressed down, this timer should decrease by that time
        playerMachineCenter.abilities.jumpBehavior.jumpHoldingTimer.CountDownFixed();

        playerMachineCenter.abilities.jumpBehavior.Action();
    }
}