using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

//This class is for the purse state-the purse state goes after the enemy tank
public class NC_PursueState_FSM : NC_BaseState_FSM
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_PursueState_FSM(NC_SmartTank_FSM NCTank)
    {
        this.nC_SmartTank_FSM = NCTank;         
    }

    public override Type StateEnter()
    {
       Debug.Log("Entering the pursue state FSM");
       return null;
    }

    public override Type StateUpdate()
    {
        Debug.Log("I'm Updating Pursue State");
        if(nC_SmartTank_FSM.VisibleEnemyTanks.Count > 0)
        {
            nC_SmartTank_FSM.NCEnTank = nC_SmartTank_FSM.VisibleEnemyTanks.First().Key;
            if(nC_SmartTank_FSM.NCEnTank != null)
            {
                float Distance = Vector3.Distance(nC_SmartTank_FSM.transform.position, nC_SmartTank_FSM.NCEnTank.transform.position);
                if (Distance < 45f) //If the distance between the tank and enemy is less than 45
                {
                    return typeof(NC_AttackState_FSM);//switch to the attack state
                }
            }
        }
        else
        {
            PursueEnemy();
        }
        return null;
        //if (VisibleEnemyTank.First().key != null)

        //    // tank.NCEnTank = VisibleEnemyTank.First().key;
        //    if (nC_SmartTank_FSM.NCEnTank != null) //If enemy tank is there
        //    {
        //        //Store the distance between the tank and enemy tank as a varible
        //        float Distance = Vector3.Distance(nC_SmartTank_FSM.transform.position, nC_SmartTank_FSM.NCEnTank.transform.position);

        //        if (Distance < 30f) //If the distance between the tank and enemy is less than 30
        //        {
        //            return typeof(NC_AttackState_FSM);//switch to the attack state

        //        }
        //    }
        //    else
        //    {
        //        //FollowPathToWorldPoint(NCEnTank, 1f, heuristicMode);
        //        PursueEnemy(); //If not less thank 30 keep pursuing
        //}
    }

    public void PursueEnemy()//function to keep pursing
    {
        nC_SmartTank_FSM.FollowPathToWorldPoint(nC_SmartTank_FSM.NCEnTank, 1f, nC_SmartTank_FSM.heuristicMode); //follow the enemy tank at a speed of one with generic heuristic
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting the purse state");
        return null;
    }
}