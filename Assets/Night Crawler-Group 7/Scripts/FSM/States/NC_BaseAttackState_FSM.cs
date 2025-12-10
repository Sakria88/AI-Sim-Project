using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NC_BaseAttackState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM nC_SmartTank_FSM;

    //use this timer to control burst firing intervals
    private float fireTimer;
    private float fireDuration = 1.2f;

    // this is how close tank should be before committing to full base fire
    private float preferredAttackDistance = 30f;

    public NC_BaseAttackState_FSM(NC_SmartTank_FSM tankRef)
    {
        nC_SmartTank_FSM = tankRef;
    }

    public override Type StateEnter()
    {
        Debug.Log("ENTERING BASE ATTACK");
        fireTimer = 0f;
        return null;
    }

    public override Type StateUpdate()
    {
        // if tank loses the enemy base reference switch to BaseDefend so not idle
        if (nC_SmartTank_FSM.NCEnBase == null)
        {
            Debug.Log("Enemy base lost → switching to BaseDefend");
            return typeof(NC_BaseDefendState_FSM);
        }

        // measure distance to the target base
        float distanceToBase = Vector3.Distance(
            nC_SmartTank_FSM.transform.position,
            nC_SmartTank_FSM.NCEnBase.transform.position
        );

        // ------------------------------------------------------
        // MOVEMENT TOWARD BASE
        // ------------------------------------------------------
        if (distanceToBase > preferredAttackDistance)
        {
            //move toward the base using the pathing function exists in SmartTank
            nC_SmartTank_FSM.FollowPathToWorldPoint(nC_SmartTank_FSM.NCEnBase, 1f, nC_SmartTank_FSM.heuristicMode);
        }

        // ------------------------------------------------------
        // TURRET CONTROL
        // ------------------------------------------------------
        //face the turret directly toward the base target
        nC_SmartTank_FSM.TurretFaceWorldPoint(nC_SmartTank_FSM.NCEnBase);

        // ------------------------------------------------------
        // FIRING LOGIC
        // ------------------------------------------------------
        fireTimer += Time.deltaTime;

        if (distanceToBase <= preferredAttackDistance)
        {
            // use the firing function that  in  SmartTank
            nC_SmartTank_FSM.TurretFireAtPoint(nC_SmartTank_FSM.NCEnBase);
        }

        // once tank timed burst is over switch to BaseDefend
        if (fireTimer >= fireDuration)
        {
            return typeof(NC_BaseDefendState_FSM);
        }

        // ------------------------------------------------------
        // HEALTH CHECK SAFETY
        // ------------------------------------------------------
        if (nC_SmartTank_FSM.TankCurrentHealth <= 12f)
        {
            Debug.Log("Health low → Retreat");
            return typeof(NC_RetreatState_FSM);
        }

        return null; // remain in BaseAttack
    }

    public override Type StateExit()
    {
        Debug.Log("EXITING BASE ATTACK");
        return null;
    }
}
