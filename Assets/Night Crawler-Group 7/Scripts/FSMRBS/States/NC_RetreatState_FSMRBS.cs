using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_RetreatState_FSMRBS : NC_BaseState_FSMRBS
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;
    private GameObject retreatPoint;

    public NC_RetreatState_FSMRBS(NC_SmartTank_FSMRBS NCTank)
    {
        this.nC_SmartTank_FSMRBS = NCTank;
    }

    public override Type StateEnter()
    {
        nC_SmartTank_FSMRBS.stats["NC_RetreatState_FSMRBS"] = true;

        Debug.Log("Entering the retreat state FSM");
        return null;
    }

    public override Type StateUpdate()
    {
        //Issue the tank is not finding retreat path and following it before exiting this state and it keeps fighting between attack and retreat state---since it has low health----
        Debug.Log("Checking Conditions");
        //if (nC_SmartTank_FSMRBS.NCEnTank != null)
        //{   //generate a random point in the world and go to it
        //    FindRetreat_Path();
        //}

        FindRetreat_Path();

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

    public void FindRetreat_Path()
    {


        // var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();

        Debug.Log("Finding Path");
        //nC_SmartTank_FSMRBS.CheckTargetSpotted();//Somthing wrinf here


        // Create 4 world quadrants (-90,-90 to 90,90)
        // Determine which quadrant the enemy tank is in
        // Move to a random position in the opposite quadrant
        Vector3 enemyPosition = nC_SmartTank_FSMRBS.NCEnTank.transform.position;  //ERROR ITS NOT GETTING THE POSITION
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

        // retreatPoint = nC_SmartTank_FSMRBS.CreateWorldPoint(retreatPosition);
        //nC_SmartTank_FSMRBS.FollowPathToWorldPoint(retreatPoint, 1f, nC_SmartTank_FSMRBS.heuristicMode);
    }

    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.FollowPathToWorldPoint(retreatPoint, 1f, nC_SmartTank_FSMRBS.heuristicMode);

        Debug.Log("Exiting the retreat state FSM");
        nC_SmartTank_FSMRBS.stats["NC_RetreatState_FSMRBS"] = true;
        return null;
    }
}