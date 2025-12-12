using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// Abstract base class for defining states in a Finite State Machine (FSM).
/// </summary>
public abstract class NC_BaseState_FSM
{
    /*  each state will have these 3 functions and because 
     *  they will follow the same structure the state machine 
     *  will be able to talk to all the states 
     *  and switch between them eassily.
     */


    /// <summary>
    /// What the state will do on entry
    /// </summary>
    public abstract Type StateEnter();

    /// <summary>
    ///  What a state will do when it updates every frame
    /// </summary>
    public abstract Type StateUpdate();

    /// <summary>
    /// What a state will do when they leave
    /// </summary>
    public abstract Type StateExit();
}