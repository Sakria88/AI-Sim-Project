using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Selectable;


/// scavenge state using FSM + Rule-Based System.


public class NC_ScavengeState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS tank;

    // slower movement speed while scavenging 
    private const float SCAVENGE_SPEED = 0.6f;

    // keeps track of which patrol point the tank is moving towards
    private int patrolIndex = 0;

    // array of posisitions used for patrol, tank moves in square if no resources are found.
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
       

        Debug.Log("Entering Scavenge (FSM + RBS)");

        // resets patrol index so movement starts consistently
        patrolIndex = 0;

        // allow movement
        tank.TankGo();

        return null;
    }

    
    public override Type StateUpdate()
    {
       // checks resources 
        tank.CheckLowHealth();
        tank.CheckLowFuel();
        tank.CheckLowAmmo();

        // checks if the tank has reached the safe zone
        tank.CheckSafeZoneReached(Vector3.zero, 15f);

        bool resourcesLow =
            tank.stats["lowHealth"] ||
            tank.stats["lowFuel"] ||
            tank.stats["lowAmmo"];

        
        tank.stats["resourcesLow"] = resourcesLow;

        
        // RULE-BASED SYSTEM 
        
        // loop through every rule in rule system
        foreach (Rule rule in tank.rules.GetRules)
        {   
            Type ruleResult = rule.CheckRule(tank.stats); // asks the rule if it should trigger a state change
            if (ruleResult != null)
            {
                return ruleResult;
            }
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

        
        // square patrol behaviour 


        Vector3 target = patrolPoints[patrolIndex]; // selects current patrol target

        
        GameObject point = tank.CreateWorldPoint(target); //creates temporary world point to location
        tank.FollowPathToWorldPoint(point, SCAVENGE_SPEED, tank.heuristicMode);

        if (Vector3.Distance(tank.transform.position, target) < 6f) // if the tank is close enough to the point, it moves to the next one
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length; // increments the patrol index and loop back if needed.
        }

        return null;
    }

    
    public override Type StateExit()
    {
     
        Debug.Log("Exiting Scavenge");
         // resets speed and turret so its not on 0.6f anymore.
        tank.TurretReset();
        tank.TankGo();

        return null;
    }
}
