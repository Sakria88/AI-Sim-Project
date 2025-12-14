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
    // Reference to the Smart Tank
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;

    // --------------------------------------------------
    // CONSTRUCTOR
    // --------------------------------------------------
    public NC_PatrolState_FSMRBS(NC_SmartTank_FSMRBS tankRef)
    {
        nC_SmartTank_FSMRBS = tankRef;
    }

    // --------------------------------------------------
    // ENTER
    // --------------------------------------------------
    public override Type StateEnter()
    {
        // Mark Patrol active
        nC_SmartTank_FSMRBS.stats["NC_PatrolState_FSMRBS"] = true;
        nC_SmartTank_FSMRBS.TankGo();

        Debug.Log("ENTERING PATROL (FSMRBS)");
        return null;
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------
    public override Type StateUpdate()
    {
        // -----------------------------
        // FACT UPDATES (RBS)
        // -----------------------------
        nC_SmartTank_FSMRBS.UpdateGlobalStats();

        // -----------------------------
        // ACTUAL MOVEMENT (PATROL OWNS THIS)
        // -----------------------------

        // if consumabkle visible get it
        if (nC_SmartTank_FSMRBS.consumable != null)
        {
            nC_SmartTank_FSMRBS.FollowPathToWorldPoint(nC_SmartTank_FSMRBS.consumable, 1f, nC_SmartTank_FSMRBS.heuristicMode);
        }else
        {
            nC_SmartTank_FSMRBS.FollowPathToRandomWorldPoint(1f, nC_SmartTank_FSMRBS.heuristicMode);
        }


        // -----------------------------
        // RULE EVALUATION (FSMRBS)
        // -----------------------------
        foreach (Rule rule in nC_SmartTank_FSMRBS.rules.GetRules)
        {
            Type nextState = rule.CheckRule(nC_SmartTank_FSMRBS.stats);
            if (nextState != null)
            {
                return nextState;
            }
        }

        return null; // remain in Patrol
    }

    // --------------------------------------------------
    // EXIT
    // --------------------------------------------------
    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.stats["NC_PatrolState_FSMRBS"] = false;
        nC_SmartTank_FSMRBS.TankStop();
        Debug.Log("EXITING PATROL");
        return null;
    }
}