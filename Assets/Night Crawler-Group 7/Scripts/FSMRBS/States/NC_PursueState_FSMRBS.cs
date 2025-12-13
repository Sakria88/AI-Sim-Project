using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Selectable;

//This class is for the purse state-the purse state goes after the enemy tank
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

        Debug.Log("They see me PURSUEING");


        if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position) <= 50)
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

    public void PursueEnemy()//function to keep pursing
    {
        nC_SmartTank_FSMRBS.checkLowHealth();
        nC_SmartTank_FSMRBS.CheckTargetReached();
        nC_SmartTank_FSMRBS.FollowPathToWorldPoint(nC_SmartTank_FSMRBS.NCEnTank, 1f, nC_SmartTank_FSMRBS.heuristicMode); //follow the enemy tank at a speed of one with generic heuristic
    }

    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.stats["NC_PursueState_FSMRBS"] = false;
        Debug.Log("Exiting the purse state");
        return null;
    }
}