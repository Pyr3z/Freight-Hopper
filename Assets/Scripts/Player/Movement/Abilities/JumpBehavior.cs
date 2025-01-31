using UnityEngine;

public class JumpBehavior : AbilityBehavior
{
    [SerializeField] JumpBehaviorInputs behaviorInputs;

    float t;
    float multiplier;
    float total;

    private RigidbodyLinker rigidbodyLinker;
    private CollisionManagement collisionManager;

    public Timer jumpHoldingTimer = new Timer(0.5f);
    public Timer coyoteeTimer = new Timer(0.5f);
    private bool active;
    public bool Active => active;
    public override void Initialize()
    {
        base.Initialize();
        rigidbodyLinker = Player.Instance.modules.rigidbodyLinker;
        collisionManager = Player.Instance.modules.collisionManagement;
    }

    /// <summary>
    /// Same as jump but inputs the users current jump height
    /// </summary>
    public void Jump()
    {
        Jump(behaviorInputs.intialVelocity);
    }
    private void FixedUpdate()
    {
        if (Active)
        {
            coyoteeTimer.DeactivateTimer();
        }
        if (collisionManager.TrueIsGrounded.old)
        {
            coyoteeTimer.ResetTimer();
            jumpHoldingTimer.DeactivateTimer();
        }
        else
        {
            coyoteeTimer.CountDown(Time.fixedDeltaTime);
        }
    }
    /// <summary>
    /// Physics behavior for the initial press of jump button
    /// </summary>
    public void Jump(float height)
    {
        Vector3 gravity = CustomGravity.GetGravity(out Vector3 upAxis);

        if (Vector3.Dot(rb.velocity, upAxis) < 0)
        {
            rb.velocity = rb.velocity.ProjectOnContactPlane(upAxis);
        }

        // Basic physics, except the force required to reach this height may not work if we consider
        // holding space That and considering that physics works in timesteps.
        float jumpForce = Mathf.Sqrt(2f * gravity.magnitude * height);

        // Upward bias for sloped jumping
        Vector3 jumpDirection = (collisionManager.ContactNormal.old + upAxis * behaviorInputs.upwardBiasWeight).normalized;

        // Considers velocity when jumping on slopes and the slope angle
        float alignedSpeed = Vector3.Dot(rb.velocity, jumpDirection);
        if (alignedSpeed > 0)
        {
            jumpForce = Mathf.Max(jumpForce - alignedSpeed, 0);
        }

        // Actual jump itself
        rb.AddForce(jumpForce * upAxis, ForceMode.VelocityChange);
        if (rigidbodyLinker.ConnectedRb.current != null)
        {
            rb.AddForce(Vector3.Project(rigidbodyLinker.ConnectionVelocity.current, upAxis), ForceMode.VelocityChange);
        }

        soundManager.Play("Jump");
    }

    public void EntryAction()
    {
        jumpHoldingTimer.ResetTimer();
        coyoteeTimer.DeactivateTimer();
        Jump();
        active = true;
    }
    public void ExitAction()
    {
        jumpHoldingTimer.DeactivateTimer();
        Player.Instance.modules.gravity.ResetGravityScale();
        active = false;
    }
    public void Action()
    {
        jumpHoldingTimer.CountDown(Time.fixedDeltaTime);
        t = Mathf.Clamp01(((jumpHoldingTimer.Current - jumpHoldingTimer.Duration) * -1) / jumpHoldingTimer.Duration);
        multiplier = behaviorInputs.jumpCurve.Evaluate(t);
        float gravityMultiplier = behaviorInputs.gravityRise.Evaluate(t);
        Player.Instance.modules.gravity.SetGravityScale(gravityMultiplier);
        rb.AddForce(CustomGravity.GetUpAxis() * multiplier, ForceMode.Acceleration);
    }
}