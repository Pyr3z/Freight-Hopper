using System.Collections;
using UnityEngine;

public class MovementBehavior : AbilityBehavior
{
    [SerializeField] private float oppositeInputAngle = 170;

    [SerializeField] private Speedometer speedometer;
    [SerializeField] private VelocityController groundController;
    [SerializeField] private VelocityController airController;
    [SerializeField, ReadOnly] bool waiting = false;
    [SerializeField] float speedSoundMultiplier = 20f;
    [SerializeField] Vector2 walkingSoundTimeRange = new Vector2(0.1f, 0.4f);
    public float HorizontalSpeed => speedometer.AbsoluteHorzSpeed;
    public float Speed => rb.velocity.magnitude;

    private Transform cameraTransform;
    private CollisionManagement collisionManager;
    private Friction friction;

    public override void Initialize()
    {
        base.Initialize();
        this.collisionManager = Player.Instance.modules.collisionManagement;
        this.friction = Player.Instance.modules.friction;
        cameraTransform = Camera.main.transform;
        speedometer.Initialize();
        groundController.Initialize(speedometer, oppositeInputAngle);
        airController.Initialize(speedometer, oppositeInputAngle);
    }

    private Vector3 ConvertInputToCameraSpace(Vector3 forward, Vector3 right)
    {
        forward = forward.ProjectOnContactPlane(collisionManager.ValidUpAxis);
        right = right.ProjectOnContactPlane(collisionManager.ValidUpAxis);

        // Moves relative to the camera
        Vector3 input = UserInput.Instance.Move();
        Vector3 move = (forward * input.z) + (right * input.x);
        return move;
    }

    public void Move(Vector3 direction)
    {
        // If the player is in the air, the default direction is upwards. So this calculation does
        // nothing in the air.
        if (direction.IsZero())
        {
            return;
        }
        Vector3 surfaceMoveDirection = direction.ProjectOnContactPlane(collisionManager.ContactNormal.current).normalized;
        
        if (!collisionManager.IsGrounded.current)
        {
            airController.Move(surfaceMoveDirection);
        }
        else
        {
            friction.ReduceFriction(1);
            groundController.Move(surfaceMoveDirection);
        }
    }

    public void MoveAction()
    {
        Vector3 relativeInput = ConvertInputToCameraSpace(cameraTransform.forward, cameraTransform.right);

        Move(relativeInput);
    }

    public void UpdateStatus()
    {
        speedometer.UpdateSpeedometer();
    }

    public void Action()
    {
        MoveAction();
        
        // For when to play the audio for moving ====
        if (waiting)
        {
            return;
        }
        if (!collisionManager.IsGrounded.current)
        {
            return;
        }
        if (UserInput.Instance.Move().IsZero())
        {
            return;
        }
        if (UserInput.Instance.GroundPoundHeld)
        {
            return;
        }

        StartCoroutine(PlayMovement(speedometer.HorzSpeed));
    }
    
    IEnumerator PlayMovement(float speed)
    {
        soundManager.StartCoroutine(soundManager.PlayRandom("Move", 7));
        soundManager.StartCoroutine(soundManager.PlayRandom("Stone", 5));
        waiting = true;
        speed = speedSoundMultiplier/speed;
        speed = Mathf.Clamp(speed, walkingSoundTimeRange.x, walkingSoundTimeRange.y);
        float elapsed = 0;
        while (elapsed < speed)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        waiting = false;
    }
    
}