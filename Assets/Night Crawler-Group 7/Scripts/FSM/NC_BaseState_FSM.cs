using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//This base class is for setting the structure for the state classes 
public abstract class NC_BaseState_FSM
{
    //each state will have these 3 functions and because they will follow the same structure the state machine will be able to talk to all the states 
    // and switch between them eassily.

    /// <summary>
    /// What a state will do when they enter
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