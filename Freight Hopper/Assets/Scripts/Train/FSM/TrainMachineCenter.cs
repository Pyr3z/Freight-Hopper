using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrainMachineCenter : FiniteStateMachineCenter
{
    private TrainStateTransitions transitionHandler;

    // States

    public FindNextPathState findNextPath;
    public FollowingPathState followPath;
    public RefindPathState refindPath;
    public WaitingState waiting;
    public WanderState wander;

    // Independent Data

    [SerializeField] private Optional<float> startWaitTime;
    [SerializeField] private Optional<float> startWhenDistanceFromPlayer;
    public Optional<float> StartWaitTime => startWaitTime;
    public Optional<float> StartWhenDistanceFromPlayer => startWhenDistanceFromPlayer;

    private void Awake()
    {
        transitionHandler = new TrainStateTransitions(this);

        // Default
        List<Func<BasicState>> defaultTransitionsList = new List<Func<BasicState>>();
        defaultTransitionsList.Add(transitionHandler.CheckStartState);
        defaultState = new DefaultState(this, defaultTransitionsList);

        // Find Next Path
        List<Func<BasicState>> findNextPathTransitionsList = new List<Func<BasicState>>();
        findNextPath = new FindNextPathState(this, findNextPathTransitionsList);

        // Follow Path
        List<Func<BasicState>> followPathTransitionsList = new List<Func<BasicState>>();
        followPath = new FollowingPathState(this, followPathTransitionsList);

        // Refind Path
        List<Func<BasicState>> refindPathTransitionsList = new List<Func<BasicState>>();
        refindPath = new RefindPathState(this, refindPathTransitionsList);

        // Waiting
        List<Func<BasicState>> waitingPathTransitionsList = new List<Func<BasicState>>();
        waiting = new WaitingState(this, waitingPathTransitionsList);

        // Wander
        List<Func<BasicState>> wanderPathTransitionsList = new List<Func<BasicState>>();
        wander = new WanderState(this, wanderPathTransitionsList);
    }

    public void OnEnable()
    {
        RestartFSM();
    }

    public void OnDisable()
    {
        currentState.ExitState();
    }

    private void FixedUpdate()
    {
        UpdateLoop();
    }
}