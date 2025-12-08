using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//This base class is for setting the structure for the state classes 
public abstract class NC_BaseScript_FSM
{
    //each state will have these 3 functions and because they will follow the same structure the state machine will be able to talk to all the states 
    // and switch between them eassily.

    //what the state will do on entry
    public abstract Type StateEnter();

    //What a state will do when it updates every frame
    public abstract Type StateUpdate();

    //What a state will do when they leave
    public abstract Type StateExit();


}