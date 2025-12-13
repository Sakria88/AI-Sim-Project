using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>NC_AttackState_FSMRBS</c> inherits from <c>NC_BaseState_FSMRBS</c>, manages behaviour for StateEnter, StateUpdate and StateExit
/// </summary>
public class NC_AttackState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;
    private const float closeRange = 30f;   // rule threshold

    public NC_AttackState_FSMRBS(NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS)
    {
        this.nC_SmartTank_FSMRBS = nC_SmartTank_FSMRBS;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering the attack state FSMRBS");
        return null;
    }

    public override Type StateUpdate()
    {
        // Safety checks first
        if (nC_SmartTank_FSMRBS.TankCurrentHealth < 35f)
            return typeof(NC_RetreatState_FSMRBS);

        if (nC_SmartTank_FSMRBS.TankCurrentFuel < 35f || nC_SmartTank_FSMRBS.TankCurrentAmmo == 0)
            return typeof(NC_ScavengeState_FSMRBS);

        GameObject target = nC_SmartTank_FSMRBS.NCEnTank;

        // No target means leave attack
        if (target == null)
            return typeof(NC_PatrolState_FSMRBS);

        // Check if visible enemy + distance
        Dictionary<GameObject, float> vis = nC_SmartTank_FSMRBS.VisibleEnemyTanks;

        bool enemyInSight = vis != null && vis.ContainsKey(target);
        if (!enemyInSight)
            return typeof(NC_PatrolState_FSMRBS);

        float enemyDistance = vis[target];

        
        // RULE:
        // if (enemyInSight && enemyDistance <= closeRange)
        //      nextState = Attack  (so: stay here and attack)
        
        if (enemyDistance <= closeRange)
        {
            // Aim + fire
            nC_SmartTank_FSMRBS.TurretFaceWorldPoint(target);
            nC_SmartTank_FSMRBS.TurretFireAtPoint(target);
            return null; // stay in Attack
        }

        // Otherwise, pursue
        return typeof(NC_PursueState_FSMRBS);
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting the attack state FSMRBS");
        return null;
    }
}
