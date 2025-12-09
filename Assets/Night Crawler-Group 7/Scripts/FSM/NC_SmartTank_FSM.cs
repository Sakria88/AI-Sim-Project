using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static AStar;

public class NC_SmartTank_FSM : AITank
{
    public GameObject NCEnTank;        /*!< <c>enemyTank</c> stores a reference to a target enemy tank. 
                                        * This should be taken from <c>enemyTanksFound</c>, only whilst within the tank sensor. 
                                        * Reference should be removed and refreshed every update. */

    public GameObject consumable;       /*!< <c>consumable</c> stores a reference to a target consumable. 
                                        * This should be taken from <c>consumablesFound</c>, only whilst within the tank sensor. 
                                        * Reference should be removed and refreshed every update. */

    public GameObject NCEnBase;        /*!< <c>enemyBase</c> stores a reference to a target enemy base. 
                                         * This should be taken from <c>enemyBasesFound</c>, only whilst within the tank sensor. 
                                        * Reference should be removed and refreshed every update. */

    public GameObject myBase;         /*!< <c>myBase</c> stores a reference to an allied base. 
                                        * This should be taken from <c>myBases</c>, at any time.  */

    float t;    /*!< <c>t</c> stores timer value */
    public HeuristicMode heuristicMode; /*!< <c>heuristicMode</c> Which heuristic used for find path. */

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



        GetComponent<NC_StateMachine_FSM>().SetStates(states);
    }
    private void Awake()
    {
        InitializeStateMachine();
    }

    // Defitnition of behaviour of the different states

    public void PatrolMap()
    {
    }
    public void PursueEnemy()
    {
    }
    public void AttackEnemy()
    {
    }
    public void RetreatToSafety()
    {
    }
    public void ScavengeResources()
    {
    }
    public void AttackEnemyBase()
    {
    }
    public void DefendAllyBase()
    {
        myBase = MyBases[0];

        if (myBase != null)
        {
            //go close to it and make a defensive perimeter searching for enemies
            if (Vector3.Distance(transform.position, myBase.transform.position) < 25f)
            {
                // Reorient tank to face outwards from base using TurretFaceWorldPoint
                Vector3 directionFromBase = transform.position - myBase.transform.position;
                GameObject pointToFace = new GameObject();
                pointToFace.transform.position = transform.position + directionFromBase.normalized * 10f;
                TurretFaceWorldPoint(pointToFace);


            }
            else
            {
                FollowPathToWorldPoint(myBase, 1f, heuristicMode);
            }

        }

    }

public void MoveTowardBase(GameObject targetBase, float speed)
{
    // helper for base-attack movement tank should move 
    // directly toward the enemy base using the normal pathfinding system.
    // if no base is currently detected, tank does nothing here
    if (targetBase == null)
        return;

    FollowPathToWorldPoint(targetBase, speed, heuristicMode);
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

    internal void FollowPathToPoint(GameObject nCEnBase, float v, HeuristicMode heuristicMode)
    {
        throw new NotImplementedException();
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