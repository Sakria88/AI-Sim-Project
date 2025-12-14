using System;
using UnityEngine;

/// <summary>
/// Attack state using FSM + Rule-Based System
/// </summary>
public class NC_AttackState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS tank;

    // Threshold for close-range combat
    private const float CLOSE_RANGE = 30f;

    public NC_AttackState_FSMRBS(NC_SmartTank_FSMRBS tank)
    {
        this.tank = tank;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering ATTACK state (FSM + RBS)");

        // Stop movement so the tank can aim accurately
        tank.TankStop();

        return null;
    }

    public override Type StateUpdate()
    {
        // UPDATE FACTS USED BY RULE SYSTEM
        tank.CheckEnemyInSight();
        tank.CheckEnemyNotDetected();

        tank.UpdateGlobalStats();

        tank.CheckEnemyDistanceClose();
        tank.CheckEnemyDistanceMid();
        tank.CheckEnemyDistanceFar();


        // RULE-BASED SYSTEM


        foreach (Rule rule in tank.rules.GetRules)
        {
            Type ruleResult = rule.CheckRule(tank.stats);
            if (ruleResult != null)
            {
                return ruleResult;
            }
        }

        // ATTACK BEHAVIOUR 

        GameObject target = tank.NCEnTank;

        if (target == null)
            return null;

        // Aim turret at enemy
        tank.TurretFaceWorldPoint(target);

        // Fire at enemy
        tank.TurretFireAtPoint(target);

        // Stay in Attack
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting ATTACK state");

        // Reset turret when leaving attack
        tank.TurretReset();

        return null;
    }
}