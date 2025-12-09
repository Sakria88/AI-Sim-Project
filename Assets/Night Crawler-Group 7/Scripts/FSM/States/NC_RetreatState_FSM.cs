using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_RetreatState_FSM: NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_RetreatState_FSM(NC_SmartTank_FSM nC_SmartTank_FSM)
    {
        this.nC_SmartTank_FSM = nC_SmartTank_FSM;
    }

    public override Type StateEnter()
    {
        throw new NotImplementedException();
    }

    public override Type StateExit()
    {
        throw new NotImplementedException();
    }

    public override Type StateUpdate()
    {
        throw new NotImplementedException();
    }
}