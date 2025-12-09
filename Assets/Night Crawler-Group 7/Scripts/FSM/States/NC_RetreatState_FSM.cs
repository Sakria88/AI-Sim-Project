using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_RetreatState_FSM: NC_BaseState_FSM
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSM tank;
    private NC_SmartTank_FSM NCEnTank;

    public NC_RetreatState_FSM(NC_SmartTank_FSM NCTank)
    {
        this.tank = NCTank;


    }


    public override Type StateEnter()
    {
        return null;
    }

  

    public override Type StateUpdate()
    {
        /* 
      Tank Logic

     in this state tank needs to get out of danger/FLEE
     --Generate random world path to follow to (FollowPathToRandomWorldPoint(float normalizedSpeed))
     --check if enemy is around --Create an instance of enemy class to refer to
     --If enemy is still around keep retreating --check around for enenmy
     --If no enemy is around change to scavange state


    if(NCEnTank > 0 && VisibleEnemyTank.First().key != null )//If enemy tank found is more than 0 and can see
      {
         GameObject NCEnTank = NCEnTank.enemyTanksFound.First().Key; // Get the first visible enemy
         if (NCEnTank != null)
             {
                 FollowPathToRandomWorldPoint(1f)

             }
         else 
             {
                 return typeof(NC_ScavangeState_FSM)

             }

      }


      */

        if (tank.NCEnTank != null)
        {
            float Distance = Vector3.Distance(tank.transform.position, tank.NCEnTank.transform.position);

            if (Distance <=52f) //If the distance between the tank and enemy is equal to and less than 52
            {
                //generate a random point in the world and go to it

                FindRetreat_Path();
            }
        }
        else
        {
            //FollowPathToWorldPoint(NCEnTank, 1f, heuristicMode);
            return typeof(NC_ScavengeState_FSM); //If not less thank 30 keep pursuing
        }

        return null;

    }

   public void FindRetreat_Path()
    {
        tank.FollowPathToRandomWorldPoint(1f, tank.heuristicMode);

    }

        
    

    public override Type StateExit()
    {
        return null;

    }
}