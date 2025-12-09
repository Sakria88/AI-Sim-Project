using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_ScavengeState_FSM: NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_ScavengeState_FSM(NC_SmartTank_FSM nC_SmartTank_FSM)
    {
        this.nC_SmartTank_FSM = nC_SmartTank_FSM;
    }

    //each state will have these 3 functions and because they will follow the same structure the state machine will be able to talk to all the states 
    // and switch between them eassily.

    //what the state will do on entry
    public override Type StateEnter()
    {
        return null;
    }

    //What a state will do when it updates every frame
    public override Type StateUpdate()
    {
        return null;
    }

    //What a state will do when they leave
    public override Type StateExit()
    {
        return null;
    }
}