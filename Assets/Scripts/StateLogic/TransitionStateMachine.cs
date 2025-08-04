using System;
using System.Collections.Generic;

public class TransitionStateMachine
{
    private Dictionary<int, Action> enterActions;
    private Dictionary<int, Action> exitActions;
    private int state, numOfStates;

    public TransitionStateMachine(int numOfStates)
    {
        this.numOfStates = numOfStates;
        state = -1;
    }

    public void SetStateFunctions(int n, Action enter, Action exit)
    {
        enterActions[n] = enter;
        exitActions[n] = exit;
    }

    public void SetEnterFunction(int n, Action enter)
    {
        enterActions[n] = enter;
    }

    public void SetExitFunction(int n, Action exit)
    {
        exitActions[n] = exit;
    }

    public void SetState(int n)
    {
        if (state >= 0 && state < numOfStates)
        {
            exitActions[state]();
        }
        state = n;
        enterActions[state]();
    }
}
