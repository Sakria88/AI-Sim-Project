using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_AttackState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_AttackState_FSM(NC_SmartTank_FSM nC_SmartTank_FSM)
    {
        this.nC_SmartTank_FSM = nC_SmartTank_FSM;
    }

    public override Type StateEnter()
    {
        return null;
    }

    public override Type StateUpdate()
    {
        // target assigned by smart tank.
        GameObject target = nC_SmartTank_FSM.NCEnTank;

        // no target found then go to patrol state
        if (target == null)
        {
            return typeof(NC_PatrolState_FSM);
        }

        // target must currently be visible to attack
        Dictionary<GameObject, float> visible = nC_SmartTank_FSM.VisibleEnemyTanks;
        if (visible == null || !visible.ContainsKey(target))
        {
            return typeof(NC_PatrolState_FSM);
        }

        float distanceToTarget = visible[target];

        // if target is less that 45f then attack otherwise pursue
        if (distanceToTarget < 45f)
        {
            nC_SmartTank_FSM.TurretFaceWorldPoint(target);
            nC_SmartTank_FSM.TurretFireAtPoint(target);
            return null;
        }


        return typeof(NC_PursueState_FSM);
    }

    public override Type StateExit()
    {
        return null;
    }
}

