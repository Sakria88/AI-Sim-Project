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
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "lowHealth", "lowFuel", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.Or));
        // Enemy in far range → Pursue
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "enemyInSight", "enemyInFarRange", typeof(NC_PursueState_FSMRBS), Rule.Predicate.And)); //###################
        // Enemy in mid range AND safe to wait → Wait
        rules.AddRule(new Rule("NC_PatrolState_FSMRBS", "enemyInMidRange", "canEnterWait", typeof(NC_Wait_FSMRBS), Rule.Predicate.And));
        
        ////////////////////////
        // Pursue State Rules //
        ////////////////////////

        rules.AddRule(new Rule("NC_PursueState_FSMRBS", "targetReached", "enemyInSight", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        rules.AddRule(new Rule("NC_PursueState_FSMRBS", "lowHealth", "enemyNotDetected", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));

        //////////////////////
        // Wait State Rules //
        //////////////////////

        // Ammo < 3 OR Fuel < 35 → Scavenge
        rules.AddRule(new Rule("NC_Wait_FSMRBS", "criticalAmmo", "lowFuel", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.Or));
        // Enemy tank appears close → Attack
        rules.AddRule(new Rule("NC_Wait_FSMRBS", "enemyInCloseRange", "enemyInSight", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        //Enemy visible but moving away (distance increases beyond far range)
        rules.AddRule(new Rule("NC_Wait_FSMRBS", "enemyInSight", "enemyDistanceFar", typeof(NC_PursueState_FSMRBS), Rule.Predicate.And));
        // Enemy not visible after wait duration
        rules.AddRule(new Rule("NC_Wait_FSMRBS", "waitTimerExceeded", "enemyNotDetected", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And));
        

        ////////////////////////
        // Attack State Rules //
        ////////////////////////

        /// No rules????

        /////////////////////////////
        // Base Attack State Rules //
        /////////////////////////////

        //Enemy tank appears within close range → Attack
        rules.AddRule(new Rule("Nc_BaseAttackState_FSMRBS", "enemyInSight", "enemyDistanceClose", typeof(NC_AttackState_FSMRBS), Rule.Predicate.And));
        //Ammo drops below safe threshold (≤3) → Scavenge
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "criticalAmmo", "!enemyInSight", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And)); //################### we need to check enemy? also, not correct
        //Health critical → Retreat
        rules.AddRule(new Rule("Nc_BaseAttackState_FSMRBS", "healthCritical", "enemyInSight", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And)); //################### healthCritical needs enemy on sight? Health critical or low?
        // Fired ≥ 3 shots AND enemy not visible → Patrol
        rules.AddRule(new Rule("Nc_BaseAttackState_FSMRBS", "shotsFiredEnough", "enemyInSight", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.nAnd)); //###################
        // Base destroyed
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "enemyBaseDestroyed", "!enemyInSight", typeof(NC_PatrolState_FSMRBS), Rule.Predicate.And)); //################### we need to check enemy? also, not correct
        // Enemy tank firing nearby enemy base
        rules.AddRule(new Rule("NC_BaseAttackState_FSMRBS", "enemyFiring", "enemyInSight", typeof(NC_RetreatState_FSMRBS), Rule.Predicate.And));

        /////////////////////////
        // Retreat State Rules //
        /////////////////////////
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "lowHealth", "targetSpotted", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.nAnd)); //###################
        // safeZoneReached AND resourcesLow returns scavenge
        rules.AddRule(new Rule("NC_RetreatState_FSMRBS", "safeZoneReached", "resourcesLow", typeof(NC_ScavengeState_FSMRBS), Rule.Predicate.And));

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
    /// Checks if the enemy is detected.
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
    /// Checks if the tank has less than 35 health.
    /// </summary>
    public void CheckLowHealth()
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

    /// <summary>
    /// Checks if the tank has less than 35 fuel.
    /// </summary>
    public void CheckLowFuel() 
    {
        var nc_smarttankRBSM = GetComponent<NC_SmartTank_FSMRBS>();

        float Fuel = nc_smarttankRBSM.TankCurrentAmmo;

        if (Fuel <= 35)
        {
            stats["lowFuel"] = true;

        }
        else
        {
            stats["lowFuel"] = false;
        }

    }

    /// <summary>
    /// Checks if the tank has less than 5 ammo.
    /// </summary>
    public void CheckLowAmmo()
    {
        var nc_smarttankRBSM = GetComponent<NC_SmartTank_FSMRBS>();
        float Ammo = nc_smarttankRBSM.TankCurrentAmmo;
        if (Ammo <= 5)
        {
            stats["lowAmmo"] = true;
        }
        else
        {
            stats["lowAmmo"] = false;
        }
    }

    /// <summary>
    /// Checks if the tank has critical health (<10).
    /// </summary>
    public void CheckCriticalHealth()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        float Health = nC_SmartTank_FSMRBS.TankCurrentHealth;
        if (Health < 10)
        {
            stats["criticalHealth"] = true;
        }
        else
        {
            stats["criticalHealth"] = false;
        }
    }

    /// <summary>
    /// Checks if the tank has critical fuel (<10).
    /// </summary>
    public void CheckCriticalFuel()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        float Fuel = nC_SmartTank_FSMRBS.TankCurrentFuel;
        if (Fuel < 10)
        {
            stats["criticalFuel"] = true;
        }
        else
        {
            stats["criticalFuel"] = false;
        }
    }

    /// <summary>
    /// Checks if the tank has critical ammo (<1).
    /// </summary>
    public void CheckCriticalAmmo()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        float Ammo = nC_SmartTank_FSMRBS.TankCurrentAmmo;
        if (Ammo < 1)
        {
            stats["criticalAmmo"] = true;
        }
        else
        {
            stats["criticalAmmo"] = false;
        }
    }

    /// <summary>
    /// Checks if the tank has enough health (>50).
    /// </summary>
    public void CheckEnoughHealth()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        float Health = nC_SmartTank_FSMRBS.TankCurrentHealth;
        if (Health > 50)
        {
            stats["enoughHealth"] = true;
        }
        else
        {
            stats["enoughHealth"] = false;
        }
    }

    /// <summary>
    /// Checks if the tank has enough fuel (>50).
    /// </summary>
    public void CheckEnoughFuel()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        float Fuel = nC_SmartTank_FSMRBS.TankCurrentFuel;
        if (Fuel > 50)
        {
            stats["enoughFuel"] = true;
        }
        else
        {
            stats["enoughFuel"] = false;
        }
    }

    /// <summary>
    /// Checks if the tank has enough ammo (>5).
    /// </summary>
    public void CheckEnoughAmmo()
    {
        var nC_SmartTank_FSMRBS = GetComponent<NC_SmartTank_FSMRBS>();
        float Ammo = nC_SmartTank_FSMRBS.TankCurrentAmmo;
        if (Ammo > 5)
        {
            stats["enoughAmmo"] = true;
        }
        else
        {
            stats["enoughAmmo"] = false;
        }
    }

    //TODO
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
        if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position) < 25f && nC_SmartTank_FSMRBS.NCEnTank != null)
        {
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
        float distance = Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position );
        if (distance >= 25f && distance < 45f && nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            stats["enemyDistanceMid"] = true;
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
        float distance = Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position);
        if (distance >= 45f && nC_SmartTank_FSMRBS.NCEnTank != null)
        {
            stats["enemyDistanceFar"] = true;
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
        t += Time.deltaTime;
        if (t >= waitTime)
        {
            stats["waitTimerExceeded"] = true;
            t = 0f; // Reset timer after exceeding
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
        if (Vector3.Distance(nC_SmartTank_FSMRBS.transform.position, nC_SmartTank_FSMRBS.NCEnTank.transform.position) < 25f) //TODO only 25?

        {
            stats["targetReached"] = true;
        }

        if (nC_SmartTank_FSMRBS.NCEnTank == null)

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