using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines the Base Attack state for the Night Crawler tank's finite state machine (FSM).
/// </summary>
public class NC_BaseAttackState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM tank;

    // firing control variables
    private float fireTimer;
    private float fireDuration = 1.2f;

    // count how many shots fired during this cycle
    private int shotsFired = 0;

    // how close tank should be before committing to firing
    private float preferredAttackDistance = 30f;

    public NC_BaseAttackState_FSM(NC_SmartTank_FSM tankRef)
    {
        tank = tankRef;
    }

    public override Type StateEnter()
    {
        Debug.Log("ENTERING BASE ATTACK");

        // reset timers and shot counter every time tank enters base attack
        fireTimer = 0f;
        shotsFired = 0;

        return null;
    }

    public override Type StateUpdate()
    {
        // -----------------------------------------------------------------
        // BASE IS GONE → RETURN TO PATROL (search for next base)
        // -----------------------------------------------------------------
        if (tank.NCEnBase == null)
        {
            Debug.Log("Base destroyed → returning to Patrol for next base");
            return typeof(NC_PatrolState_FSM);
        }

        // -----------------------------------------------------------------
        // AMMO == 0 → SCAVENGE (FSM rule)
        // -----------------------------------------------------------------
        if (tank.TankCurrentAmmo <= 0)
        {
            Debug.Log("Ammo depleted → Scavenge");
            return typeof(NC_ScavengeState_FSM);
        }

        // -----------------------------------------------------------------
        // measure distance to enemy base
        // -----------------------------------------------------------------
        float distanceToBase = Vector3.Distance(
            tank.transform.position,
            tank.NCEnBase.transform.position
        );

        // -----------------------------------------------------------------
        // MOVE INTO RANGE
        // -----------------------------------------------------------------
        if (distanceToBase > preferredAttackDistance)
        {
            tank.FollowPathToWorldPoint(
                tank.NCEnBase,
                1f,
                tank.heuristicMode
            );
        }

        // -----------------------------------------------------------------
        // TURRET CONTROL - always keep my aim locked on the base
        // -----------------------------------------------------------------
        tank.TurretFaceWorldPoint(tank.NCEnBase);

        // -----------------------------------------------------------------
        // FIRING LOGIC (burst-fire behaviour)
        // -----------------------------------------------------------------
        fireTimer += Time.deltaTime;

        // only fire when close enough to the base
        if (distanceToBase <= preferredAttackDistance)
        {
            tank.TurretFireAtPoint(tank.NCEnBase);
            shotsFired++; // count each shot so tank can follow FSM rules
        }

        // -----------------------------------------------------------------
        // NEW FSM RULE → SHOTS FIRED == 3 → PATROL
        // -----------------------------------------------------------------
        if (shotsFired >= 3)
        {
            Debug.Log("Fired 3 shots → returning to Patrol");
            return typeof(NC_PatrolState_FSM);
        }

        // -----------------------------------------------------------------
        // BURST FINISHED → BASE DEFEND (original behaviour)
        // -----------------------------------------------------------------
        if (fireTimer >= fireDuration)
        {
            Debug.Log("Burst complete → switching to BaseDefend");
            //return typeof(NC_BaseDefendState_FSM);
            return typeof(NC_PatrolState_FSM);

        }

        // -----------------------------------------------------------------
        // SAFETY → LOW HEALTH → RETREAT
        // -----------------------------------------------------------------
        if (tank.TankCurrentHealth <= 12f)
        {
            Debug.Log("Health low → Retreat");
            return typeof(NC_RetreatState_FSM);
        }

        return null; // stay in base attack
    }

    public override Type StateExit()
    {
        Debug.Log("EXITING BASE ATTACK");
        return null;
    }
}
