using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_ScavengeState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM tank; 

    public NC_ScavengeState_FSM(NC_SmartTank_FSM tank)
    {
        this.tank = tank;
    }

    public override Type StateEnter()
    {
        // makes sure tank starts moving normally, good practice for reducing bugs
        tank.TankGo();

        return null;
    }

    public override Type StateUpdate()
    {
        // if health is no longer low, return to Patrol
        if (tank.TankCurrentHealth >= 30f)
        {
            return typeof(NC_PatrolState_FSM);
        }

        // scavenge logic
        Dictionary<GameObject, float> visibleCons = tank.VisibleConsumables;

        GameObject bestTarget = null;
        float closestDist = Mathf.Infinity; // Finds closest consumable to pick up

        // prioritises health then either fuel or ammo
        foreach (var item in visibleCons)
        {
            GameObject cons = item.Key;

            if (cons == null) continue; // skip consumable if no longer there

            string tag = cons.tag; // checks the consumbable tag so we know to pick up health first

            // priority is health
            if (tag == "Health")
            {
                float dist = item.Value;
                if (dist < closestDist) // checks if this health pickup is closer than the currently stored closest distance.
                {
                    closestDist = dist; // updates the closest distance to this new smaller value.
                    bestTarget = cons; // saves this consumable as the current best target.
                }
            }
        }

        // if no health, then looks for fuel or ammo
        if (bestTarget == null)
        {
            foreach (var item in visibleCons)
            {
                GameObject cons = item.Key;
                if (cons == null) continue;

                string tag = cons.tag;

                if (tag == "Fuel" || tag == "Ammo")
                {
                    float dist = item.Value;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestTarget = cons;
                    }
                }
            }
        }

        // finds best consumable first
        if (bestTarget != null)
        {
            tank.FollowPathToWorldPoint(bestTarget, 1f, tank.heuristicMode); // sends tank to bestTarget (best consumable) using heuristic mode. 1f = full speed. 
            tank.TurretReset(); // prevents turret from moving when scavenging
        }
        else
        {
            // if no consumbales are found then wander until consumbale are found
            tank.FollowPathToRandomWorldPoint(1f, tank.heuristicMode);
            tank.TurretReset();
        }

        return null; // stays in Scavenge state
    }

    public override Type StateExit()
    {
        // reset turret and allow tank to move normally again, prevents bugs for next state
        tank.TurretReset();
        tank.TankGo();
        return null;
    }
}
