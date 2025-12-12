using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_PatrolState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM tank;
    private float exploreTimer = 0f;
    private float exploreInterval = 2.5f;

    public 
        NC_PatrolState_FSM(NC_SmartTank_FSM tankRef)
    {
        tank = tankRef;
    }

    public override Type StateEnter()
    {
        Debug.Log("ENTERING PATROL (GLOBAL SEARCH MODE)");
        exploreTimer = 0f;
        return null;
    }

    public override Type StateUpdate()
    {
        exploreTimer += Time.deltaTime;

        // ------------------------------------------------
        // HEALTH | FUEL CHECK → SCAVENGE
        // ------------------------------------------------
        if (tank.TankCurrentHealth < 35f || tank.TankCurrentFuel < 35f)
        {
            return typeof(NC_ScavengeState_FSM);
        }

        // ------------------------------------------------
        // CONSUMABLE WITHIN 52 → SCAVENGE
        // ------------------------------------------------
        if (tank.VisibleEnemyTanks.Count > 0)
       {
        float closestTankDist = float.MaxValue;
        GameObject closestTank = null;

        foreach (var entry in tank.VisibleEnemyTanks)
         {
          if (entry.Value < closestTankDist)
          {
            closestTankDist = entry.Value;
            closestTank = entry.Key;
          }
       }

    if (closestTankDist < 52f)
    {
        tank.NCEnTank = closestTank;
        return typeof(NC_PursueState_FSM);
    }
  }

        // ------------------------------------------------
        // ENEMY BASE WITHIN 52 → BASE ATTACK
        // ------------------------------------------------
        if (tank.VisibleEnemyBases.Count > 0)
        {
            float closest = float.MaxValue;
            GameObject baseObj = null;

            foreach (var entry in tank.VisibleEnemyBases)
            {
                if (entry.Value < closest)
                {
                    closest = entry.Value;
                    baseObj = entry.Key;
                }
            }

            if (closest < 52f)
            {
                tank.NCEnBase = baseObj;
                return typeof(NC_BaseAttackState_FSM);
            }
        }

        // ---------------------------------------
        // ENEMY TANK WITHIN 52 → PURSUE
        // ---------------------------------------
        if (tank.VisibleEnemyTanks.Count > 0)
        {
            float closestT = float.MaxValue;
            GameObject enemy = null;

            foreach (var entry in tank.VisibleEnemyTanks)
            {
                if (entry.Value < closestT)
                {
                    closestT = entry.Value;
                    enemy = entry.Key;
                }
            }

            if (closestT < 52f)
            {
                tank.NCEnTank = enemy;
                return typeof(NC_PursueState_FSM);
            }
        }

        // ------------------------------------------------
        // CONTINUOUS MOVEMENT ACROSS THE WHOLE MAP
        // ------------------------------------------------
        tank.FollowPathToRandomWorldPoint(1f, tank.heuristicMode);

        if (exploreTimer >= exploreInterval)
        {
            tank.FollowPathToRandomWorldPoint(1f, tank.heuristicMode);
            exploreTimer = 0f;
        }

        return null; // stay in patrol
    }

    public override Type StateExit()
    {
        Debug.Log("EXITING PATROL");
        return null;
    }
}
