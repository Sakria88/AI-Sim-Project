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

    // Enemy fire detection
    private float lastHealth;
    private float healthSampleTimer = 0f;

    private Queue<float> recentDamage = new Queue<float>();
    private const int DAMAGE_SAMPLE_WINDOW = 15;   // últimos 15 segundos
    private const float DAMAGE_THRESHOLD = 15f;    // daño de 1 disparo


    /// <summary>
    /// Initialises finite state machine, setting the states.
    /// </summary>
    private void InitializeStateMachine()
    {
        Dictionary<Type, NC_BaseState_FSMRBS> states = new Dictionary<Type, NC_BaseState_FSMRBS>();

        states.Add(typeof(NC_PatrolState_FSMRBS), new NC_PatrolState_FSMRBS(this));
        states.Add(typeof(NC_PursueState_FSMRBS), new NC_PursueState_FSMRBS(this));
        states.Add(typeof(NC_AttackState_FSMRBS), new NC_AttackState_FSMRBS(this));
        states.Add(typeof(NC_RetreatState_FSMRBS), new NC_RetreatState_FSMRBS(this));
        states.Add(typeof(NC_ScavengeState_FSMRBS), new NC_ScavengeState_FSMRBS(this));
        states.Add(typeof(NC_BaseAttackState_FSMRBS), new NC_BaseAttackState_FSMRBS(this));
        states.Add(typeof(NC_WaitState_FSMRBS), new NC_WaitState_FSMRBS(this));

        GetComponent<NC_StateMachine_FSMRBS>().SetStates(states);
    }

    /// <summary>
    /// Initialises the rules for the FSM-RBS.
    /// </summary>
    public void InitiliseRules()
    {
        ////////////////////////
        // Patrol State Rules //
        ////////////////////////

        // Enemy not in range but enemy base is
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "enemyNotDetected", "enemyBaseDetected", typeof(NC_BaseAttackState_FSMRBS), Rule.Predicate.And));
        // Health or fuel drops below safe threshold(<35)
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "lowHealth", "lowHealth", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "lowFuel", "lowHealth", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        // Enemy in far range → Pursue
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "enemyInSight", "enemyDistanceFar", typeof(NC_PursueState_FSMRBS), Rule.Predicate.And)); //###################
        // Enemy in mid range AND safe to wait → Wait
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "enemyDistanceMid", "enemyInSight", typeof(NC_WaitState_FSMRBS), Rule.Predicate.And));
        
        ////////////////////////
        // Pursue State Rules //
        ////////////////////////

        rules.AddRule(new Rule("NC_PursueState_FSMRBS", "targetReached", "enemyInSight", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_PursueState_FSMRBS", "lowHealth", "enemyNotDetected", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));

        //////////////////////
        // Wait State Rules //
        //////////////////////

        // Ammo < 3 OR Fuel < 35 → Scavenge
        rules.AddRule(new Rule("NC_WaitState_FSMRBS", "criticalAmmo", "criticalAmmo", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_WaitState_FSMRBS", "lowFuel", "lowFuel", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        // Enemy tank appears close → Attack
        rules.AddRule(new Rule("NC_WaitState_FSMRBS", "enemyDistanceClose", "enemyInSight", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        //Enemy visible but moving away (distance increases beyond far range)
        rules.AddRule(new Rule("NC_WaitState_FSMRBS", "enemyInSight", "enemyDistanceFar", typeof(NC_PursueState_FSMRBS), Rule.Predicate.And));
        // Enemy not visible after wait duration
        rules.AddRule(new Rule("NC_WaitState_FSMRBS", "waitTimerExceeded", "enemyNotDetected", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And));

        ////////////////////////
        // Attack State Rules //
        ////////////////////////
        // low health while attacking returns Retreat
        rules.AddRule(new Rule("NC_AttackState_FSMRBS", "enemyInSight", "lowHealth", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_AttackState_FSMRBS", "enemyInSight", "criticalAmmo", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And));

        // Low ammo OR low fuel while attacking returns Retreat
        rules.AddRule(new Rule("NC_AttackState_FSMRBS", "criticalAmmo", "enemyInSight", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_AttackState_FSMRBS", "lowFuel", "lowFuel", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        // Enemy no longer detected while attacking returns Patrol
        rules.AddRule(new Rule("NC_AttackState_FSMRBS", "enemyNotDetected", "enemyNotDetected", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And));

        /////////////////////////////
        // Base Attack State Rules //
        /////////////////////////////

        //Enemy tank appears within close range → Attack
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "enemyInSight", "enemyDistanceClose", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        //Ammo drops below safe threshold (≤3) → Scavenge
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "lowAmmo", "lowAmmo", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));
        //Health low → Retreat
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "lowHealth", "lowHealth", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And));
        // Fired ≥ 3 shots AND enemy not visible → Patrol
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "shotsFired", "enemyNotDetected", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And));
        // Base destroyed
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "enemyBaseDestroyed", "enemyNotDetected", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And));
        // Enemy tank firing nearby enemy base
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "enemyFiring", "enemyInSight", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And));

        /////////////////////////
        // Retreat State Rules //
        /////////////////////////
        // SafeZoneReached AND resourcesLow returns scavenge
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "safeZoneReached", "lowHealth", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And)); // TODO
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "safeZoneReached", "lowFuel", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And)); // TODO
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "safeZoneReached", "lowHealth", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And)); // TODO
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "safeZoneReached", "enemyNotDetected", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And)); // TODO


        //////////////////////////
        // Scavenge State Rules // ONLY 1 rule??? also non existent
        //////////////////////////

        // enoughAmmo and enoughFuel returns patrol
        rules.AddRule(new Rule("NC_ScavengeState_FSMRBS", "enoughHealth", "enoughFuel", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And));
    }

    /// <summary>
    /// Initialises the stats for the FSM-RBS.
    /// </summary>
    public void InitiliseStats()
    {
        // Condition tracking stats
        stats.Add("enemyBaseDetected", false);
        stats.Add("enemyBaseDestroyed", false);
        stats.Add("enemyInSight", false);
        stats.Add("enemyNotDetected", false);
        stats.Add("enemyFiring", false);
        stats.Add("targetReached", false);

        stats.Add("lowHealth", false);
        stats.Add("lowFuel", false);
        stats.Add("lowAmmo", false);
        stats.Add("criticalHealth", false);
        stats.Add("criticalFuel", false);
        stats.Add("criticalAmmo", false);
        stats.Add("enoughHealth", false);
        stats.Add("enoughFuel", false);
        stats.Add("enoughAmmo", false);
        stats.Add("shotsFired", false);

        stats.Add("enemyDistanceClose", false);
        stats.Add("enemyDistanceMid", false);
        stats.Add("enemyDistanceFar", false);

        stats.Add("waitTimerExceeded", false);
        stats.Add("safeZoneReached", false);


        // State tracking stats
        stats.Add("NC_PatrolState_FSMRBS", false);
        stats.Add("NC_PursueState_FSMRBS", false);
        stats.Add("NC_AttackState_FSMRBS", false);
        stats.Add("NC_RetreatState_FSMRBS", false);
        stats.Add("NC_ScavengeState_FSMRBS", false);
        stats.Add("NC_BaseAttackState_FSMRBS", false);
        stats.Add("NC_WaitState_FSMRBS", false);
    }

    /// <summary>
    /// Updates all stats for the FSM-RBS.
    /// </summary>
    public void UpdateGlobalStats()
    {
        CheckEnemyBaseDetected();
        CheckEnemyInSight();
        CheckEnemyNotDetected();
        CheckEnemyFiring();

        CheckHealth();
        CheckFuel();
        CheckAmmo();

        CheckEnemyDistanceClose();
        CheckEnemyDistanceMid();
        CheckEnemyDistanceFar();
        //
        //CheckEnemyBaseDestroyed();
        //CheckShotsFired();
        //CheckTargetReached();
        //CheckWaitTimerExceeded(10f);
        //CheckSafeZoneReached();
    }
    
    /// <summary>
    /// Initialises the AI Tank.
    /// </summary>
    public override void AITankStart()
    {
        InitializeStateMachine();
        InitiliseStats();
        InitiliseRules();

        lastHealth = TankCurrentHealth;
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
            else if (entry.Key.name.Contains("Health") == false)
            {
                consumableHealth = null;
            }
            else if (entry.Key.name.Contains("Ammo"))
            {
                consumableAmmo = entry.Key;
            }
            else if (entry.Key.name.Contains("Ammo") == false)
            {
                consumableAmmo = null;
            }
            else if (entry.Key.name.Contains("Fuel"))
            {
                consumableFuel = entry.Key;
            }
            else if (entry.Key.name.Contains("Fuel") == false)
            {
                consumableFuel = null;
            }
        }
    }

    //-----------------------------------------
    //  CHECKING RULE HELPERS
    //-----------------------------------------

    /// <summary>
    /// Checks if the enemy base is detected.
    /// </summary>
    public void CheckEnemyBaseDetected()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnBase != null)
        {
            stats["enemyBaseDetected"] = true;
        }
        else
        {
            stats["enemyBaseDetected"] = false;
        }
    }

    /// <summary>
    /// Checks if the enemy base is destroyed.
    /// </summary>
    public void CheckEnemyBaseDestroyed()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnBase == null)
        {
            stats["enemyBaseDestroyed"] = true;
        }
        else
        {
            stats["enemyBaseDestroyed"] = false;
        }
    }

    /// <summary>
    /// Checks if the enemy is in sight.
    /// </summary>
    public void CheckEnemyInSight()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            stats["enemyInSight"] = true;
        }
        else
        {
            stats["enemyInSight"] = false;
        }
    }

    /// <summary>
    /// Checks if the enemy is not detected.
    /// </summary>
    public void CheckEnemyNotDetected()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            stats["enemyNotDetected"] = false;
        }
        else
        {
            stats["enemyNotDetected"] = true;
        }
    }

    /// <summary>
    /// Checks if the enemy is firing by monitoring health changes over time.
    /// </summary>
    public void CheckEnemyFiring()
    {
        healthSampleTimer += Time.deltaTime;

        // Sample health once per second
        if (healthSampleTimer < 1f)
            return;

        healthSampleTimer = 0f;

        float currentHealth = TankCurrentHealth;
        float damageTaken = Mathf.Max(0f, lastHealth - currentHealth);

        lastHealth = currentHealth;

        // Register recent damage
        recentDamage.Enqueue(damageTaken);

        if (recentDamage.Count > DAMAGE_SAMPLE_WINDOW)
        {
            recentDamage.Dequeue();
        }

        float totalRecentDamage = recentDamage.Sum();

        // if damage in the last DAMAGE_SAMPLE_WINDOW seconds exceeds threshold, enemy is firing
        stats["enemyFiring"] = totalRecentDamage >= DAMAGE_THRESHOLD;
    }

    /// <summary>
    /// Checks the health levels and updates the corresponding stats.
    /// </summary>
    public void CheckHealth()
    {
        float health = TankCurrentHealth;

        stats["criticalHealth"] = health < 10f;
        stats["lowHealth"] = health >= 10f && health < 35f;
        stats["enoughHealth"] = health > 50f;
    }

    /// <summary>
    /// Checks the ammo levels and updates the corresponding stats.
    /// </summary>
    public void CheckAmmo()
    {
        float ammo = TankCurrentAmmo;

        stats["criticalAmmo"] = ammo < 1f;
        stats["lowAmmo"] = ammo >= 1f && ammo <= 3f;
        stats["enoughAmmo"] = ammo > 5f;
    }

    /// <summary>
    /// Checks the fuel levels and updates the corresponding stats.
    /// </summary>
    public void CheckFuel()
    {
        float fuel = TankCurrentFuel;

        stats["criticalFuel"] = fuel < 10f;
        stats["lowFuel"] = fuel >= 10f && fuel <= 35f;
        stats["enoughFuel"] = fuel > 50f;
    }


    /// <summary>
    /// Checks if enough shots have been fired (≥3).
    /// </summary>
    public void CheckShotsFired()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.TankIsFiring())
        {
            stats["shotsFired"] = true;
        }
        else
        {
            stats["shotsFired"] = false;
        }
    }

    /// <summary>
    /// Checks if the enemy is within close distance (<25 units).
    /// </summary>
    public void CheckEnemyDistanceClose()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            float distance = Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position);

            if (distance < 25f)
                stats["enemyDistanceClose"] = true;
        }
        else
        {
            stats["enemyDistanceClose"] = false;
        }
    }

    /// <summary>
    /// Checks if the enemy is within mid distance (25-45 units).
    /// </summary>
    public void CheckEnemyDistanceMid()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            float distance = Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position);
            if (distance >= 25f && distance < 45f)
            {
                stats["enemyDistanceMid"] = true;
            }
        }
        else
        {
            stats["enemyDistanceMid"] = false;
        }
    }

    /// <summary>
    /// Checks if the enemy is within far distance (>=45 units).
    /// </summary>
    public void CheckEnemyDistanceFar()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            float distance = Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position);
            if (distance >= 45f)
            {
                stats["enemyDistanceMid"] = true;
            }
        }
        else
        {
            stats["enemyDistanceFar"] = false;
        }
    }

    /// <summary>
    /// Checks if the wait timer has exceeded the specified wait time.
    /// </summary>
    /// <param name="waitTime"></param>
    public void CheckWaitTimerExceeded(float waitTime)
    {
        if (waitTime >= 3)
        {
            stats["waitTimerExceeded"] = true;
        }
        else
        {
            stats["waitTimerExceeded"] = false;
        }
    }

    // TODO
    /// <summary>
    /// Checks if the safe zone has been reached.
    /// </summary>
    public void CheckSafeZoneReached(Vector3 safeZonePosition, float threshold)
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, safeZonePosition) < threshold)
        {
            stats["safeZoneReached"] = true;
        }
        else
        {
            stats["safeZoneReached"] = false;
        }
    }

    /// <summary>
    /// Checks if the target has been reached.
    /// </summary>
    public void CheckTargetReached()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        if (nC_SmartTank_FSMRBS.NCEnTank != null && Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position) < 25f) //TODO only 25?

        {
            stats["targetReached"] = true;
        } else
        {
            stats["targetReached"] = false;
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