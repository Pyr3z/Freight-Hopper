using System.Collections.Generic;
using System;

public class DefaultState : BasicState
{
    public DefaultState(FiniteStateMachineCenter finiteStateMachineCenter, List<Func<BasicState>> stateTransitions) : base(finiteStateMachineCenter, stateTransitions)
    {
    }
    
    public DefaultState(FiniteStateMachineCenter finiteStateMachineCenter) : base(finiteStateMachineCenter)
    {
    }

    public override BasicState TransitionState()
    {
        BasicState state = CheckTransitions();
        return state;
    }

    public override void PerformBehavior()
    {
    }
}