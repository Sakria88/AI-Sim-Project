using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Selectable;

public class NC_ScavengeState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;
    private int patrolIndex = 0;



    public 
        NC_ScavengeState_FSM(NC_SmartTank_FSM tank)
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
        //Current State   Transition Condition    Next State
        //Scavange Enemy visible & Fuel > 45 & Health < 30 Retreat
        //Scavange    Health & Fuel >= 50 & Ammo > 5  Patrol
        if (nC_SmartTank_FSM.NCEnTank != null && nC_SmartTank_FSM.TankCurrentFuel > 45f && nC_SmartTank_FSM.TankCurrentHealth < 30f)
        {
            return typeof(NC_RetreatState_FSM);
        }
        else if (nC_SmartTank_FSM.TankCurrentHealth > 50f && nC_SmartTank_FSM.TankCurrentFuel > 50f && nC_SmartTank_FSM.TankCurrentAmmo > 5)
        {
            return typeof(NC_PatrolState_FSM);
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
                    new Vector3(80, 0, 80),
                    new Vector3(80, 0, -80),
                    new Vector3(-80, 0, -80),
                    new Vector3(-80, 0, 80)
                };
                // Move to the next patrol point
                GameObject point = nC_SmartTank_FSM.CreateWorldPoint(patrolPoints[patrolIndex]);
                nC_SmartTank_FSM.FollowPathToWorldPoint(point, 1f, nC_SmartTank_FSM.heuristicMode);

                // Check if the tank is close enough to the patrol point to switch to the next one
                if (Vector3.Distance(nC_SmartTank_FSM.transform.position, patrolPoints[patrolIndex]) < 5f)
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length; // Loop back to the first point
                }
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
