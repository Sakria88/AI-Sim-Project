using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Selectable;

public class NC_AttackState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_AttackState_FSM(NC_SmartTank_FSM nC_SmartTank_FSM)
    {
        this.nC_SmartTank_FSM = nC_SmartTank_FSM;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering the attack state FSM");
        return null;
    }

    public override Type StateUpdate()
    {
        if (nC_SmartTank_FSM.TankCurrentHealth < 35f)
        {
            return typeof(NC_RetreatState_FSM);
        }
        else if (nC_SmartTank_FSM.TankCurrentFuel < 35f || nC_SmartTank_FSM.TankCurrentAmmo == 0)
        {
            return typeof(NC_ScavengeState_FSM);
        }
        else if (nC_SmartTank_FSM.NCEnTank != null)
        {
            float distance = Vector3.Distance(nC_SmartTank_FSM.transform.position, nC_SmartTank_FSM.NCEnTank.transform.position);
            if (distance > 30f)
            {
                return typeof(NC_PursueState_FSM);
            }
            else
            {
                // Attack logic
                nC_SmartTank_FSM.TurretFireAtPoint(nC_SmartTank_FSM.NCEnTank);
            }
        }
        else
        {
            return typeof(NC_PatrolState_FSM);
        }
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting the attack state");
        return null;
    }
}

