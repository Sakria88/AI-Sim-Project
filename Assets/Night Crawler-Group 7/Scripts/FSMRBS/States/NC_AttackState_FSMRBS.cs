using System;
using UnityEngine;

/// <summary>
/// Attack state using FSM + Rule-Based System (RBS) for the Night Crawler tank.
/// </summary>
public class NC_AttackState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmarTank_FSMRBS;

    // Threshold for close-range combat
    private const float CLOSE_RANGE = 30f;

    public NC_AttackState_FSMRBS(NC_SmartTank_FSMRBS tank)
    {
        this.nC_SmarTank_FSMRBS = tank;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering ATTACK state (FSM + RBS)");
        nC_SmarTank_FSMRBS.stats["NC_AttackState_FSMRBS"] = true;

        // Stop movement so the tank can aim accurately
        nC_SmarTank_FSMRBS.TankStop();

        return null;
    }

    public override Type StateUpdate()
    {
        // UPDATE FACTS USED BY RULE SYSTEM
        nC_SmarTank_FSMRBS.UpdateGlobalStats();

        //----------------------------------------
        // ATTACK BEHAVIOUR
        //----------------------------------------
        if (nC_SmarTank_FSMRBS.NCEnTank != null)
        {
            float distanceToEnemy = Vector3.Distance(
                nC_SmarTank_FSMRBS.transform.position,
                nC_SmarTank_FSMRBS.NCEnTank.transform.position
            );
            if ( distanceToEnemy <= CLOSE_RANGE) // If enemy is within close range
            {
                // Aim turret at enemy
                nC_SmarTank_FSMRBS.TurretFaceWorldPoint(nC_SmarTank_FSMRBS.NCEnTank);

                // Fire at enemy
                nC_SmarTank_FSMRBS.TurretFireAtPoint(nC_SmarTank_FSMRBS.NCEnTank);
            }
        }
        else
        {
            return typeof(NC_PursueState_FSMRBS);
        }

        foreach (Rule rule in nC_SmarTank_FSMRBS.rules.GetRules)
        {
            Type ruleResult = rule.CheckRule(nC_SmarTank_FSMRBS.stats);
            if (ruleResult != null)
            {
                return ruleResult;
            }
        }

        // Stay in Attack
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting ATTACK state");
        nC_SmarTank_FSMRBS.stats["NC_AttackState_FSMRBS"] = false;

        // Reset turret when leaving attack
        nC_SmarTank_FSMRBS.TurretReset();
        nC_SmarTank_FSMRBS.TankGo();

        return null;
    }
}