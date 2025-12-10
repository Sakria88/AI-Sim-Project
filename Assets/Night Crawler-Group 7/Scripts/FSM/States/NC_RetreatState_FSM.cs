using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_RetreatState_FSM: NC_BaseState_FSM
{
    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    public NC_RetreatState_FSM(NC_SmartTank_FSM NCTank)
    {
        this.nC_SmartTank_FSM = NCTank;
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
        // Generate Point away from enemy

        nC_SmartTank_FSM.FollowPathToWorldPoint(nC_SmartTank_FSM.NCEnTank, 1f, nC_SmartTank_FSM.heuristicMode);

        nC_SmartTank_FSM.GenerateNewRandomWorldPoint();
        nC_SmartTank_FSM.FollowPathToRandomWorldPoint(1f, nC_SmartTank_FSM.heuristicMode);

    }

        
    

    public override Type StateExit()
    {

        return null;

    }
}