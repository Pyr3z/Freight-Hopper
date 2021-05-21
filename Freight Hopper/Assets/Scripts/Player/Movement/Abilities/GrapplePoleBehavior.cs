using UnityEngine;

public class GrapplePoleBehavior : AbilityBehavior
{
    [SerializeField, ReadOnly] private float length;
    [SerializeField] private LineRenderer pole;
    [SerializeField, Range(0.9f, 3)] private float breakMaxLengthPercent = 1.2f;
    [SerializeField] private float maxLength = 10;
    [SerializeField] private float grappleExtensionSpeed = 10;
    [SerializeField] private float grappleMoveSpeed = 10;

    [SerializeField] private LayerMask affectedLayers;

    private Ray playerAnchor;
    private Ray anchor;
    private Rigidbody anchoredRb;
    private Vector3 anchoredRbLocalPosition;
    private bool anchored;

    [SerializeField]
    private PID distanceController;

    private Transform cameraTransform;

    public override void Initialize(Rigidbody rb, CollisionManagement cm, SoundManager sm)
    {
        base.Initialize(rb, cm, sm);
        cameraTransform = Camera.main.transform;
    }

    private void Awake()
    {
        pole.enabled = false;
    }

    /// <summary>
    /// Grapple anchored code, direction is the direction the player wants to move
    /// </summary>
    public void Grapple(Vector3 direction)
    {
        if (anchoredRb != null)
        {
            anchor = new Ray(anchoredRb.transform.TransformPoint(anchoredRbLocalPosition), -playerAnchor.direction);
        }
        playerAnchor = new Ray(playerRb.position + pole.GetPosition(0), playerAnchor.direction);
        pole.SetPosition(1, playerRb.transform.InverseTransformPoint(anchor.origin));

        //Transform playerTransform = rb.transform;
        Vector3 up = (playerAnchor.origin - anchor.origin).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(direction.x * cameraTransform.right, up);
        Vector3 right = Vector3.ProjectOnPlane(direction.z * cameraTransform.forward, up);
        //Debug.Log("dot product of normal and camera forward: " + Vector3.Dot(normal, cameraTransform.right));

        //Debug.Log("dot product of anchor direction and camera forward: " + Vector3.Dot(anchor.direction, cameraTransform.forward));
        Debug.DrawLine(playerAnchor.origin, playerAnchor.origin + up, Color.green, Time.fixedDeltaTime);
        Debug.DrawLine(playerAnchor.origin, playerAnchor.origin + forward, Color.blue, Time.fixedDeltaTime);
        Debug.DrawLine(playerAnchor.origin, playerAnchor.origin + right, Color.red, Time.fixedDeltaTime);

        Vector3 move = right + forward;
        //move.Normalize();

        playerRb.AddForce(move * grappleMoveSpeed, ForceMode.Acceleration);

        float expectedLength = length;
        float actualLength = (playerAnchor.origin - anchor.origin).magnitude;
        float error = (expectedLength - actualLength);
        Vector3 tensionVelocity = up * distanceController.GetOutput(error, Time.fixedDeltaTime);
        playerRb.AddForce(tensionVelocity, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Grapple transition is the firing code. Tries to attach to a surface
    /// </summary>
    public void GrappleTransition()
    {
        playerAnchor = new Ray(playerRb.position + pole.GetPosition(0), cameraTransform.transform.forward);

        length += Time.fixedDeltaTime * grappleExtensionSpeed;
        length = Mathf.Min(length, maxLength);
        pole.SetPosition(1, playerRb.transform.InverseTransformPoint(playerAnchor.GetPoint(length)));

        Debug.DrawLine(playerAnchor.origin, playerAnchor.GetPoint(length), Color.yellow, Time.fixedDeltaTime);
        if (Physics.Raycast(playerAnchor, out RaycastHit hit, length, affectedLayers))
        {
            anchored = true;
            anchor = new Ray(hit.point, -playerAnchor.direction);
            length = (hit.point - playerAnchor.origin).magnitude;
            if (hit.rigidbody != null)
            {
                anchoredRb = hit.rigidbody;
                anchoredRbLocalPosition = anchoredRb.transform.InverseTransformPoint(hit.point);
            }
            distanceController.Reset();
            playerSM.Play("GrappleAnchor", hit.point);
        }
    }

    /// <summary>
    /// Returns true if the grapple pole is anchored
    /// </summary>
    public bool IsAnchored()
    {
        return anchored;
    }

    /// <summary>
    /// Returns true if the pole should be snapped due to extending too far past max distance
    /// </summary>
    public bool GrapplePoleBroken()
    {
        float actualLength = (playerAnchor.origin - anchor.origin).magnitude;
        return actualLength > maxLength * breakMaxLengthPercent;
    }

    public override void EntryAction()
    {
        playerSM.Play("GrappleFire");
        playerAnchor = new Ray(playerRb.position + pole.GetPosition(0), cameraTransform.transform.forward);
        pole.enabled = true;
        pole.SetPosition(1, pole.GetPosition(0));
    }

    public override void Action()
    {
        GrappleTransition();
    }

    public override void ExitAction()
    {
        base.ExitAction();
        ResetPole();
    }

    public void ResetPole()
    {
        length = 0;
        anchoredRb = null;
        anchoredRbLocalPosition = Vector3.zero;
        anchored = false;
        pole.enabled = false;
    }
}