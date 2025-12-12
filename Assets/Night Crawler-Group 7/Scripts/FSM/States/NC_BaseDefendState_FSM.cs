using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AStar;

public class NC_BaseDefendState_FSM: NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_BaseDefendState_FSM(NC_SmartTank_FSM nC_SmartTank_FSM)
    {
        this.nC_SmartTank_FSM = nC_SmartTank_FSM;
    }

    public override Type StateEnter()
    {
        return null;
    }

    public override Type StateUpdate()
    {
        /*
         * State Transition Logic:
         * - If an enemy tank is detected, transition to Pursue state.
         * - If no enemy tanks are detected, continue defending the base by moving towards it and facing away from it.
         */

        DefendAllyBase();
        if (nC_SmartTank_FSM.NCEnTank != null)
        {
            return typeof(NC_PursueState_FSM); // Switch to Pursue state
        } else {
            return typeof(NC_PatrolState_FSM); // Switch to Patrol state
        }
    }

    public override Type StateExit()
    {
        return null;
    }

    /// <summary>
    /// Defend the allied base by moving towards it and facing away from it.
    /// </summary>
    public void DefendAllyBase()
    {
        GameObject myBase = nC_SmartTank_FSM.MyBases[0];

        if (myBase != null)
        {
            if (Vector3.Distance(nC_SmartTank_FSM.transform.position, myBase.transform.position) < 25f)
            {
                Vector3 directionFromBase = nC_SmartTank_FSM.transform.position - myBase.transform.position; // Calculate direction away from base
                GameObject pointToFace = new GameObject(); // Create a temporary point to face
                pointToFace.transform.position = nC_SmartTank_FSM.transform.position + directionFromBase.normalized * 10f; // Position it further away in that direction
                nC_SmartTank_FSM.TurretFaceWorldPoint(pointToFace); // Face the turret towards that point
            }
            else
            {
                nC_SmartTank_FSM.FollowPathToWorldPoint(myBase, 1f, nC_SmartTank_FSM.heuristicMode); // Move towards the base
            }
        }
    }
}