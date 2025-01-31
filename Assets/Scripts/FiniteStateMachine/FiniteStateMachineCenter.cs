using UnityEngine;

public abstract class FiniteStateMachineCenter : MonoBehaviour
{
    // Machine Fields
    [SerializeField] private bool debugging;
    [SerializeField] private string currentStateName;
    [SerializeField] private string currentSubStateName;
    [SerializeField] private string previousStateName;

    public BasicState currentState;
    public BasicState previousState;
    public BasicState currentSubState;

    public DefaultState defaultState;

    protected bool amSubStateMachine;

    private void Awake()
    {
        debugging = false;
    }

    // Resets the FSM to be the default state
    public virtual void RestartFSM()
    {
        currentState?.ExitState();
        currentState = defaultState;
        previousState = defaultState;
        currentState.EntryState();
    }

    // Pefromed each Update Tick. NEEDS TO BE CALLED BY INHERITTED CLASS
    public void UpdateLoop()
    {
        if (currentState == null)
        {
            currentStateName = "Null State";
            currentSubStateName = "No Sub States";
            return;
        }
        // perform anything that is independent of being in any one single state
        this.PerformStateIndependentBehaviors();

        // Perform state behavior
        currentState.PerformBehavior();

        // check if the state needs to transition, and return the state it should belong in
        currentState = currentState.TransitionState();

        // checks if the prevousState is not the currentState
        this.CheckAndChangeCurrentStateListeners();
        EndLoop();

        // Debugging
        if (debugging)
        {
            currentStateName = currentState.ToString();
            previousStateName = previousState.ToString();
            if (currentState == null)
            {
                currentStateName = "Null State";
                currentSubStateName = "No Sub States";
            }
            if (previousState == null)
            {
                previousStateName = "Null State";
            }
            if (!currentState.HasSubStateMachine())
            {
                currentSubStateName = "No Sub States";
            }
            else
            {
#if UNITY_EDITOR
                currentSubState = currentState.GetCurrentSubState();
                currentSubStateName = currentSubState.ToString();
#endif
            }
        }
    }

    protected virtual void EndLoop()
    {
    }

    // perform anything that is independent of being in any one single state
    public virtual void PerformStateIndependentBehaviors() { }

    // checks if the prevousState is not the currentState
    private void CheckAndChangeCurrentStateListeners()
    {
        // If current state is a new transisiton, unsub from old listeners, and sub to new ones
        if (previousState != currentState)
        {
            previousState.ExitState();
            currentState.EntryState();
            previousState = currentState;
        }
    }

    // Returns currentState field
    public BasicState GetCurrentState() { return currentState; }

    public BasicState GetPreviousState()
    {
        return previousState;
    }

    public void SetAsSubStateMachine()
    {
        amSubStateMachine = true;
    }
}