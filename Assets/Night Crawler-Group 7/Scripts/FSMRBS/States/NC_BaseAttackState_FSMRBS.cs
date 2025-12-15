using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines the Base Attack state for the Night Crawler tank's finite state machine (FSM)
/// </summary>
public class NC_BaseAttackState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;

    // firing control
    private float fireTimer;
    private float fireDuration = 1.2f;

    // count how many shots fired during this cycle
    private int shotsFired = 0;

    // how close tank should be before committing to firing
    private float preferredAttackDistance = 30f;

    public NC_BaseAttackState_FSMRBS(NC_SmartTank_FSMRBS tankRef)
    {
        nC_SmartTank_FSMRBS = tankRef;
    }

    public override Type StateEnter()
    {
        Debug.Log("ENTERING BASE ATTACK");
        nC_SmartTank_FSMRBS.stats["NC_BaseAttackState_FSMRBS"] = true;
        // reset timers and shot counter every time tank enter base attack
        fireTimer = 0f;
        shotsFired = 0;

        return null;
    }

    public override Type StateUpdate()
    {
        nC_SmartTank_FSMRBS.UpdateGlobalStats();
        nC_SmartTank_FSMRBS.CheckEnemyBaseDestroyed(); // update fact about enemy base status

        // -----------------------------------------------------------------
        // BASE IS GONE → RETURN TO PATROL (search for next base)
        // -----------------------------------------------------------------
        if (nC_SmartTank_FSMRBS.NCEnBase == null)
        {
            Debug.Log("Base destroyed → returning to Patrol for next base");
            return typeof(NC_PatrolState_FSMRBS);
        }

        // -----------------------------------------------------------------
        // measure distance to enemy base
        // -----------------------------------------------------------------
        float distanceToBase = Vector3.Distance(
            nC_SmartTank_FSMRBS.transform.position,
            nC_SmartTank_FSMRBS.NCEnBase.transform.position
        );

        // -----------------------------------------------------------------
        // MOVE INTO RANGE
        // -----------------------------------------------------------------
        if (distanceToBase > preferredAttackDistance)
        {
            nC_SmartTank_FSMRBS.FollowPathToWorldPoint(
                nC_SmartTank_FSMRBS.NCEnBase,
                1f,
                nC_SmartTank_FSMRBS.heuristicMode
            );
        }

        // -----------------------------------------------------------------
        // TURRET CONTROL - always keep my aim locked on the base
        // -----------------------------------------------------------------
        nC_SmartTank_FSMRBS.TurretFaceWorldPoint(nC_SmartTank_FSMRBS.NCEnBase);

        // -----------------------------------------------------------------
        // FIRING LOGIC (burst-fire behaviour)
        // -----------------------------------------------------------------
        fireTimer += Time.deltaTime;

        // only fire when close enough to the base
        if (distanceToBase <= preferredAttackDistance)
        {
            nC_SmartTank_FSMRBS.TurretFireAtPoint(nC_SmartTank_FSMRBS.NCEnBase);
            shotsFired++; // count each shot so tank can follow FSM rules
        }

        // -----------------------------------------------------------------
        // NEW FSM RULE → SHOTS FIRED == 3 → PATROL
        // -----------------------------------------------------------------
        if (shotsFired >= 3)
        {
            Debug.Log("Fired 3 shots → returning to Patrol");
            nC_SmartTank_FSMRBS.stats["ShotsFired"] = true;
        }

        return null; // stay in base attack
    }

    public override Type StateExit()
    {
        nC_SmartTank_FSMRBS.stats["NC_BaseAttackState_FSMRBS"] = false;
        nC_SmartTank_FSMRBS.stats["ShotsFired"] = false; // reset for next time
        Debug.Log("EXITING BASE ATTACK");
        return null;
    }
}
