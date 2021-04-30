using UnityEngine;

public class MovementBehavior : AbilityBehavior
{
    [SerializeField] private float groundAcceleration = 20;
    [SerializeField] private float airAcceleration = 10;

    private Transform cameraTransform;
    public float Speed => groundAcceleration;

    public override void Initialize(Rigidbody rb, CollisionManagement cm, SoundManager sm)
    {
        base.Initialize(rb, cm, sm);
        cameraTransform = Camera.main.transform;
    }

    private Vector3 RelativeMove(Vector3 forward, Vector3 right)
    {
        forward = forward.ProjectOnContactPlane(playerCM.ValidUpAxis);
        right = right.ProjectOnContactPlane(playerCM.ValidUpAxis);

        // Moves relative to the camera
        Vector3 input = UserInput.Input.Move();
        Vector3 move = forward * input.z + right * input.x;
        return move;
    }

    public void Move(Rigidbody rigidbody, CollisionManagement collision, Vector3 direction, float acceleration)
    {
        Vector3 relativeDirection = direction.ProjectOnContactPlane(collision.ContactNormal.current).normalized;

        rigidbody.AddForce(relativeDirection * acceleration, ForceMode.Acceleration);
    }

    public override void EntryAction()
    {
    }

    public override void Action()
    {
        Vector3 relativeMove = RelativeMove(cameraTransform.forward, cameraTransform.right);
        playerSM.PlayRandom("Move 1", 7);
        Move(playerRb, playerCM, relativeMove, playerCM.IsGrounded.current ? groundAcceleration : airAcceleration);
    }

    public override void ExitAction()
    {
    }
}