using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// NightCrawler PATROL STATE (FSM ONLY)
/// following FSM transition table.
/// </summary>
public class NC_PatrolState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_PatrolState_FSM(NC_SmartTank_FSM tankRef)
    {
        nC_SmartTank_FSM = tankRef;
    }

    public override Type StateEnter()
    {
        Debug.Log("ENTERING PATROL (GLOBAL SEARCH MODE)");

        return null;
    }

    public override Type StateUpdate()
    {
        // --------------------------------------------
        // 3. THIRD PRIORITY → CHECK FOR ENEMY TANKS
        // --------------------------------------------
        if (nC_SmartTank_FSM.VisibleEnemyTanks.Count > 0)
        {
            nC_SmartTank_FSM.NCEnTank = nC_SmartTank_FSM.VisibleEnemyTanks.First().Key;
            if (nC_SmartTank_FSM.NCEnTank != null)
            {
                return typeof(NC_PursueState_FSM);//switch to the attack state
            }
        } else
        {
            nC_SmartTank_FSM.FollowPathToRandomWorldPoint(0.5f, nC_SmartTank_FSM.heuristicMode);
        }
    // stay in Patrol
    return null;
    }
    public override Type StateExit()
    {
        Debug.Log("EXITING PATROL");
        return null;
    }

}
