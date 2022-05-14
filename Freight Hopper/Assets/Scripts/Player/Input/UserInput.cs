using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UserInput : MonoBehaviour
{
    public InputMaster UserInputMaster => master;
    private InputMaster master;

    private static UserInput input;

    public static UserInput Instance => input;

    public delegate void PressEventHandler();

    public event PressEventHandler JumpInput;

    public event PressEventHandler JumpInputCanceled;

    public event PressEventHandler GroundPoundInput;

    public event PressEventHandler GroundPoundCanceled;

    //public Toggle jumpPressed = new Toggle();
    //public Toggle jumpReleased = new Toggle();
    //public Toggle groundPoundPressed = new Toggle();
    //public Toggle groundPoundReleased = new Toggle();

    public bool GroundPoundHeld => groundPoundHeld;
    public bool JumpHeld => jumpHeld;

    [ReadOnly, SerializeField] private bool groundPoundHeld;
    [ReadOnly, SerializeField] private bool jumpHeld;

    private void OnEnable()
    {
        master.Enable();
        master.Player.GroundPound.performed += GroundPoundPressed;
        master.Player.GroundPound.canceled += GroundPoundReleased;
        master.Player.Jump.performed += JumpPressed;
        master.Player.Jump.canceled += JumpReleased;

        if (SceneManager.GetActiveScene().name.Equals("DefaultScene"))
        {
            LevelController.LevelLoadedIn += RespawnLinked;
        }
        else
        {
            Player.PlayerLoadedIn += RespawnLinked;
        }
        master.Disable();
    }
    private int frameCount = 0;
    bool masterEnabled = false;
    private void Update()
    {
        if (frameCount >= 4)
        {
            master.Enable();
            masterEnabled = true;
            frameCount = 0;
        } else if (!masterEnabled){
            frameCount++;
        }
        
    }

    private void RespawnLinked()
    {
        master.Player.Restart.performed += LevelController.Instance.Respawn;
    }

    private void OnDisable()
    {
        master.Disable();
        LevelController.LevelLoadedIn -= RespawnLinked;
        master.Player.Restart.performed -= LevelController.Instance.Respawn;
        Player.PlayerLoadedIn -= RespawnLinked;
    }

    private void Awake()
    {
        // Singleton instance pattern
        if (input == null)
        {
            input = this;
            master = new InputMaster();
        }
        else
        {
            Destroy(this);
        }
    }
    private void GroundPoundPressed(InputAction.CallbackContext context)
    {
        groundPoundHeld = !groundPoundHeld;
        if (groundPoundHeld)
        {
            GroundPoundInput?.Invoke();
        }
    }

    private void JumpPressed(InputAction.CallbackContext _)
    {
        jumpHeld = !jumpHeld;
        if (jumpHeld)
        {
            JumpInput?.Invoke();
        }
    }

    private void JumpReleased(InputAction.CallbackContext _)
    {
        JumpInputCanceled?.Invoke();
    }

    private void GroundPoundReleased(InputAction.CallbackContext _)
    {
        GroundPoundCanceled?.Invoke();
    }

    // Returns the direction the player wants to move
    public Vector3 Move()
    {
        Vector2 rawInput = master.Player.Movement.ReadValue<Vector2>();
        return new Vector3(rawInput.x, 0, rawInput.y);
    }

    public Vector2 Look()
    {
        return master.Player.Look.ReadValue<Vector2>();
    }

    // Returns true if the player press the restart key/button
    public bool Restart()
    {
        return master.Player.Restart.triggered;
    }

    /*
     * public Toggle jumpPressed = new Toggle();
    public Toggle jumpReleased = new Toggle();
    public Toggle groundPoundPressed = new Toggle();
    public Toggle groundPoundReleased = new Toggle();
    private bool lastStateGroundPound = false;

    public void ResetInputs()
    {
        jumpPressed.Reset();
        jumpReleased.Reset();
        groundPoundPressed.Reset();
        groundPoundReleased.Reset();
    }

    public void OnDisable()
    {
        this.UnsubToListeners();
    }

    private void SubToListeners()
    {
        UserInput.Instance.JumpInput += jumpPressed.Trigger;
        UserInput.Instance.JumpInputCanceled += jumpReleased.Trigger;
        UserInput.Instance.GroundPoundInput += groundPoundPressed.Trigger;
        UserInput.Instance.GroundPoundCanceled += groundPoundReleased.Trigger;
    }

    private void UnsubToListeners()
    {
        UserInput.Instance.JumpInput -= jumpPressed.Trigger;
        UserInput.Instance.JumpInputCanceled -= jumpReleased.Trigger;
        UserInput.Instance.GroundPoundInput -= groundPoundPressed.Trigger;
        UserInput.Instance.GroundPoundCanceled -= groundPoundReleased.Trigger;
    }
     */
}