using Cinemachine;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField, ReadOnly] private Vector3 upAxisAngleRotation = Vector3.zero;
    [SerializeField, ReadOnly] private Memory<Vector3> upAxis;
    [SerializeField, ReadOnly] private Vector3 smoothedUpAxis;
    [SerializeField, ReadOnly] private Vector3 oldUpAxis;
    [SerializeField, ReadOnly] private float timeStep;
    [SerializeField] private Transform upAxisTransform;
    private Transform player;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private GameObject ui;
    private GameObject otherUIs;

    // for when the cameras up axis changes like for gravity or wall running
    [SerializeField] private float smoothingDelta;
    [SerializeField] private float mouseSensitivityConversionValue = 10;
    private int curFrameCount;
    private bool calledEvent = false;
    private const int stopCameraFrameCount = 4;
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        calledEvent = false;
        float mouseSensitivity = Settings.GetMouseSensitivity;
        player = Player.Instance.transform.Find("Mesh");
        cinemachineVirtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = mouseSensitivity / mouseSensitivityConversionValue;
        cinemachineVirtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = mouseSensitivity / mouseSensitivityConversionValue;

    }

    private void Update()
    {
        CalculateSmoothedUpAxis(Vector3.up);
        if (stopCameraFrameCount > curFrameCount)
        {
            if(!calledEvent){
                calledEvent = true;
                Player.PlayerCanMove.Invoke();
            }
            cinemachineVirtualCamera.ForceCameraPosition(LevelController.Instance.playerSpawnTransform.position, LevelController.Instance.playerSpawnTransform.rotation);
        }
        if(InGameStates.Instance.StateIs(InGameStates.States.Paused) || InGameStates.Instance.StateIs(InGameStates.States.LevelComplete))
        {
            cinemachineVirtualCamera.gameObject.SetActive(false);
        } 
        else 
        {
            cinemachineVirtualCamera.gameObject.SetActive(true);
        }
        curFrameCount++;
        
    }

    private void FixedUpdate()
    {
        Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        player.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void OnEnable()
    {
        UserInput.Instance.UserInputMaster.Player.Freecam.performed += ToggleFreecam;
    }
    private void OnDisable()
    {
        UserInput.Instance.UserInputMaster.Player.Freecam.performed -= ToggleFreecam;
    }

    private void ToggleFreecam(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(InGameStates.Instance.StateIs(InGameStates.States.Playing)) {
            InGameStates.Instance.SwitchState(InGameStates.States.Freecam);
            Time.timeScale = 0;
            ui.SetActive(false);
            otherUIs = GameObject.Find("UniqueUICanvas");
            if(otherUIs != null){
                otherUIs.SetActive(false);
            }
        }
        else if(InGameStates.Instance.StateIs(InGameStates.States.Freecam)){
            InGameStates.Instance.SwitchState(InGameStates.States.Playing);
            Time.timeScale = 1;
            ui.SetActive(true);
            if (otherUIs != null)
            {
                otherUIs.SetActive(true);
            }
        }
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
        upAxisTransform.transform.up = smoothedUpAxis;
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