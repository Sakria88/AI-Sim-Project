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
    private NC_SmartTank_FSMRBS nC_SmartTank_FSM;

    public NC_PursueState_FSMRBS(NC_SmartTank_FSMRBS NCTank)
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
        if (nC_SmartTank_FSM.TankCurrentHealth < 35 || nC_SmartTank_FSM.TankCurrentFuel < 35 || nC_SmartTank_FSM.TankCurrentAmmo < 3) //If health or fuel is less than 35 or ammo is less than 3
        {
            return typeof(NC_ScavengeState_FSM); //Switch to scavenge state
        } else {
            if (nC_SmartTank_FSM.NCEnTank != null) //If enemy tank is there
            {
                //Store the distance between the tank and enemy tank as a varible
                float Distance = Vector3.Distance(nC_SmartTank_FSM.transform.position, nC_SmartTank_FSM.NCEnTank.transform.position);
                if (Distance < 25f) //If the distance between the tank and enemy is less than 25
                {
                    return typeof(NC_AttackState_FSM);//switch to the attack state
                } else
                {
                    PursueEnemy(); //If not less thank 25 keep pursuing
                }
            }
            else
            {
                return typeof(NC_PatrolState_FSM); //If enemy tank is lost switch to patrol state
            }
        }
        return null;
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