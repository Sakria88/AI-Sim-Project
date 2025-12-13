using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static AStar;


public class NC_SmartTank_FSMRBS : AITank
{
    public GameObject NCEnTank;        /*!< <c>enemyTank</c> stores a reference to a target enemy tank. 
                                        * This should be taken from <c>enemyTanksFound</c>, only whilst within the tank sensor. 
                                        * Reference should be removed and refreshed every update. */

    public GameObject consumable;       /*!< <c>consumable</c> stores a reference to a target consumable. 
                                        * This should be taken from <c>consumablesFound</c>, only whilst within the tank sensor. 
                                        * Reference should be removed and refreshed every update. */

    public GameObject consumableHealth;
    public GameObject consumableAmmo;
    public GameObject consumableFuel;

    public GameObject NCEnBase;        /*!< <c>enemyBase</c> stores a reference to a target enemy base. 
                                         * This should be taken from <c>enemyBasesFound</c>, only whilst within the tank sensor. 
                                        * Reference should be removed and refreshed every update. */

    public GameObject myBase;         /*!< <c>myBase</c> stores a reference to an allied base. 
                                        * This should be taken from <c>myBases</c>, at any time.  */

    float t;    /*!< <c>t</c> stores timer value */
    public HeuristicMode heuristicMode; /*!< <c>heuristicMode</c> Which heuristic used for find path. */

    public Dictionary<string, bool> stats = new Dictionary<string, bool>();
    public Rules rules = new Rules();


    private void InitializeStateMachine()
    {
        Dictionary<Type, NC_BaseState_FSMRBS> states = new Dictionary<Type, NC_BaseState_FSMRBS>();

        states.Add(typeof(NC_PatrolState_FSMRBS), new NC_PatrolState_FSMRBS(this));
        states.Add(typeof(NC_PursueState_FSMRBS), new NC_PursueState_FSMRBS(this));
        states.Add(typeof(NC_AttackState_FSMRBS), new NC_AttackState_FSMRBS(this));
        states.Add(typeof(NC_RetreatState_FSMRBS), new NC_RetreatState_FSMRBS(this));
        states.Add(typeof(NC_ScavengeState_FSMRBS), new NC_ScavengeState_FSMRBS(this));
        states.Add(typeof(NC_BaseAttackState_FSMRBS), new NC_BaseAttackState_FSMRBS(this));

        GetComponent<NC_StateMachine_FSMRBS>().SetStates(states);
    }

    // Definition of behaviour of the different states
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
    // Start is called before the first frame update
    public void RandomRoam()
    {
        FollowPathToRandomWorldPoint(1f, heuristicMode);
    }

    // Start is called before the first frame update
    public override void AITankStart()
    {
        InitializeStateMachine();
        InitiliseStats();
        InitiliseRules();
    }

    // Update is called once per frame
    public override void AITankUpdate()
    {
        // Update visible objects, enemy tanks, bases and consumables
        if (VisibleEnemyTanks.Count > 0)
        {
            NCEnTank = VisibleEnemyTanks.First().Key;
        }
        else
        {
            NCEnTank = null;
        }

        if (VisibleEnemyBases.Count > 0)
        {
            NCEnBase = VisibleEnemyBases.First().Key;
        }

        if (VisibleConsumables.Count > 0)
        {
            consumable = VisibleConsumables.First().Key;
        }
        else
        {
            consumable = null;
        }

        foreach (var entry in VisibleConsumables)
        {
            if (entry.Key.name.Contains("Health"))
            {
                consumableHealth = entry.Key;
            }
            else if (entry.Key.name.Contains("Ammo"))
            {
                consumableAmmo = entry.Key;
            }
            else if (entry.Key.name.Contains("Fuel"))
            {
                consumableFuel = entry.Key;
            }
        }

    }

    public void InitiliseRules()
    {
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "lowHealth", "!targetSpotted", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_PursueState_FSMRBS", "targetReached", "targetSpotted", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_PursueState_FSMRBS", "lowHealth", "!targetSpotted", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));

    }

    public void InitiliseStats()
    {
        stats.Add("lowHealth", false);

        stats.Add("targetSpotted", false);

        stats.Add("targetReached", false);

        stats.Add("NC_PatrolState_FSMRBS", false);

        stats.Add("NC_PursueState_FSMRBS", false);

        stats.Add("NC_RetreatState_FSMRBS", false);

        stats.Add("NC_AttackState_FSMRBS", false);


    }

    public void checkLowHealth()
    {
        var nc_smarttankRBSM = GetComponent<NC_SmartTank_FSMRBS>();

        float Health = nc_smarttankRBSM.TankCurrentHealth;

        if (Health < 35)
        {
            stats["lowHealth"] = true;

        }
        else
        {
            stats["lowHealth"] = false;
        }

    }

    public void checkLowFuel() 
    {
        var nc_smarttankRBSM = GetComponent<NC_SmartTank_FSMRBS>();

        float Fuel = nc_smarttankRBSM.TankCurrentAmmo;

        if (Fuel <= 30)
        {
            stats["lowFuel"] = true;

        }
        else
        {
            stats["lowFuel"] = false;
        }

    }

    public void CheckTargetReached() //Function for checking if target is in  range
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position) < 25f)

        {
            stats["targetReached"] = true;
        }

        if (nC_SmartTank_FSMRBS.NCEnTank == null)

        {
            stats["targetReached"] = false;
        }

    }

    public void CheckTargetSpotted() //Function in smart tank for checking if the target is there 
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();

        if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position) < 50f)
        {
            stats["targetSpotted"] = true;
        }

        if (nC_SmartTank_FSMRBS.NCEnTank == null)
        {
            stats["targetSpotted"] = false;
        }


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

    //-----------------------------------------
    //  PATROL & BASE ATTACK HELPERS
    //-----------------------------------------
    //generate a temporary world point to feed into A* pathfinding

   public GameObject CreateWorldPoint(Vector3 position)
    {
        GameObject point = new GameObject("WorldPoint");
        point.transform.position = position;
        return point;
    }

    // used by patrol states to move to random map locations
    public void MoveToPatrolPoint(Vector3 targetPosition)
    {
        GameObject point = CreateWorldPoint(targetPosition);
        FollowPathToWorldPoint(point, 1f, heuristicMode);
    }
   // used by base attack state to push toward enemy base
    public void MoveTowardBase(GameObject targetBase, float speed)
    {
        if (targetBase == null)
            return;

        FollowPathToWorldPoint(targetBase, speed, heuristicMode);
    }

/// ----------------------
/// TANK CONTROL HELPERS
/// ----------------------
  public void TankStop()
    {
        a_StopTank();
    }
     public void TankGo()
    {
        a_StartTank();
    }

///-------------------------
///TURRET CONTROL
///-------------------------
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

///-------------------------
/// TANK STATUS PROPERTIES
///-------------------------
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

    ///--------------------------------
    /// SENSOR DATA ACCESS
    ///--------------------------------
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