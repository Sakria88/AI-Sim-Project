using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Selectable;

// This class is for the patrol state – roaming and deferring decisions to RBS
public class NC_PatrolState_FSMRBS : NC_BaseState_FSMRBS
{
    // follow SAME naming convention as Pursue
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;

    public NC_PatrolState_FSMRBS(NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS)
    {
        Debug.Log("FSM passed to PatrolState = " + nC_SmartTank_FSMRBS);
        this.nC_SmartTank_FSMRBS = nC_SmartTank_FSMRBS;
    }

    public override Type StateEnter()
    {
        nC_SmartTank_FSMRBS.stats["NC_PatrolState_FSMRBS"] = true;
        Debug.Log("Entering the patrol state FSM");
        return null;
    }

    public override Type StateUpdate()
    {
        Debug.Log("Patrolling...");

        // -----------------------------
        // FACT UPDATES (ONLY WHAT PATROL NEEDS)
        // -----------------------------
        nC_SmartTank_FSMRBS.CheckEnemyInSight();
        nC_SmartTank_FSMRBS.CheckEnemyBaseDetected();

        nC_SmartTank_FSMRBS.CheckLowHealth();
        nC_SmartTank_FSMRBS.CheckLowFuel();
        nC_SmartTank_FSMRBS.CheckLowAmmo();

        // Distance checks ONLY if enemy exists
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            nC_SmartTank_FSMRBS.CheckEnemyDistanceClose();
            nC_SmartTank_FSMRBS.CheckEnemyDistanceMid();
            nC_SmartTank_FSMRBS.CheckEnemyDistanceFar();
        }
        else
        {
            nC_SmartTank_FSMRBS.stats["enemyDistanceClose"] = false;
            nC_SmartTank_FSMRBS.stats["enemyDistanceMid"] = false;
            nC_SmartTank_FSMRBS.stats["enemyDistanceFar"] = false;
        }

        // -----------------------------
        // DEFAULT FSM BEHAVIOUR
        // -----------------------------
        nC_SmartTank_FSMRBS.FollowPathToRandomWorldPoint(
            1f,
            nC_SmartTank_FSMRBS.heuristicMode
        );

        // -----------------------------
        // RULE CHECK (IDENTICAL TO PURSUE)
        // -----------------------------
        foreach (Rule item in nC_SmartTank_FSMRBS.rules.GetRules)
        {
            Type nextState = item.CheckRule(nC_SmartTank_FSMRBS.stats);
            if (nextState != null)
            {
                return nextState;
            }
        }

        return null;
    }

    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.stats["NC_PatrolState_FSMRBS"] = false;
        Debug.Log("Exiting the patrol state");
        return null;
    }
}

