using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

// Help developing this and related code using this YouTube video: https://www.youtube.com/watch?v=V75hgcsCGOM&t=944s
// Addition help from this link on delagate funcs: https://www.tutorialsteacher.com/csharp/csharp-func-delegate
public class PlayerStatesTransitions
{
    private PlayerMachineCenter playerMachine;

    public Toggle jumpPressed = new Toggle();
    public Toggle jumpReleased = new Toggle();
    public Toggle grapplePressed = new Toggle();
    public Toggle grappleReleased = new Toggle();
    public Toggle groundPoundPressed = new Toggle();
    public Toggle groundPoundReleased = new Toggle();
    public Toggle upwardDashPressed = new Toggle();
    public Toggle fullStopPressed = new Toggle();
    public Toggle burstPressed = new Toggle();
    public Toggle releasedUpwardDash = new Toggle();

    public void ResetInputs()
    {
        jumpPressed.Reset();
        jumpReleased.Reset();
        grapplePressed.Reset();
        grappleReleased.Reset();
        groundPoundPressed.Reset();
        groundPoundReleased.Reset();
        upwardDashPressed.Reset();
        fullStopPressed.Reset();
        burstPressed.Reset();
        releasedUpwardDash.Reset();
    }

    public void OnDisable()
    {
        this.UnsubToListeners();
    }

    private void SubToListeners()
    {
        UserInput.Instance.JumpInput += jumpPressed.Trigger;
        UserInput.Instance.JumpInputCanceled += jumpReleased.Trigger;
        UserInput.Instance.GrappleInput += grapplePressed.Trigger;
        UserInput.Instance.GrappleInputCanceled += grappleReleased.Trigger;
        UserInput.Instance.GroundPoundInput += groundPoundPressed.Trigger;
        UserInput.Instance.GroundPoundCanceled += groundPoundReleased.Trigger;
        UserInput.Instance.UpwardDashInput += upwardDashPressed.Trigger;
        UserInput.Instance.FullStopInput += fullStopPressed.Trigger;
        UserInput.Instance.BurstInput += burstPressed.Trigger;
        UserInput.Instance.UpwardDashInputCanceled += releasedUpwardDash.Trigger;
    }

    private void UnsubToListeners()
    {
        UserInput.Instance.JumpInput -= jumpPressed.Trigger;
        UserInput.Instance.JumpInputCanceled -= jumpReleased.Trigger;
        UserInput.Instance.GrappleInput -= grapplePressed.Trigger;
        UserInput.Instance.GrappleInputCanceled -= grappleReleased.Trigger;
        UserInput.Instance.GroundPoundInput -= groundPoundPressed.Trigger;
        UserInput.Instance.GroundPoundCanceled -= groundPoundReleased.Trigger;
        UserInput.Instance.UpwardDashInput -= upwardDashPressed.Trigger;
        UserInput.Instance.FullStopInput -= fullStopPressed.Trigger;
        UserInput.Instance.BurstInput -= burstPressed.Trigger;
        UserInput.Instance.UpwardDashInputCanceled -= releasedUpwardDash.Trigger;
    }

    public PlayerStatesTransitions(FiniteStateMachineCenter machineCenter)
    {
        SubToListeners();

        playerMachine = (PlayerMachineCenter)machineCenter;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // The conditional functions to check if the PFSM should transition to a different state ///////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public BasicState CheckToFallState()
    {
        // Jump or Double Jump
        if ((jumpReleased.value || !playerMachine.abilities.jumpBehavior.jumpHoldingTimer.TimerActive()) &&
            (playerMachine.currentState == playerMachine.jumpState || playerMachine.currentState == playerMachine.doubleJumpState))
        {
            if (playerMachine.currentState == playerMachine.doubleJumpState)
            {
                playerMachine.abilities.jumpBehavior.jumpHoldingTimer.DeactivateTimer();
            }
            return playerMachine.fallState;
        }

        // Idle
        if ((!playerMachine.playerCM.IsGrounded.current) &&
            (playerMachine.currentState == playerMachine.runState || playerMachine.currentState == playerMachine.idleState))
        {
            return playerMachine.fallState;
        }

        // Upward Dash
        if ((releasedUpwardDash.value || !playerMachine.abilities.upwardDashBehavior.duration.TimerActive()) && playerMachine.currentState == playerMachine.upwardDashState)
        {
            return playerMachine.fallState;
        }

        // Ground Pound
        if (groundPoundReleased.value && playerMachine.currentState == playerMachine.groundPoundState)
        {
            return playerMachine.fallState;
        }

        // Grapple
        if ((grappleReleased.value || (playerMachine.abilities.grapplePoleBehavior.GrapplePoleBroken() &&
            playerMachine.abilities.grapplePoleBehavior.IsAnchored())) &&
            (playerMachine.currentState == playerMachine.grapplePoleAnchoredState))
        {
            return playerMachine.fallState;
        }

        // Full stop
        if ((playerMachine.abilities.fullstopBehavior.FullStopFinished()) && (playerMachine.currentState == playerMachine.fullStopState))
        {
            return playerMachine.fallState;
        }

        // Wall run
        if (playerMachine.currentState == playerMachine.wallRunState)
        {
            bool[] status = playerMachine.abilities.wallRunBehavior.CheckWalls();
            // Fall from wall climb
            if (!status[0] && !status[1] && !status[3] &&
                playerMachine.wallRunState.GetPlayerSubStateMachineCenter().currentState != playerMachine.wallRunState.GetSubStateArray()[2] &&
                playerMachine.wallRunState.GetPlayerSubStateMachineCenter().currentState != playerMachine.wallRunState.GetSubStateArray()[1])
            {
                playerMachine.abilities.wallRunBehavior.coyoteTimer.CountDownFixed();
                if (!playerMachine.abilities.wallRunBehavior.coyoteTimer.TimerActive())
                {
                    playerMachine.abilities.wallRunBehavior.coyoteTimer.ResetTimer();
                    return playerMachine.fallState;
                }
            }
            if (playerMachine.wallRunState.GetPlayerSubStateMachineCenter().currentState == playerMachine.wallRunState.GetSubStateArray()[2]
                && (jumpReleased.value || !playerMachine.abilities.wallRunBehavior.jumpHoldingTimer.TimerActive()))
            {
                return playerMachine.fallState;
            }
            if (playerMachine.wallRunState.GetPlayerSubStateMachineCenter().currentState == playerMachine.wallRunState.GetSubStateArray()[1] && !status[0])
            {
                return playerMachine.fallState;
            }
        }
        return null;
    }

    public BasicState CheckToJumpState()
    {
        // Other
        if (playerMachine.GetCurrentState() != playerMachine.fallState &&
            (jumpPressed.value || playerMachine.abilities.jumpBehavior.jumpBufferTimer.TimerActive()) &&
            playerMachine.abilities.jumpBehavior.UnlockedAndReady)
        {
            return playerMachine.jumpState;
        }

        // Fall
        if (playerMachine.GetCurrentState() == playerMachine.fallState &&
            jumpPressed.value && playerMachine.abilities.jumpBehavior.coyoteeTimer.TimerActive() &&
            playerMachine.GetPreviousState() != playerMachine.jumpState && playerMachine.abilities.jumpBehavior.UnlockedAndReady)
        {
            return playerMachine.jumpState;
        }

        // Ground Pound
        if (jumpPressed.value && playerMachine.abilities.jumpBehavior.UnlockedAndReady &&
            playerMachine.currentState == playerMachine.groundPoundState)
        {
            return playerMachine.jumpState;
        }

        // Grapple Pole
        if (jumpPressed.value && playerMachine.currentState == playerMachine.grapplePoleAnchoredState)
        {
            return playerMachine.jumpState;
        }

        if (jumpPressed.value && playerMachine.abilities.jumpBehavior.Unlocked && playerMachine.abilities.jumpBehavior.Consumed &&
            !playerMachine.abilities.doubleJumpBehavior.Unlocked)
        {
            playerMachine.abilities.jumpBehavior.PlayerSoundManager().Play("JumpFail");
        }

        return null;
    }

    public BasicState CheckToIdleState()
    {
        if ((playerMachine.playerCM.IsGrounded.current && playerMachine.GetCurrentState() == playerMachine.fallState) ||
            (UserInput.Instance.Move() == Vector3.zero && playerMachine.GetCurrentState() == playerMachine.runState))
        {
            return playerMachine.idleState;
        }
        if (playerMachine.playerCM.IsGrounded.current && playerMachine.currentState == playerMachine.wallRunState)
        {
            return playerMachine.idleState;
        }
        return null;
    }

    public BasicState CheckToRunState()
    {
        if (UserInput.Instance.Move() != Vector3.zero && playerMachine.abilities.movementBehavior.Unlocked)
        {
            return playerMachine.runState;
        }
        return null;
    }

    public BasicState CheckToDoubleJumpState()
    {
        if (jumpPressed.value && playerMachine.abilities.doubleJumpBehavior.UnlockedAndReady &&
            playerMachine.currentState == playerMachine.fallState)
        {
            return playerMachine.doubleJumpState;
        }
        if (jumpPressed.value && playerMachine.abilities.jumpBehavior.Consumed &&
            playerMachine.abilities.doubleJumpBehavior.UnlockedAndReady &&
            playerMachine.currentState == playerMachine.doubleJumpState)
        {
            return playerMachine.doubleJumpState;
        }
        if (jumpPressed.value && playerMachine.abilities.doubleJumpBehavior.Unlocked && playerMachine.abilities.doubleJumpBehavior.Consumed)
        {
            playerMachine.abilities.doubleJumpBehavior.PlayerSoundManager().Play("JumpFail");
        }
        return null;
    }

    public BasicState CheckToGroundPoundState()
    {
        if (groundPoundPressed.value &&
            (playerMachine.playerCM.ContactNormal.current != playerMachine.playerCM.ValidUpAxis ||
            playerMachine.playerCM.IsGrounded.current == false) && playerMachine.abilities.groundPoundBehavior.Unlocked)
        {
            return playerMachine.groundPoundState;
        }

        return null;
    }

    public BasicState CheckToGrappleGroundPoundState()
    {
        if (groundPoundPressed.value && playerMachine.currentState == playerMachine.grapplePoleAnchoredState ||
            grapplePressed.value && playerMachine.currentState == playerMachine.groundPoundState)
        {
            if ((playerMachine.playerCM.ContactNormal.current != playerMachine.playerCM.ValidUpAxis ||
                playerMachine.playerCM.IsGrounded.current == false) && playerMachine.abilities.groundPoundBehavior.Unlocked)
            {
                if (playerMachine.playerCM.IsGrounded.current)
                {
                    playerMachine.abilities.groundPoundBehavior.PreventConsumption();
                }

                return playerMachine.grapplePoleGroundPoundState;
            }
        }

        return null;
    }

    public BasicState CheckToUpwardDashState()
    {
        if (upwardDashPressed.value && playerMachine.abilities.upwardDashBehavior.UnlockedAndReady)
        {
            if (playerMachine.currentState == playerMachine.idleState || playerMachine.currentState == playerMachine.runState)
            {
                playerMachine.abilities.upwardDashBehavior.PreventConsumption();
            }
            return playerMachine.upwardDashState;
        }
        if (upwardDashPressed.value && playerMachine.abilities.upwardDashBehavior.Unlocked && playerMachine.abilities.upwardDashBehavior.Consumed)
        {
            playerMachine.abilities.upwardDashBehavior.PlayerSoundManager().Play("UpwardDashFail");
        }
        return null;
    }

    public BasicState CheckToFullStopState()
    {
        if (fullStopPressed.value && playerMachine.abilities.fullstopBehavior.UnlockedAndReady)
        {
            return playerMachine.fullStopState;
        }
        if (fullStopPressed.value && playerMachine.abilities.fullstopBehavior.Unlocked && playerMachine.abilities.fullstopBehavior.Consumed)
        {
            playerMachine.abilities.fullstopBehavior.PlayerSoundManager().Play("FullstopFail");
        }
        return null;
    }

    public BasicState CheckToGrappleFullStopState()
    {
        if (fullStopPressed.value && playerMachine.abilities.fullstopBehavior.UnlockedAndReady)
        {
            return playerMachine.grapplePoleFullStopState;
        }
        if (fullStopPressed.value && playerMachine.abilities.fullstopBehavior.Unlocked && playerMachine.abilities.fullstopBehavior.Consumed)
        {
            playerMachine.abilities.fullstopBehavior.PlayerSoundManager().Play("FullstopFail");
        }
        return null;
    }

    public BasicState CheckToBurstState()
    {
        if (burstPressed.value && playerMachine.abilities.burstBehavior.UnlockedAndReady)
        {
            if (playerMachine.playerCM.IsGrounded.current)
            {
                playerMachine.abilities.burstBehavior.PreventConsumption();
            }
            return playerMachine.burstState;
        }
        if (burstPressed.value && playerMachine.abilities.burstBehavior.Unlocked && playerMachine.abilities.burstBehavior.Consumed)
        {
            playerMachine.abilities.burstBehavior.PlayerSoundManager().Play("BurstFail");
        }
        return null;
    }

    public BasicState CheckToGrappleBurstState()
    {
        if (burstPressed.value && playerMachine.abilities.burstBehavior.UnlockedAndReady)
        {
            if (playerMachine.playerCM.IsGrounded.current)
            {
                playerMachine.abilities.burstBehavior.PreventConsumption();
            }
            return playerMachine.grapplePoleBurstState;
        }
        if (burstPressed.value && playerMachine.abilities.burstBehavior.Unlocked && playerMachine.abilities.burstBehavior.Consumed)
        {
            playerMachine.abilities.burstBehavior.PlayerSoundManager().Play("BurstFail");
        }
        return null;
    }

    public BasicState CheckToGrapplePoleAnchoredState()
    {
        if (playerMachine.abilities.grapplePoleBehavior.IsAnchored())
        {
            if (playerMachine.currentState == playerMachine.idleState || playerMachine.currentState == playerMachine.runState)
            {
                playerMachine.abilities.grapplePoleBehavior.PreventConsumption();
            }

            if (playerMachine.currentState == playerMachine.grapplePoleFullStopState)
            {
                if (!playerMachine.abilities.fullstopBehavior.FullStopFinished())
                {
                    return null;
                }
            }

            if (playerMachine.currentState == playerMachine.grapplePoleGroundPoundState)
            {
                if (!groundPoundReleased.value)
                {
                    return null;
                }
            }
            return playerMachine.grapplePoleAnchoredState;
        }
        return null;
    }

    public BasicState CheckToWallRunState()
    {
        if (playerMachine.abilities.wallRunBehavior.Unlocked)
        {
            bool[] walls = playerMachine.abilities.wallRunBehavior.CheckWalls();
            if (walls[0] || walls[1] || walls[3])
            {
                return playerMachine.wallRunState;
            }
        }

        return null;
    }

    public BasicState CheckToWallRunWallClimbingState()
    {
        bool[] status = playerMachine.abilities.wallRunBehavior.CheckWalls();
        // Wall Climb
        if (status[0] && !status[1] && !status[3])
        {
            return playerMachine.GetCurrentState().GetSubStateArray()[1];
        }
        return null;
    }

    public BasicState CheckToWallRunWallJumpState()
    {
        if (jumpPressed.value)
        {
            return playerMachine.GetCurrentState().GetSubStateArray()[2];
        }
        return null;
    }
}