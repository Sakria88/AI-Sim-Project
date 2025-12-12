using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Selectable;

/// <summary>
/// This class defines the Scavenge state for the Night Crawler tank's finite state machine (FSM).
/// </summary>
public class NC_ScavengeState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;
    private int patrolIndex = 0; // Index for patrol points in square pattern

    public NC_ScavengeState_FSM(NC_SmartTank_FSM tank)
    {
        this.nC_SmartTank_FSM = tank;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering the scavenge state FSM");
        patrolIndex = 0;
        // makes sure tank starts moving normally, good practice for reducing bugs
        nC_SmartTank_FSM.TankGo();

        return null;
    }

    public override Type StateUpdate()
    {
        /*
         * State Transition Logic:
         * - If the tank has an enemy target, sufficient fuel (>45), but low health (<30), transition to Retreat state.
         * - If the tank has good health (>50), fuel (>50), and ammo (>5), transition to Patrol state.
         * - Otherwise, continue scavenging for resources.
         * - If a consumable is detected, move towards it.
         * - If no consumables are detected, follow a square patrol pattern around the map.
         */

        if (nC_SmartTank_FSM.NCEnTank != null && nC_SmartTank_FSM.TankCurrentFuel > 45f && nC_SmartTank_FSM.TankCurrentHealth < 30f)
        {
            return typeof(NC_RetreatState_FSM); // Switch to Retreat state
        }
        else if (nC_SmartTank_FSM.TankCurrentHealth > 50f && nC_SmartTank_FSM.TankCurrentFuel > 50f && nC_SmartTank_FSM.TankCurrentAmmo > 5)
        {
            return typeof(NC_PatrolState_FSM); // Switch to Patrol state
        }
        else
        {
            // Scavenge logic
            if (nC_SmartTank_FSM.consumable != null)
            {
                nC_SmartTank_FSM.FollowPathToWorldPoint(nC_SmartTank_FSM.consumable, 1f, nC_SmartTank_FSM.heuristicMode);
            }
            else // moves around the map following a square pattern if no consumables are detected
            {
                // Define square patrol points
                Vector3[] patrolPoints = new Vector3[]
                {
                    new Vector3(65, 0, 65),
                    new Vector3(65, 0, -65),
                    new Vector3(-65, 0, -65),
                    new Vector3(-65, 0, 65)
                };
                // Move to the next patrol point
                GameObject point = nC_SmartTank_FSM.CreateWorldPoint(patrolPoints[patrolIndex]);
                nC_SmartTank_FSM.FollowPathToWorldPoint(point, 0.5f, nC_SmartTank_FSM.heuristicMode);

                // Check if the tank is close enough to the patrol point to switch to the next one
                if (Vector3.Distance(nC_SmartTank_FSM.transform.position, patrolPoints[patrolIndex]) < 5f)
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length; // Loop back to the first point
                }
                // Delete the temporary point object to avoid clutter
                GameObject.Destroy(point);
            }
        }
            return null; // stays in Scavenge state
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting the scavenge state");
        // reset turret and allow tank to move normally again, prevents bugs for next state
        nC_SmartTank_FSM.TurretReset();
        nC_SmartTank_FSM.TankGo();
        return null;
    }
}
