using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class defines the Retreat state for the Night Crawler tank's finite state machine (FSM) 
/// combined with a Rule-Based System (RBS).
/// </summary>
public class NC_RetreatState_FSMRBS : NC_BaseState_FSMRBS
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;

    // A single world point that represents the retreat destination.
    // IMPORTANT: This is now created ONLY ONCE when entering the state 
    // to avoid spawning multiple objects every frame.
    private GameObject retreatPoint;

    public NC_RetreatState_FSMRBS(NC_SmartTank_FSMRBS NCTank)
    {
        this.nC_SmartTank_FSMRBS = NCTank;
    }

    public override Type StateEnter()
    {
        nC_SmartTank_FSMRBS.stats["NC_RetreatState_FSMRBS"] = true;
        CreateRetreatPoint();

        Debug.Log("Entering the retreat state FSM");

        // ---------------------------------------------------------
        // When entering Retreat, select ONE retreat destination.
        // This prevents generating dozens of world points per frame
        // and ensures stable navigation.
        // ---------------------------------------------------------
        return null;
    }

    public override Type StateUpdate()
    {
        //Issue the tank is not finding retreat path and following it before exiting this state and it keeps fighting between attack and retreat state---since it has low health----
        nC_SmartTank_FSMRBS.UpdateGlobalStats();
        nC_SmartTank_FSMRBS.CheckSafeZoneReached(retreatPoint.transform.position, 10f);


        //Check the rules to see if there are any that need to be used
        foreach (Rule item in nC_SmartTank_FSMRBS.rules.GetRules)
        {
            Debug.Log("Checking Rules");
            if (item.CheckRule(nC_SmartTank_FSMRBS.stats) != null)

            {
                return item.CheckRule(nC_SmartTank_FSMRBS.stats);
            }
        }

        return null;
    }

    /// <summary>
    /// Calculates and creates a retreat destination based on the enemy tank's position.
    /// This is only executed ONCE when entering the state.
    /// </summary>
    private void CreateRetreatPoint()
    {
        /*
         * Retreat Logic:
         * ---------------------------------------------
         * - Identify which quadrant the enemy tank is in.
         * - Select a retreat point in the opposite quadrant.
         * - This ensures maximum distancing and safer escape routes.
         */

        Vector3 enemyPosition = nC_SmartTank_FSMRBS.NCEnTank.transform.position;
        Vector3 retreatPosition = Vector3.zero;

        if (enemyPosition.x >= 0 && enemyPosition.z >= 0)
        {
            // Enemy in Quadrant 1 → Retreat to Quadrant 3
            retreatPosition = new Vector3(RandomRange(-85, -10), 0, RandomRange(-85, -10));
        }
        else if (enemyPosition.x < 0 && enemyPosition.z >= 0)
        {
            // Enemy in Quadrant 2 → Retreat to Quadrant 4
            retreatPosition = new Vector3(RandomRange(10, 85), 0, RandomRange(-85, -10));
        }
        else if (enemyPosition.x < 0 && enemyPosition.z < 0)
        {
            // Enemy in Quadrant 3 → Retreat to Quadrant 1
            retreatPosition = new Vector3(RandomRange(10, 85), 0, RandomRange(10, 85));
        }
        else
        {
            // Enemy in Quadrant 4 → Retreat to Quadrant 2
            retreatPosition = new Vector3(RandomRange(-85, -10), 0, RandomRange(10, 85));
        }

        // ---------------------------------------------------------
        // Generate ONE world point for the retreat destination.
        // This avoids spawning hundreds of objects per frame.
        // ---------------------------------------------------------
        retreatPoint = nC_SmartTank_FSMRBS.CreateWorldPoint(retreatPosition);
    }

    /// <summary>
    /// Utility wrapper for cleaner Random.Range calls.
    /// </summary>
    private float RandomRange(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.FollowPathToWorldPoint(retreatPoint, 1f, nC_SmartTank_FSMRBS.heuristicMode);

        Debug.Log("Exiting the retreat state FSM");
        nC_SmartTank_FSMRBS.stats["NC_RetreatState_FSMRBS"] = true;
        return null;
    }
}