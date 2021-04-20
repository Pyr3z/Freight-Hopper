using UnityEngine;

[RequireComponent(typeof(CollisionManagement), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public MovementBehavior movement;
    public JumpBehavior jumpBehavior;
    public GroundPoundBehavior groundPoundBehavior;
    public DoubleJump doubleJump;
    public FullStop fullstop;

    private Rigidbody rb;
    private CollisionManagement playerCollision;

    private void OnValidate()
    {
        movement = GetComponentInChildren<MovementBehavior>();
        jumpBehavior = GetComponentInChildren<JumpBehavior>();
        groundPoundBehavior = GetComponentInChildren<GroundPoundBehavior>();
        doubleJump = GetComponentInChildren<DoubleJump>();
        fullstop = GetComponentInChildren<FullStop>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollision = GetComponent<CollisionManagement>();

        movement.Initialize(rb, playerCollision, Camera.main.transform);
        doubleJump.Initialize(playerCollision, jumpBehavior);
        jumpBehavior.Initialize(rb, playerCollision);
        groundPoundBehavior.Initalize(rb, playerCollision);

        fullstop.Initialize(rb, playerCollision);
    }
}