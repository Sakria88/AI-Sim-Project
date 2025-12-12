using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// Implements a generic Finite State Machine (FSM) responsible for managing
/// state initialization, execution, and transitions. Designed to be reusable
/// across different entity types within Unity.
/// </summary>
public class NC_StateMachine_FSMRBS : MonoBehaviour
{
    private Dictionary<Type, NC_StateMachine_FSMRBS> states; // Holds all possible states
    public NC_StateMachine_FSMRBS currentState; // Holds the current state

    /// <summary>
    /// Provides access to the currently active state.
    /// Setter is private to ensure controlled state transitions.
    /// </summary>
    public NC_StateMachine_FSMRBS CurrentState
    {
        get
        {
            return currentState;
        }
        private set
        {
            currentState = value; // Set is private to prevent external modification
        }
    }

    /// <summary>
    /// Injects the available states into the FSM.
    /// Must be called before the state machine begins operating.
    /// </summary>
    public void SetStates(Dictionary<Type, NC_StateMachine_FSMRBS> states)
    {
        this.states = states; // Assign the provided states dictionary to the class member
    }

    /// <summary>
    /// Unity's per-frame update loop.
    /// Handles initial state assignment and delegates update logic to the current state.
    /// Also manages transitions when a state requests a change.
    /// </summary>
    void Update()
    {
        if (CurrentState == null) // If no current state is set set it to the first state in the dictionary
        {
            CurrentState = states.Values.First();
        }
        else
        {
            var nextState = CurrentState.StateUpdate(); // Call StateUpdate on the current state to determine the next state
            if (nextState != null && nextState != CurrentState.GetType()) // If a next state is returned and it's different from the current state
            {
                SwitchToState(nextState); // Switch to the next state
            }
        }
    }

    /// <summary>
    /// Performs a full state transition, including exit logic for the current state
    /// and enter logic for the new one. Ensures each state properly handles its lifecycle.
    /// </summary>
    void SwitchToState(Type nextState)
    {
        CurrentState.StateExit(); // Call StateExit on the current state
        CurrentState = states[nextState]; // Update the current state to the new state
        CurrentState.StateEnter(); // Call StateEnter on the new current state
    }
}
