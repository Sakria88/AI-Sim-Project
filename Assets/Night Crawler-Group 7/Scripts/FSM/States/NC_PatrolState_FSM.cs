using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class defines the Patrol state for the Night Crawler tank's finite state machine (FSM).
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
        /*  Patrol State Logic:
         * 1. If health or fuel is below 35%, transition to Scavenge state.
         * 2. If an enemy tank is detected within 52 units, transition to Pursue state.
         * 3. If an enemy base is detected within 52 units, transition to Base Attack state.
         * 4. If a consumable is detected within 52 units, transition to Pursue state.
         * 5. Otherwise, continue patrolling by moving to random points on the map.
         */

        // ------------------------------------------------
        // HEALTH | FUEL CHECK → SCAVENGE
        // ------------------------------------------------
        if (nC_SmartTank_FSM.TankCurrentHealth < 35f || nC_SmartTank_FSM.TankCurrentFuel < 35f)
        {
            return typeof(NC_ScavengeState_FSM);
        }

        // ---------------------------------------
        // CONSUMABLE WITHIN 52 → SCAVENGE
        // ---------------------------------------
        else if (nC_SmartTank_FSM.consumable != null)
        {
            return typeof(NC_ScavengeState_FSM);
        }

        // ------------------------------------------------
        // ENEMY TANK WITHIN 52 → PURSUE
        // ------------------------------------------------
        else if (nC_SmartTank_FSM.NCEnTank != null)
        {
            return typeof(NC_PursueState_FSM);
        }

        // ------------------------------------------------
        // ENEMY BASE WITHIN 52 → BASE ATTACK
        // ------------------------------------------------
        else if (nC_SmartTank_FSM.NCEnBase != null)
        {
            return typeof(NC_BaseAttackState_FSM);
        }

        // ------------------------------------------------
        // CONTINUOUS MOVEMENT ACROSS THE WHOLE MAP
        // ------------------------------------------------
        else
        {
            nC_SmartTank_FSM.FollowPathToRandomWorldPoint(1f, nC_SmartTank_FSM.heuristicMode);
        }

        return null; // stay in patrol
    }

    public override Type StateExit()
    {
        Debug.Log("EXITING PATROL");
        return null;
    }
}
