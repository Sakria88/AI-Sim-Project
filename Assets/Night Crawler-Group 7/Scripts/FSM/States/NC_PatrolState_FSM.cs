using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// NightCrawler PATROL STATE (FSM ONLY)
/// This version removes all Rule-Based System logic
/// and follows ONLY the pure FSM transition table.
/// </summary>
public class NC_PatrolState : TankState
{
    private NC_SmartTank_FSM tank;
    private Vector3 patrolTarget;
    private float patrolRadius = 25f;

    public NC_PatrolState(NC_SmartTank_FSM tankRef)
    {
        tank = tankRef;
    }

    public override void Enter()
    {
        // *I log when switching into Patrol so debugging becomes easier later*
        tank.DebugMessage("ENTERING PATROL (FSM ONLY)");

        // *I select a patrol point so I can wander the map*
        SetNewPatrolPoint();
    }

    public override void Update()
    {
        // ======================================================
        //  🔹 PATROL MOVEMENT
        // ======================================================
        tank.MoveTowards(patrolTarget);

        // *If I reach the patrol point, I pick a new one*
        if (Vector3.Distance(tank.transform.position, patrolTarget) < 2.5f)
        {
            SetNewPatrolPoint();
        }

        // ======================================================
        //  🔹 FSM TRANSITION CHECK (NO RBS)
        //  From the FSM table:
        //  Patrol → Pursue when TargetDistance > 52
        // ======================================================
        float targetDistance = tank.GetDistanceToEnemy();

        // *I check the one condition that triggers a state change*
        if (targetDistance > 52f)
        {
            tank.DebugMessage("FSM: Target distance > 52 → PURSUE");
            tank.ChangeState(new NC_PursueState(tank));
            return;
        }

        // (No other transitions exist in FSM table for Patrol)
    }

    public override void Exit()
    {
        // *I log that I am leaving the state*
        tank.DebugMessage("EXITING PATROL");
    }

    // ======================================================
    // Helper: pick a random patrol point
    // ======================================================
    private void SetNewPatrolPoint()
    {
        // *I wander by selecting a random point in a radius*
        Vector3 randomDir = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDir.y = 0;
        patrolTarget = tank.transform.position + randomDir;
    }
}
