using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(JumpBehavior))]
public class DoubleJump : MonoBehaviour
{
    private bool doubleJumpPossible = true;
    [SerializeField] private Timer cooldown = new Timer(2);
    [SerializeField] private Timer delay = new Timer(0.1f);
    private bool doubleJump = false;
    private bool wasOnGround;
    private bool releasedJump;
    private JumpBehavior jumpBehavior;
    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        jumpBehavior = GetComponent<JumpBehavior>();
    }

    private void Update()
    {
        cooldown.CountDown();
        delay.CountDown();
        if (jumpBehavior.IsGrounded)
        {
            wasOnGround = true;
            doubleJumpPossible = false;
        }

        if (jumpBehavior.CanJump && wasOnGround)
        {
            doubleJumpPossible = true;
            wasOnGround = false;
            delay.ResetTimer();
        }

        if (UserInput.Input.Jump() && !cooldown.TimerActive() && !delay.TimerActive() && doubleJumpPossible && releasedJump)
        {
            doubleJump = true;
            doubleJumpPossible = false;
            cooldown.ResetTimer();
        }

        releasedJump = !UserInput.Input.Jump();
    }

    private void FixedUpdate()
    {
        if (doubleJump)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }

            /*rb.AddForce(jumpBehavior.JumpForce * Vector3.up, ForceMode.Impulse);*/
            jumpBehavior.Jump();
            doubleJump = false;
        }
    }
}