using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Selectable;

/// <summary>
/// Patrol state (FSMRBS)
/// Continuous movement state – rules may interrupt, but patrol never stalls.
/// </summary>
public class NC_PatrolState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;
    private bool patrolPathRequested = false;

    public NC_PatrolState_FSMRBS(NC_SmartTank_FSMRBS tankRef)
    {
        nC_SmartTank_FSMRBS = tankRef;
    }

    public override Type StateEnter()
    {
        nC_SmartTank_FSMRBS.stats["NC_PatrolState_FSMRBS"] = true;
        patrolPathRequested = false;
        return null;
    }

    public override Type StateUpdate()
    {
        // -----------------------------
        // UPDATE FACTS (RBS)
        // -----------------------------
        nC_SmartTank_FSMRBS.CheckEnemyInSight();
        nC_SmartTank_FSMRBS.CheckEnemyDetected();
        nC_SmartTank_FSMRBS.CheckEnemyBaseDetected();
        nC_SmartTank_FSMRBS.CheckTargetSpotted();

        nC_SmartTank_FSMRBS.CheckLowHealth();
        nC_SmartTank_FSMRBS.CheckLowFuel();
        nC_SmartTank_FSMRBS.CheckLowAmmo();

        // -----------------------------
        // PATROL MOVEMENT (DO NOT SPAM)
        // -----------------------------
        if (!patrolPathRequested)
        {
            nC_SmartTank_FSMRBS.FollowPathToRandomWorldPoint(
                1f,
                nC_SmartTank_FSMRBS.heuristicMode
            );

            patrolPathRequested = true;
        }

        // -----------------------------
        // RULE EVALUATION
        // -----------------------------
        foreach (Rule rule in nC_SmartTank_FSMRBS.rules.GetRules)
        {
            Type nextState = rule.CheckRule(nC_SmartTank_FSMRBS.stats);
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
        patrolPathRequested = false;
        return null;
    }
}
