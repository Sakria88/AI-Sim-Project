using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_RetreatState_FSM: NC_BaseState_FSM
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSM nC_SmartTank_FSM;
    private GameObject retreatPoint;

    public 
        NC_RetreatState_FSM(NC_SmartTank_FSM NCTank)
    {
        this.nC_SmartTank_FSM = NCTank;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering the retreat state FSM");
        return null;
    }

    public override Type StateUpdate()
    {        
        if (nC_SmartTank_FSM.NCEnTank != null )
        {   //generate a random point in the world and go to it
                FindRetreat_Path();
        }
        else
        {
            return typeof(NC_ScavengeState_FSM); //If not Switch state
        }

        return null;

    }

   public void FindRetreat_Path()
    {
        // Create 4 world quadrants (-90,-90 to 90,90)
        // Determine which quadrant the enemy tank is in
        // Move to a random position in the opposite quadrant
        Vector3 enemyPosition = nC_SmartTank_FSM.NCEnTank.transform.position;
        Vector3 retreatPosition = Vector3.zero;
        if (enemyPosition.x >= 0 && enemyPosition.z >= 0)
        {
            // Enemy in Quadrant 1, retreat to Quadrant 3
            retreatPosition = new Vector3(UnityEngine.Random.Range(-85, -10), 0, UnityEngine.Random.Range(-85, -10));
        }
        else if (enemyPosition.x < 0 && enemyPosition.z >= 0)
        {
            // Enemy in Quadrant 2, retreat to Quadrant 4
            retreatPosition = new Vector3(UnityEngine.Random.Range(10, 85), 0, UnityEngine.Random.Range(-85, -10));
        }
        else if (enemyPosition.x < 0 && enemyPosition.z < 0)
        {
            // Enemy in Quadrant 3, retreat to Quadrant 1
            retreatPosition = new Vector3(UnityEngine.Random.Range(10, 85), 0, UnityEngine.Random.Range(10, 85));
        }
        else if (enemyPosition.x >= 0 && enemyPosition.z < 0)
        {
            // Enemy in Quadrant 4, retreat to Quadrant 2
            retreatPosition = new Vector3(UnityEngine.Random.Range(-85, -10), 0, UnityEngine.Random.Range(10, 85));
        }
        retreatPoint = nC_SmartTank_FSM.CreateWorldPoint(retreatPosition);
        nC_SmartTank_FSM.FollowPathToWorldPoint(retreatPoint, 1f, nC_SmartTank_FSM.heuristicMode);

    }

    public override Type StateExit()
    {
        nC_SmartTank_FSM.FollowPathToWorldPoint(retreatPoint, 1f, nC_SmartTank_FSM.heuristicMode);
        Debug.Log("Exiting the retreat state FSM");
        return null;
    }
}