using UnityEngine;

[System.Serializable]
public class VelocityController
{
    [SerializeField] private Current<float> speedLimit;
    [SerializeField] private float speedLimitIncreaseValue;
    [SerializeField] private float deltaAngle;
    [SerializeField] private float acceleration;
    [SerializeField] private float sideInputPenaltyMultiplier;

    private float oppositeAngle;
    private Rigidbody rb;
    private Friction friction;
    private Speedometer speedometer;

    public void Initialize(Speedometer speedometer, float oppositeAngle)
    {
        this.rb = Player.Instance.modules.rigidbody;
        this.friction = Player.Instance.modules.friction;
        this.speedometer = speedometer;
        this.oppositeAngle = oppositeAngle;
        speedLimit = new Current<float>(speedLimit.value);
    }

    public void RotateVelocity(Vector3 input)
    {
        float deltaAngleMultipier = 1;
        rb.AddForce(-speedometer.HorzVelocity, ForceMode.VelocityChange);
        if (Mathf.Abs(UserInput.Instance.Move().x) > 0.2f)
        {
            deltaAngleMultipier = sideInputPenaltyMultiplier;
        }
        Vector3 rotatedVector = Vector3.RotateTowards(speedometer.HorzVelocity.normalized, input, deltaAngle * deltaAngleMultipier, 0);
        rb.AddForce(rotatedVector * speedometer.HorzSpeed, ForceMode.VelocityChange);
    }

    public void Move(Vector3 input)
    {
        float nextSpeed = ((acceleration * Time.fixedDeltaTime * input) +
            speedometer.HorzVelocity).magnitude;

        if (nextSpeed < speedLimit.value || nextSpeed < speedometer.HorzSpeed)
        {
            rb.AddForce(input * acceleration, ForceMode.Acceleration);
            if (nextSpeed < speedLimit.Stored)
            {
                speedLimit.Reset();
            }
        }
        else
        {
            RotateVelocity(input);
            speedLimit.value += Time.fixedDeltaTime * speedLimitIncreaseValue;
        }
        if (OppositeInput(input))
        {
            friction.ResetFrictionReduction();
        }
    }

    public bool OppositeInput(Vector3 input)
    {
        if (input.IsZero())
        {
            return false;
        }
        Vector3 normalizedMomentumDirection = speedometer.HorzVelocity.normalized;
        return Vector3.Angle(normalizedMomentumDirection, input) > oppositeAngle;
    }
}