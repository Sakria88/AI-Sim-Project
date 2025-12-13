using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Selectable;


/// Scavenge state using FSM + Rule-Based System.


public class NC_ScavengeState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS tank;

    // Slower movement speed while scavenging 
    private const float SCAVENGE_SPEED = 0.6f;

    private int patrolIndex = 0;

    private Vector3[] patrolPoints =
    {
        new Vector3(80, 0, 80),
        new Vector3(80, 0, -80),
        new Vector3(-80, 0, -80),
        new Vector3(-80, 0, 80)
    };

    public NC_ScavengeState_FSMRBS(NC_SmartTank_FSMRBS tank)
    {
        this.tank = tank;
    }

    
    public override Type StateEnter()
    {
        tank.stats["NC_ScavengeState_FSMRBS"] = true;

        Debug.Log("Entering Scavenge (FSM + RBS)");

        patrolIndex = 0;

        // Allow movement, but speed is controlled 
        tank.TankGo();

        return null;
    }

    
    public override Type StateUpdate()
    {
        
        tank.CheckLowHealth();
        tank.CheckLowFuel();
        tank.CheckLowAmmo();
        tank.CheckSafeZoneReached(Vector3.zero, 15f);

        bool resourcesLow =
            tank.stats["lowHealth"] ||
            tank.stats["lowFuel"] ||
            tank.stats["lowAmmo"];

        tank.stats["resourcesLow"] = resourcesLow;

        
        // RULE-BASED SYSTEM 
        
        foreach (Rule rule in tank.rules.GetRules)
        {
            Type ruleResult = rule.CheckRule(tank.stats);
            if (ruleResult != null)
            {
                return ruleResult;
            }
        }

        // FSM TRANSITIONS
        
        if (tank.stats["enoughHealth"] &&
            tank.stats["enoughFuel"] &&
            tank.stats["enoughAmmo"])
        {
            return typeof(NC_PatrolState_FSMRBS);
        }

        // SCAVENGE BEHAVIOUR (SLOW SPEED)
      

        // PRIORITY 1: Health pickup
        if (tank.consumableHealth != null)
        {
            tank.FollowPathToWorldPoint(
                tank.consumableHealth,
                SCAVENGE_SPEED,
                tank.heuristicMode);

            return null;
        }

        // PRIORITY 2: Fuel pickup
        if (tank.consumableFuel != null)
        {
            tank.FollowPathToWorldPoint(
                tank.consumableFuel,
                SCAVENGE_SPEED,
                tank.heuristicMode);

            return null;
        }

        // PRIORITY 3: Ammo pickup
        if (tank.consumableAmmo != null)
        {
            tank.FollowPathToWorldPoint(
                tank.consumableAmmo,
                SCAVENGE_SPEED,
                tank.heuristicMode);

            return null;
        }

        
      
        Vector3 target = patrolPoints[patrolIndex];

        
        GameObject point = tank.CreateWorldPoint(target);
        tank.FollowPathToWorldPoint(point, SCAVENGE_SPEED, tank.heuristicMode);

        if (Vector3.Distance(tank.transform.position, target) < 6f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        return null;
    }

    
    public override Type StateExit()
    {
        tank.stats["NC_ScavengeState_FSMRBS"] = false;

        Debug.Log("Exiting Scavenge");

        tank.TurretReset();
        tank.TankGo();

        return null;
    }
}
