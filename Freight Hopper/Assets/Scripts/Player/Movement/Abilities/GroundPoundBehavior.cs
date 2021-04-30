using UnityEngine;

public class GroundPoundBehavior : AbilityBehavior
{
    [ReadOnly, SerializeField] private Var<float> increasingForce = new Var<float>(1, 1);

    [SerializeField] private float deltaIncreaseForce = 0.01f;
    [SerializeField] private float initialBurstForce = 20;
    [SerializeField] private float downwardsForce = 10;
    [SerializeField] private float slopeDownForce = 500;

    public override void EntryAction()
    {
        if (playerRb.velocity.y > 0)
        {
            playerRb.velocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z);
        }
        playerRb.AddForce(-playerCM.ValidUpAxis * initialBurstForce, ForceMode.VelocityChange);
    }

    public override void Action()
    {
        Vector3 upAxis = playerCM.ValidUpAxis;
        Vector3 direction = -upAxis;
        if (playerCM.IsGrounded.current)
        {
            Vector3 acrossSlope = Vector3.Cross(upAxis, playerCM.ContactNormal.current);
            Vector3 downSlope = Vector3.Cross(acrossSlope, playerCM.ContactNormal.current);
            direction = downSlope;
            direction *= slopeDownForce;
        }
        else
        {
            direction *= downwardsForce;
        }

        playerRb.AddForce(direction * increasingForce.current, ForceMode.Acceleration);
        increasingForce.current += deltaIncreaseForce * Time.fixedDeltaTime;
    }

    public override void ExitAction()
    {
        base.ExitAction();
        increasingForce.RevertCurrent();
    }
}