using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField, ReadOnly] private Vector3 upAxisAngleRotation = Vector3.zero;
    [SerializeField, ReadOnly] private Memory<Vector3> upAxis;
    [SerializeField, ReadOnly] private Vector3 smoothedUpAxis;
    [SerializeField, ReadOnly] private Vector3 oldUpAxis;
    [SerializeField, ReadOnly] private float timeStep;
    [SerializeField] private Transform playerHead;
    // y max
    [SerializeField] private float yRotationLock = 89.99f;
    // for when the cameras up axis changes like for gravity or wall running
    [SerializeField] private float smoothingDelta;

    public static int fov = 90;
    public static Vector2 mouseSensitivity = new Vector2(12, 10);
    private Vector2 delta = Vector3.zero;
    private CollisionManagement playerCM;
    private Transform camTransform;
    private Transform player;
    private float mouseY;
    private int frameCount = 0;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Camera.main.fieldOfView = fov;
        camTransform = Camera.main.transform;
        player = Player.Instance.transform;

        playerCM = Player.Instance.modules.collisionManagement;
    }

    private void Update()
    {
        frameCount++;
        delta = UserInput.Instance.Look() * mouseSensitivity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        RotatePlayer();
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        camTransform.position = playerHead.position;
    }

    private void RotatePlayer()
    {
        Vector3 validUpAxis = playerCM.ValidUpAxis;
        CalculateSmoothedUpAxis(validUpAxis);

        // convert input to rotation

        if (frameCount < 4)
        {
            delta = Vector2.zero;
        }
        mouseY += delta.y;
        mouseY = Mathf.Clamp(mouseY, -yRotationLock, yRotationLock);
        Quaternion mouseRotationHorizontal = Quaternion.AngleAxis(delta.x, Vector3.up);
        Quaternion mouseRotationVertical = Quaternion.AngleAxis(mouseY, -Vector3.right);

        camTransform.rotation = Quaternion.LookRotation(player.forward, smoothedUpAxis) * mouseRotationVertical;
        player.GetComponent<Rigidbody>().MoveRotation(Quaternion.LookRotation(player.forward, validUpAxis) * mouseRotationHorizontal);
    }

    private void CalculateSmoothedUpAxis(Vector3 upAxisCamera)
    {
        upAxis.current = Quaternion.Euler(upAxisAngleRotation) * upAxisCamera;

        if (upAxis.current != upAxis.old)
        {
            timeStep = 0;
            oldUpAxis = upAxis.old;
        }
        upAxis.UpdateOld();
        timeStep = Mathf.Min(timeStep + smoothingDelta, 1);
        smoothedUpAxis = Vector3.Lerp(oldUpAxis, upAxis.current, timeStep);
        if (timeStep >= 1)
        {
            oldUpAxis = upAxis.current;
            smoothedUpAxis = upAxis.current;
        }
    }

    public void TiltUpAxis(Vector3 angles)
    {
        upAxisAngleRotation = angles;
    }

    public void ResetUpAxis()
    {
        upAxisAngleRotation = Vector3.zero;
    }
}