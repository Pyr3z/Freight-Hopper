using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpHoldState : BasicState
{
    public void SubToListeners(FiniteStateMachineCenter machineCenter)
    {
    }

    public void UnsubToListeners(FiniteStateMachineCenter machineCenter)
    {
    }

    public BasicState TransitionState(FiniteStateMachineCenter machineCenter)
    {
        return this;
    }

    public void PerformBehavior(FiniteStateMachineCenter machineCenter)
    {
        PlayerMachineCenter playerMachine = (PlayerMachineCenter)machineCenter;

        playerMachine.jumpHoldingTimer.CountDownFixed();
        playerMachine.playerAbilities.movementBehavior.Action();
        playerMachine.playerAbilities.jumpBehavior.Action();
    }

    public bool HasSubStateMachine()
    {
        return false;
    }

    public BasicState GetCurrentSubState()
    {
        return null;
    }

    public BasicState[] GetSubStateArray()
    {
        return null;
    }
}