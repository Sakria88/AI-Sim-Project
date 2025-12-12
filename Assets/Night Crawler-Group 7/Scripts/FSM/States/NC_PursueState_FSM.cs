using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Selectable;

/// <summary>
/// This class defines the Pursue state for the Night Crawler tank's finite state machine (FSM).
/// </summary>
public class NC_PursueState_FSM : NC_BaseState_FSM
{
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
        /*
         * State Transition Logic:
         * - If health < 35 or fuel < 35 or ammo < 3, transition to Scavenge state.
         * - If enemy tank is within 25 units, transition to Attack state.
         * - If enemy tank is lost, transition to Patrol state.
         * - Otherwise, continue pursuing the enemy tank.
         */

        if (nC_SmartTank_FSM.TankCurrentHealth < 35 || nC_SmartTank_FSM.TankCurrentFuel < 35 
            || nC_SmartTank_FSM.TankCurrentAmmo < 3)
        {
            return typeof(NC_ScavengeState_FSM); //Switch to scavenge state
        } else 
        {
            if (nC_SmartTank_FSM.NCEnTank != null)
            {
                //Store the distance between the tank and enemy tank as a varible
                float Distance = Vector3.Distance(nC_SmartTank_FSM.transform.position, nC_SmartTank_FSM.NCEnTank.transform.position);
                if (Distance < 25f) //If the distance between the tank and enemy is less than 25
                {
                    return typeof(NC_AttackState_FSM);//switch to the attack state
                } else
                {
                    PursueEnemy(); // Keep pursuing the enemy tank
                }
            }
            else
            {
                return typeof(NC_PatrolState_FSM); // Switch to patrol state
            }
        }
        return null;
    }

    /// <summary>
    /// Pursues the enemy tank by following its path.
    /// </summary>
    public void PursueEnemy()
    {
        nC_SmartTank_FSM.FollowPathToWorldPoint(nC_SmartTank_FSM.NCEnTank, 1f, nC_SmartTank_FSM.heuristicMode);
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting the purse state");
        return null;
    }
}