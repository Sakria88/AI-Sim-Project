using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Selectable;

/// <summary>
/// This class defines the Scavenge state for the Night Crawler tank's finite state machine (FSM).
/// </summary>
public class NC_ScavengeState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;

    // slower movement speed while scavenging 
    private const float SCAVENGE_SPEED = 0.6f;

    // keeps track of which patrol point the tank is moving towards
    private int patrolIndex = 0;

    // array of posisitions used for patrol, tank moves in square if no resources are found.
    private Vector3[] patrolPoints =
    {
        new Vector3(65, 0, 65),
        new Vector3(65, 0, -65),
        new Vector3(-65, 0, -65),
        new Vector3(-65, 0, 65)
    };

    public NC_ScavengeState_FSMRBS(NC_SmartTank_FSMRBS tank)
    {
        this.nC_SmartTank_FSMRBS = tank;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering Scavenge (FSM + RBS)");

        nC_SmartTank_FSMRBS.stats["NC_ScavengeState_FSMRBS"] = true; // mark state as active

        // resets patrol index so movement starts consistently
        patrolIndex = 0;

        // allow movement
        nC_SmartTank_FSMRBS.TankGo();

        return null;
    }

    
    public override Type StateUpdate()
    {
       // checks resources 
        nC_SmartTank_FSMRBS.UpdateGlobalStats();

        // if all resources are sufficient, switch to Patrol state
        if (nC_SmartTank_FSMRBS.stats["enoughHealth"] && nC_SmartTank_FSMRBS.stats["enoughFuel"] && nC_SmartTank_FSMRBS.stats["enoughAmmo"])
        {
            return typeof(NC_PatrolState_FSMRBS); // Switch to Patrol state
        }
        // Scavenge logic
        if (nC_SmartTank_FSMRBS.consumable != null)
        {
            nC_SmartTank_FSMRBS.FollowPathToWorldPoint(nC_SmartTank_FSMRBS.consumable, 1f, nC_SmartTank_FSMRBS.heuristicMode);
        }
        else // moves around the map following a square pattern if no consumables are detected
        {
            // Move to the next patrol point
            GameObject point2 = nC_SmartTank_FSMRBS.CreateWorldPoint(patrolPoints[patrolIndex]);
            nC_SmartTank_FSMRBS.FollowPathToWorldPoint(point2, 0.5f, nC_SmartTank_FSMRBS.heuristicMode);

            // Check if the tank is close enough to the patrol point to switch to the next one
            if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, patrolPoints[patrolIndex]) < 5f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length; // Loop back to the first point
            }
            // Delete the temporary point object to avoid clutter
            GameObject.Destroy(point2);
        }

        // loop through every rule in rule system
        foreach (Rule rule in nC_SmartTank_FSMRBS.rules.GetRules)
        {   
            Type ruleResult = rule.CheckRule(nC_SmartTank_FSMRBS.stats); // asks the rule if it should trigger a state change
            if (ruleResult != null)
            {
                return ruleResult;
            }
        }

        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting Scavenge");
        // resets speed and turret so its not on 0.6f anymore.
        nC_SmartTank_FSMRBS.TurretReset();
        nC_SmartTank_FSMRBS.TankGo();

        return null;
    }
}
