using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static AStar;

public class NC_SmartTank_FSM : AITank
{
    private void InitializeStateMachine()
    {
        Dictionary<Type, NC_BaseState_FSM> states = new Dictionary<Type, NC_BaseState_FSM>();

        states.Add(typeof(NC_PatrolState_FSM), new NC_PatrolState_FSM(this));
        states.Add(typeof(NC_PursueState_FSM), new NC_PursueState_FSM(this));
        states.Add(typeof(NC_AttackState_FSM), new NC_AttackState_FSM(this));
        states.Add(typeof(NC_RetreatState_FSM), new NC_RetreatState_FSM(this));
        states.Add(typeof(NC_ScavengeState_FSM), new NC_ScavengeState_FSM(this));
        states.Add(typeof(NC_BaseAttackState_FSM), new NC_BaseAttackState_FSM(this));
        states.Add(typeof(NC_BaseDefendState_FSM), new NC_BaseDefendState_FSM(this));



        GetComponent<NC_BaseState_FSM>().SetStates(states);
    }
    private void Awake()
    {
        InitializeStateMachine();
    }

    public void AttackTarget()
    {
    }

    // Start is called before the first frame update
    public override void AITankStart()
    {
    }
    // Update is called once per frame
    public override void AITankUpdate()
    {
    }
    public override void AIOnCollisionEnter(Collision collision)
    {
    }
    public void GeneratePathToWorldPoint(GameObject pointInWorld)
    {
        a_FindPathToPoint(pointInWorld);
    }
    public void GeneratePathToWorldPoint(GameObject pointInWorld, HeuristicMode heuristic)
    {
        a_FindPathToPoint(pointInWorld, heuristic);
    }
    public void FollowPathToWorldPoint(GameObject pointInWorld, float normalizedSpeed)
    {
        a_FollowPathToPoint(pointInWorld, normalizedSpeed);
    }
    public void FollowPathToWorldPoint(GameObject pointInWorld, float normalizedSpeed, HeuristicMode heuristic)
    {
        a_FollowPathToPoint(pointInWorld, normalizedSpeed, heuristic);
    }
    public void FollowPathToRandomWorldPoint(float normalizedSpeed)
    {
        a_FollowPathToRandomPoint(normalizedSpeed);
    }

    public void FollowPathToRandomWorldPoint(float normalizedSpeed, HeuristicMode heuristic)
    {
        a_FollowPathToRandomPoint(normalizedSpeed, heuristic);
    }

    public void GenerateNewRandomWorldPoint()
    {
        a_GenerateRandomPoint();
    }
     public void TankStop()
    {
        a_StopTank();
    }
     public void TankGo()
    {
        a_StartTank();
    }
    public void TurretFaceWorldPoint(GameObject pointInWorld)
    {
        a_FaceTurretToPoint(pointInWorld);
    }
    public void TurretReset()
    {
        a_ResetTurret();
    }
    public void TurretFireAtPoint(GameObject pointInWorld)
    {
        a_FireAtPoint(pointInWorld);
    }
    public bool TankIsFiring()
    {
        return a_IsFiring;
    }
    public float TankCurrentHealth
    {
        get
        {
            return a_GetHealthLevel;
        }
    }
    public float TankCurrentAmmo
    {
        get
        {
            return a_GetAmmoLevel;
        }
    }
    public float TankCurrentFuel
    {
        get
        {
            return a_GetFuelLevel;
        }
    }
    public List<GameObject> MyBases
    {
        get
        {
            return a_GetMyBases;
        }
    }
    public Dictionary<GameObject, float> VisibleEnemyTanks
    {
        get
        {
            return a_TanksFound;
        }
    }
    public Dictionary<GameObject, float> VisibleConsumables
    {
        get
        {
            return a_ConsumablesFound;
        }
    }
    public Dictionary<GameObject, float> VisibleEnemyBases
    {
        get
        {
            return a_BasesFound;
        }
    }
}