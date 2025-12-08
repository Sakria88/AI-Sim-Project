using using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
;

/// <summary>
/// NightCrawler PATROL STATE
/// Handles wandering, scanning, rule evaluation, and FSM transitions.
/// This version is updated according to my full transition table.
/// </summary>
public class NC_PatrolState_FSM : NC_BaseScript_FSM
{
    private NC_SmartTank_FSM tank;
    private Vector3 patrolTarget;
    private float patrolRadius = 25f;

    // ===== RBS FACTS (based on my transition table) =====
    private bool fact_enemyInRange = false;
    private bool fact_canSeeEnemy = false;
    private bool fact_enemyApproaching = false;
    private bool fact_enemyBaseDetected = false;

    public NC_PatrolState(NC_SmartTank_FSM tankRef)
    {
        tank = tankRef;
    }

    public override void Enter()
    {
        // *I log my state entry to help future debugging*
        tank.DebugMessage("ENTERING PATROL");

        // *I pick a random patrol location*
        SetNewPatrolPoint();
    }

    public override void Update()
    {
        // ======================================================
        //  🔹 PATROL MOVEMENT
        // ======================================================
        tank.MoveTowards(patrolTarget);

        if (Vector3.Distance(tank.transform.position, patrolTarget) < 2.5f)
        {
            SetNewPatrolPoint();
        }

        // ======================================================
        //  🔹 UPDATE RBS FACTS (from my transition table)
        // ======================================================
        fact_enemyInRange = tank.EnemyWithinRange();
        fact_canSeeEnemy = tank.CanSeeEnemy();
        fact_enemyApproaching = tank.EnemyApproaching();
        fact_enemyBaseDetected = tank.EnemyBaseNearby();

        // ======================================================
        //  🔹 APPLY RBS RULES (these match my transition table EXACTLY)
        // ======================================================


        // RULE A (Patrol row 1):
        // Enemy within range & enemy CAN'T see me → PURSUE
        if (fact_enemyInRange && !fact_canSeeEnemy)
        {
            tank.DebugMessage("RULE A: Enemy in range but cannot see me → PURSUE");
            tank.ChangeState(new NC_PursueState(tank));
            return;
        }

        // RULE B (Patrol row 2):
        // Enemy within range & CAN see me (enemy coming closer) → WAIT
        if (fact_enemyInRange && fact_canSeeEnemy && fact_enemyApproaching)
        {
            tank.DebugMessage("RULE B: Enemy sees me & approaching → WAIT");
            tank.ChangeState(new NC_WaitState(tank));
            return;
        }

        // RULE C (Patrol row 3):
        // Enemy NOT in range & enemy base detected → BASE ATTACK
        if (!fact_enemyInRange && fact_enemyBaseDetected)
        {
            tank.DebugMessage("RULE C: No enemy nearby but enemy base detected → BASE ATTACK");
            tank.ChangeState(new NC_BaseAttackState(tank));
            return;
        }

        // ======================================================
        //  🔹 (Optional) Behaviour Tree Hook
        //  So later I can plug this state into a BT node easily.
        // ======================================================
        // Example: tank.BT_ReportState("Patrol");
    }

    public override void Exit()
    {
        // *I notify when leaving the state to help testing*
        tank.DebugMessage("EXITING PATROL");
    }

    // ======================================================
    // Helper: Generates new wander point
    // ======================================================
    private void SetNewPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir.y = 0;
        patrolTarget = tank.transform.position + randomDir;
    }
}
