using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BasicState
{
    private bool jumpPressed = false;

    public void SubToListeners(PlayerMachineCenter playerMachine)
    {
        UserInput.Input.JumpInput += this.JumpButtonPressed;
    }

    public void UnsubToListeners(PlayerMachineCenter playerMachine)
    {
        UserInput.Input.JumpInput -= this.JumpButtonPressed;
    }

    public BasicState TransitionState(PlayerMachineCenter playerMachine) {

        // if jumpbuffer timer is not expired, then jump


        // Jump
        if (jumpPressed)
        {
            jumpPressed = false;
            //Debug.Log("Should be in Jump state!");
            return playerMachine.jumpState;
        }

        // if the <INSERT ABILITY> button was pressed, then go to <INSERT ABILITY> state (return myPlayer.<INSERT ABILITY>State)

        // if the run buttons was pressed, then go to run state (return myPlayer.runState)
        if (UserInput.Input.Move() != Vector2.zero)
        {
            //Debug.Log("Should be in Run state!");
            return playerMachine.runState;
        }
        else {
            //Debug.Log("In Idle state");
            return this;
        }
    }

    public void PerformBehavior(PlayerMachineCenter playerMachine)
    {
        // reset coyotee timer, only decrements when in falling state
    }

    private void JumpButtonPressed()
    {
        jumpPressed = true;
    }

    public bool HasSubStateMachine()
    {
        return false;
    }

    public BasicState GetCurrentSubState() { return null; }
    public BasicState[] GetSubStateArray() { return null; }
}