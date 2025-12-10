using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// NightCrawler PATROL STATE (FSM ONLY)
/// following FSM transition table.
/// </summary>
public class NC_PatrolState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM tank;
    private float exploreTimer= 0;

    private float exploreInterval = 2.5f; // how often I choose a new random map point

    public NC_PatrolState_FSM(NC_SmartTank_FSM tankRef)
    {
        tank = tankRef;
    }

    public override Type StateEnter()
    {
        Debug.Log("ENTERING PATROL (GLOBAL SEARCH MODE)");

        //reset exploration timer
        exploreTimer = 0f;

        return null;
    }

    public override Type StateUpdate()
{
    // I increase my timer so I know when to pick a new random destination
    exploreTimer += Time.deltaTime;

    // --------------------------------------------
    // 1. HIGH PRIORITY → CHECK FOR CONSUMABLES FIRST
    // --------------------------------------------
    if (tank.VisibleConsumables.Count > 0)
    {
        return typeof(NC_ScavengeState_FSM);
    }

    // --------------------------------------------
    // 2. SECOND PRIORITY → CHECK FOR ENEMY BASES
    // --------------------------------------------
    if (tank.VisibleEnemyBases.Count > 0)
    {
        float closest = float.MaxValue;
        GameObject closestBase = null;

        foreach (var entry in tank.VisibleEnemyBases)
        {
            if (entry.Value < closest)
            {
                closest = entry.Value;
                closestBase = entry.Key;
            }
        }

        // I store this base so my next state knows the correct target
        tank.NCEnBase = closestBase;

        return typeof(NC_BaseAttackState_FSM);
    }

    // --------------------------------------------
    // 3. THIRD PRIORITY → CHECK FOR ENEMY TANKS
    // --------------------------------------------
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

        tank.NCEnTank = closestTank;
        return typeof(NC_PursueState_FSM);
    }

    // --------------------------------------------
    // 4. CONTINUOUS MOVEMENT ACROSS THE MAP
    // --------------------------------------------
    // I force the tank to always move along its current path
    tank.FollowPathToRandomWorldPoint(1f, tank.heuristicMode);

    // Every few seconds, I choose a new random point to explore
    if (exploreTimer >= exploreInterval)
    {
        tank.FollowPathToRandomWorldPoint(1f, tank.heuristicMode);
        exploreTimer = 0f;
    }

    // stay in Patrol
    return null;
}
public override Type StateExit()
{
    Debug.Log("EXITING PATROL");
    return null;
}

}
