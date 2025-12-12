using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class defines the Retreat state for the Night Crawler tank's finite state machine (FSM).
/// Handles the logic for selecting a safe retreat location and navigating toward it.
/// </summary>
public class NC_RetreatState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    // A single world point that represents the retreat destination.
    // IMPORTANT: This is now created ONLY ONCE when entering the state 
    // to avoid spawning multiple objects every frame.
    private GameObject retreatPoint;

    public NC_RetreatState_FSM(NC_SmartTank_FSM NCTank)
    {
        this.nC_SmartTank_FSM = NCTank;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering the retreat state FSM");

        // ---------------------------------------------------------
        // When entering Retreat, select ONE retreat destination.
        // This prevents generating dozens of world points per frame
        // and ensures stable navigation.
        // ---------------------------------------------------------
        CreateRetreatPoint();

        return null;
    }

    public override Type StateUpdate()
    {
        /*
         * State Transition Logic:
         * ------------------------
         * - If the enemy tank is no longer detected → retreat no longer needed → Scavenge.
         * - Otherwise → continue navigating toward the previously selected retreat point.
         */

        if (nC_SmartTank_FSM.NCEnTank == null)
        {
            return typeof(NC_ScavengeState_FSM);
        }

        // ---------------------------------------------------------
        // Continue retreating to the same target position.
        // ---------------------------------------------------------
        nC_SmartTank_FSM.FollowPathToWorldPoint(
            retreatPoint,
            1f,
            nC_SmartTank_FSM.heuristicMode
        );

        return null; // remain in retreat
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

        Vector3 enemyPosition = nC_SmartTank_FSM.NCEnTank.transform.position;
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
        retreatPoint = nC_SmartTank_FSM.CreateWorldPoint(retreatPosition);
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
        /*
         * When exiting the retreat state:
         * - Destroy the retreat point to prevent scene clutter.
         * - This is crucial because retreat points are created dynamically.
         * - Ensures clean memory usage and avoids leftover objects.
         */

        if (retreatPoint != null)
        {
            UnityEngine.Object.Destroy(retreatPoint);
        }

        Debug.Log("Exiting the retreat state FSM");
        return null;
    }
}
