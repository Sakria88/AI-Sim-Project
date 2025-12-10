using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// NightCrawler PATROL STATE (FSM ONLY)
/// following FSM transition table.
/// Where do I want NCEnTank to come from?
/// Option A — use the nearest enemy tank from: tank.VisibleEnemyTanks (This is a Dictionary<GameObject,float>) 
/// Option B — use the enemyTank variable already included in NC_SmartTank_FSM: public GameObject enemyTank; 
/// Option C — choose whichever enemy tank is closest 
/// </summary>
public class NC_PatrolState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;
    private Vector3 patrolTarget;
    private float patrolRadius = 25f;

    public NC_PatrolState_FSM(NC_SmartTank_FSM tankRef)
    {
        nC_SmartTank_FSM = tankRef;
    }

    // -------------------------------------------------
    //  STATE ENTER
    // -------------------------------------------------
    public override Type StateEnter()
    {
        Debug.Log("ENTERING PATROL (FSM ONLY)");

        // I generate a new patrol point when entering this state
        SetNewPatrolPoint();

        return null;
    }


    //  -------------------------------------------------
    //  STATE UPDATE
    //  -------------------------------------------------
    public override Type StateUpdate()
    {
        if (nC_SmartTank_FSM.NCEnTank != null)
        {
            if (Vector3.Distance(nC_SmartTank_FSM.transform.position,
                nC_SmartTank_FSM.NCEnTank.transform.position) < 52f) // If the target is within 3 units chase it
            {
                return typeof(NC_PursueState_FSM);
            }
        }
        else // else continue roaming
        {
            nC_SmartTank_FSM.RandomRoam();
            return null;
        }
        return null;
        //// First, I select an enemy tank (A/B/C logic)
        //SelectEnemyTank();

        //// If an enemy exists, I check distance for FSM transition
        //if (tank.NCEnTank != null)
        //{
        //    float distanceToEnemy = Vector3.Distance(
        //        tank.transform.position,
        //        tank.NCEnTank.transform.position);

        //    // From my FSM table: Patrol → Pursue when distance > 52
        //    if (distanceToEnemy < 52f)
        //    {
        //        Debug.Log("FSM: Target distance > 52 → PURSUE");
        //        return typeof(NC_PursueState_FSM);
        //    }
        //}

        //// TODO: Add movement using FollowPathToWorldPoint if needed

        //return null; // stay in Patrol
    }


    // -------------------------------------------------
    //  STATE EXIT
    // -------------------------------------------------
    public override Type StateExit()
    {
        Debug.Log("EXITING PATROL");
        return null;
    }


    //  -------------------------------------------------
    //  HELPER — Generate new random patrol point
    //  -------------------------------------------------
    private void SetNewPatrolPoint()
    {
        Vector3 randomDir = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDir.y = 0;
        patrolTarget = nC_SmartTank_FSM.transform.position + randomDir;
    }


    //  -------------------------------------------------
    //  HELPER — Create a temporary pathing point for A*
    //  -------------------------------------------------
    private GameObject CreateTempPoint(Vector3 position)
    {
        GameObject temp = new GameObject("PatrolPoint");
        temp.transform.position = position;
        return temp;
    }


    // -------------------------------------------------
    //  ENEMY SELECTION (A, B, and C)
    // -------------------------------------------------
    private void SelectEnemyTank()
    {
        // OPTION A + C — Choose nearest visible enemy from sensor dictionary
        Dictionary<GameObject, float> visibleEnemies = nC_SmartTank_FSM.VisibleEnemyTanks;

        if (visibleEnemies.Count > 0)
        {
            float closestDistance = float.MaxValue;
            GameObject closestEnemy = null;

            foreach (var entry in visibleEnemies)
            {
                if (entry.Value < closestDistance)
                {
                    closestEnemy = entry.Key;
                    closestDistance = entry.Value;
                }
            }

            nC_SmartTank_FSM.NCEnTank = closestEnemy;
            return;
        }

        // OPTION B — Use existing enemy stored in SmartTank
        if (nC_SmartTank_FSM.NCEnTank != null)
        {
            return;
        }

       
    }
}

