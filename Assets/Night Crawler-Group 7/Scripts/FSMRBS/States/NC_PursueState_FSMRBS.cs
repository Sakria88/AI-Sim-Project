using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Selectable;

/// <summary>
/// Pursue state (FSMRBS) – tank chases enemy tank when detected.
/// </summary>
public class NC_PursueState_FSMRBS : NC_BaseState_FSMRBS
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;

    public NC_PursueState_FSMRBS(NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS)
    {
        Debug.Log("FSM passed to PursueState = " + nC_SmartTank_FSMRBS);
        this.nC_SmartTank_FSMRBS = nC_SmartTank_FSMRBS;
    }

    public override Type StateEnter()
    {
        nC_SmartTank_FSMRBS.stats["NC_PursueState_FSMRBS"] = true;

        Debug.Log("Entering the pursue state FSM");
        return null;
    }

    public override Type StateUpdate()
    {
        nC_SmartTank_FSMRBS.UpdateGlobalStats();
        nC_SmartTank_FSMRBS.CheckTargetReached(); //check if the target is reached

        //If enemy tank is not null pursue it
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            PursueEnemy(); //function to pursue enemy
        }
        else
        {
            return typeof(NC_PatrolState_FSMRBS);
        }

        //Check the rules to see if there are any that need to be used
        foreach (Rule item in nC_SmartTank_FSMRBS.rules.GetRules)
        {
            if (item.CheckRule(nC_SmartTank_FSMRBS.stats) != null)

            {
                return item.CheckRule(nC_SmartTank_FSMRBS.stats);
            }
        }

        return null;
    }

    /// <summary>
    /// Function to pursue the enemy tank
    /// </summary>
    public void PursueEnemy()
    {
        nC_SmartTank_FSMRBS.FollowPathToWorldPoint(nC_SmartTank_FSMRBS.NCEnTank, 1f, nC_SmartTank_FSMRBS.heuristicMode); //follow the enemy tank at a speed of one with generic heuristic
    }
    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.stats["NC_PursueState_FSMRBS"] = false;
        Debug.Log("Exiting the purse state");
        return null;
    }
}