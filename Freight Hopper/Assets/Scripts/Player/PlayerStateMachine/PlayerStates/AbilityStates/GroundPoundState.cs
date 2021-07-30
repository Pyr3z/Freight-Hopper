using System.Collections.Generic;
using System;

public class GroundPoundState : PlayerState
{
    public GroundPoundState(PlayerMachineCenter playerMachineCenter, List<Func<BasicState>> myTransitions) : base(playerMachineCenter, myTransitions)
    {
    }

    public override void EntryState()
    {
        if (!playerMachineCenter.initialGroundPoundBurstCoolDown.TimerActive())
        {
            playerMachineCenter.abilities.groundPoundBehavior.EntryAction();
            playerMachineCenter.initialGroundPoundBurstCoolDown.ResetTimer();
        }
    }

    public override void ExitState()
    {
        playerMachineCenter.abilities.groundPoundBehavior.ExitAction();
    }

    public override BasicState TransitionState()
    {
        BasicState state = CheckTransitions();

        return state;
    }

    public override void PerformBehavior()
    {
        playerMachineCenter.physicsManager.friction.ReduceFriction(playerMachineCenter.abilities.groundPoundBehavior.FrictionReduction);
        playerMachineCenter.abilities.movementBehavior.MoveAction();
        playerMachineCenter.abilities.groundPoundBehavior.Action();
    }
}