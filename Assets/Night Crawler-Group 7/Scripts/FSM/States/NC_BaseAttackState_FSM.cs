using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_BaseAttackState_FSM : NC_BaseState_FSM
{
    private NC_SmartTank_FSM tank;

    //  smooth my turret turning so the aim feels more natural
    private Vector3 velocityTurretRot;

    //  keep a simple timer system so I can control burst firing or timed transitions
    private float fireTimer;
    private float fireDuration = 1.2f;   //  how long the firing burst should last

    // this is the minimum distance the tank should  be from the enemy base before firing properly
    private float preferredAttackDistance = 30f;

    public NC_BaseAttackState_FSM(NC_SmartTank_FSM tankRef)
    {
        tank = tankRef;
    }

    // -------------------------------------------------
    // ENTER STATE
    // -------------------------------------------------
    public override Type StateEnter()
    {
        Debug.Log("ENTERING BASE ATTACK");

        // I reset my firing timer whenever I start this state
        fireTimer = 0f;

        return null;
    }

    // -------------------------------------------------
    // UPDATE STATE — runs every frame
    //-------------------------------------------------
    public override Type StateUpdate()
    {
        // first I make sure that I still have a valid enemy base reference
        if (tank.NCEnBase == null)
        {
            // if I lose the base target, I fallback to base defend or search logic later
            Debug.Log("Enemy base lost → switching to BaseDefend");
            return typeof(NC_BaseDefendState_FSM);
        }

        // I calculate how far I am from the enemy base so I know if I should move or fire
        float distanceToBase = Vector3.Distance(tank.transform.position, tank.NCEnBase.transform.position);

        // ------------------------------------------------------
        // MOVING TOWARD THE BASE (Ares-like pathing logic)
        // ------------------------------------------------------
        if (distanceToBase > preferredAttackDistance)
        {
            // if I'm too far, I move closer using the same method Ares does
            tank.FollowPathToPoint(tank.NCEnBase, 1f, tank.heuristicMode);
        }

        // ------------------------------------------------------
        // TURRET ROTATION (my smoothed version)
        // ------------------------------------------------------
        Vector3 targetPos = tank.NCEnBase.transform.position;
        Vector3 dir = (targetPos - tank.transform.position).normalized;
        Vector3 smoothDir = Vector3.SmoothDamp(tank.turret.forward, dir, ref velocityTurretRot, 0.15f);

        // I rotate the turret so it always aims at the base
        tank.TurretFaceWorldPoint(tank.NCEnBase);

        // ------------------------------------------------------
        // FIRING LOGIC (inspired by Ares' attack cycle)
        // ------------------------------------------------------
        fireTimer += Time.deltaTime;

        // as long as I'm within firing distance I shoot at the base
        if (distanceToBase <= preferredAttackDistance)
        {
            tank.FireAtPoint(tank.NCEnBase);
        }

        // I stop my firing cycle after the timer expires so I can switch state or reposition
        if (fireTimer >= fireDuration)
        {
            // after this burst I switch to my BaseDefend state
            return typeof(NC_BaseDefendState_FSM);
        }

        // ------------------------------------------------------
        // SAFETY BEHAVIOUR — If my health is too low I bail out
        // ------------------------------------------------------
        if (tank.GetHealthLevel() <= 12f)
        {
            // I should not stay near the enemy base if I'm almost destroyed
            Debug.Log("Health too low → Retreat");
            return typeof(NC_RetreatState_FSM);
        }

        return null; // stay in base attack
    }

    //-------------------------------------------------
    // EXIT STATE
    // -------------------------------------------------
    public override Type StateExit()
    {
        Debug.Log("EXITING BASE ATTACK");
        return null;
    }
}
