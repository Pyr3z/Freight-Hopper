using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : BasicState
{
    private bool releasedJumpPressed = false;
    private PlayerSubStateMachineCenter pSSMC;
    private PlayerMachineCenter myPlayerMachineCenter;
    private BasicState[] miniStateArray;

    public JumpState(PlayerMachineCenter myPMC)
    {
        myPlayerMachineCenter = myPMC;
        miniStateArray = new BasicState[2];
        miniStateArray[0] = new JumpInitialState();
        miniStateArray[1] = new JumpHoldState();
        pSSMC = new PlayerSubStateMachineCenter(this, miniStateArray, myPlayerMachineCenter);
    }

    public void SubToListeners(FiniteStateMachineCenter machineCenter)
    {
        PlayerMachineCenter playerMachine = (PlayerMachineCenter)machineCenter;

        UserInput.Input.JumpInputCanceled += this.ReleasedJumpButtonPressed;
        pSSMC.GetCurrentSubState().SubToListeners(playerMachine);

        ///////////////MAYBE HERE RESET THE INITIAL SUBSTATE??
        pSSMC.SetPrevCurrState(miniStateArray[0]);

        // reset jump hold timer
        playerMachine.jumpHoldingTimer.ResetTimer();
    }

    public void UnsubToListeners(FiniteStateMachineCenter machineCenter)
    {
        PlayerMachineCenter playerMachine = (PlayerMachineCenter)machineCenter;

        UserInput.Input.JumpInputCanceled -= this.ReleasedJumpButtonPressed;
        pSSMC.GetCurrentSubState().UnsubToListeners(playerMachine);

        // deactivate jump hold timer
        playerMachine.jumpHoldingTimer.DeactivateTimer();

        ///////////////MAYBE HERE RESET THE INITIAL SUBSTATE??
        pSSMC.SetPrevCurrState(miniStateArray[0]);
    }

    public BasicState TransitionState(FiniteStateMachineCenter machineCenter)
    {
        PlayerMachineCenter playerMachine = (PlayerMachineCenter)machineCenter;

        // Fall
        if (releasedJumpPressed || !playerMachine.jumpHoldingTimer.TimerActive())
        {
            playerMachine.jumpHoldingTimer.DeactivateTimer();
            releasedJumpPressed = false;
            return playerMachine.fallState;
        }
        // Jump
        else
        {
            return this;
        }
    }

    public void PerformBehavior(FiniteStateMachineCenter machineCenter)
    {
        PlayerMachineCenter playerMachine = (PlayerMachineCenter)machineCenter;

        // each fixedupdate the jump button is pressed down, this timer should decrease by that time
        playerMachine.jumpHoldingTimer.CountDownFixed();

        // Perform the SubStateMachine Behavior
        pSSMC.PerformSubMachineBehavior();
    }

    private void ReleasedJumpButtonPressed()
    {
        releasedJumpPressed = true;
    }

    public bool HasSubStateMachine()
    {
        return true;
    }

    public BasicState GetCurrentSubState()
    {
        return pSSMC.GetCurrentSubState();
    }

    public BasicState[] GetSubStateArray()
    {
        return miniStateArray;
    }
}