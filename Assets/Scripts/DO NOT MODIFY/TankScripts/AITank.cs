using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static AStar;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Class <c>AITank</c> handles the Tank functions. Must be used as base class for SmartTank/DumbTank classes.
/// </summary>
public abstract class AITank : MonoBehaviour
{
    float angle;
    bool slowTurn;

    private float fuel = 100f;  /*!< Current fuel level */
    private float fuelMax = 125f; /*!< Max fual level */
    private int ammo; /*!< Current ammo */
    private int ammoMax = 20; /*!< Max ammo */
    private float health = 100f; /*!< Current health */
    private float healthMax = 125f; /*!< Max health */
    private float moveSpeed = 1700; /*!< Tank max movement speed */
    private float bodyRotationSpeed = 7f; /*!< Tank max rotation speed */
    private float turrentRotationSpeed = 2f; /*!< Tank max turret rotation speed */
    private GameObject projectileObject; /*!< Tank projectile reference */
    private GameObject randomPoint; /*!< Random point to go to */

    private ParticleSystem smokeParticles;
    private ParticleSystem TankExplosionParticle;
    private ParticleSystem fireParticle;
    private ParticleSystem.EmissionModule smokePartEmission;
    private GameObject turretObject;
    private AStar aStarScript; /*!< Reference to <c>AStar</c> script for pathfinding */
    private List<GameObject> myBases = new List<GameObject>(); /*!< List of freindly bases */
    private Rigidbody rb;
    private SpriteRenderer ammoSprite; /*!< Ammo bar on tank */
    private SpriteRenderer fuelSprite; /*!< Fuel bar on tank */
    private SpriteRenderer healthSprite; /*!< health bar on tank */
    private Quaternion turretStartRot;
    private bool firing;
    private bool destroyed;
    private Vector3 projectileForce = new Vector3(0, 0, 60);
    private bool collisionWithObstacle;
    private float tankMaxSpeed = 22f; /*!< tank max speed */
    private float tankMaxSpeedHolder;
    //sensor
    private float viewRadius = 52f; /*!< How far out sensor goes */
    private float viewAngle = 150f; /*!< How how wide sensor is */

    private AudioSource engineSound;
    private AudioSource fireSound;

    private LayerMask tankMainMask;
    private LayerMask obstacleMask;
    private LayerMask consumableMask;
    private LayerMask baseMask;

    private GameObject sensorPoint;

    private Dictionary<GameObject, float> targetsFound = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> basesFound = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> consumablesFound = new Dictionary<GameObject, float>();

    private List<Vector3> pathFound = new List<Vector3>();

    private bool randomNodeFound = true;

    private GameController gameControllerScript;

    private Vector3 velocity;
    private Vector3 velocityRot;
    private Vector3 velocityCentre;

    private Vector3 velocityTurretRot;
    private bool projectileHit;
    private bool collisionWithObstacleRock;

    UIControllerScript uiContScript;

    // Start is called before the first frame update
    private void Start()
    {


        //References
        gameControllerScript = GameObject.Find("GameController - DO NOT MODIFY -").GetComponent<GameController>();
        TankExplosionParticle = GameObject.Find("GameController - DO NOT MODIFY -").transform.Find("TankExplosionParticle").GetComponent<ParticleSystem>();
        projectileObject = GameObject.Find("Projectile").gameObject;
        randomPoint = GameObject.Instantiate(GameObject.Find("RandomPoint").gameObject, Vector3.zero, Quaternion.identity);
        rb = GetComponent<Rigidbody>();
        smokeParticles = transform.Find("Model").transform.Find("Body").transform.Find("SmokeParticles").GetComponent<ParticleSystem>();
        fireParticle = transform.Find("Model").transform.Find("Turret").transform.Find("FireParticle").GetComponent<ParticleSystem>();
        ammoSprite = transform.Find("Model").transform.Find("Turret").transform.Find("Ammo").transform.Find("Bar").GetComponent<SpriteRenderer>();
        fuelSprite = transform.Find("Stats").transform.Find("Fuel").transform.Find("Bar").GetComponent<SpriteRenderer>();
        healthSprite = transform.Find("Stats").transform.Find("Health").transform.Find("Bar").GetComponent<SpriteRenderer>();
        smokePartEmission = smokeParticles.emission;
        aStarScript = GameObject.Find("AStarPlane").GetComponent<AStar>();
        turretObject = transform.Find("Model").transform.Find("Turret").gameObject;
        turretStartRot = turretObject.transform.localRotation;
        engineSound = GetComponent<AudioSource>();
        fireSound = transform.Find("FireSound").GetComponent<AudioSource>();
        uiContScript = FindObjectOfType<UIControllerScript>();

        //Search for friendly bases.
        BaseScript[] basesScript = transform.parent.GetComponentsInChildren<BaseScript>();
        //Store bases in a list. 
        foreach (var item in basesScript)
        {
            myBases.Add(item.gameObject);
        }

        //Sensor values.
        sensorPoint = turretObject;

        //Tank speed setup.
        tankMaxSpeedHolder = tankMaxSpeed;

        //Tank stats setup.
        ammo = 15;
        fuel = 100f;
        health = 100f;
        rb.mass = 1200;
        rb.drag = 2.9f;
        rb.angularDrag = 3.3f;

        //Layer mask references.
        tankMainMask = LayerMask.GetMask("TankMain");
        obstacleMask = LayerMask.GetMask("Obstacle");
        consumableMask = LayerMask.GetMask("Consumable");
        baseMask = LayerMask.GetMask("Base");

        //Search targets
        StartCoroutine(TargetsFind(0.2f));

        //Abstact Start Function, must be implemented in derived class. 
        AITankStart();
    }

    // Update is called once per frame
    private void Update()
    {
        //Particles
        smokePartEmission.rateOverTime = Mathf.Abs(((rb.velocity.x + rb.velocity.y + rb.velocity.z) / 3f) * 10f);

        //Fuel depletion
        fuel -= Mathf.Abs(((rb.velocity.x + rb.velocity.y + rb.velocity.z) / 3f) * 0.002f);
        //Idle fuel depletion
        fuel -= 0.004f;
        //Fuel level sprite
        fuelSprite.size = new Vector2(fuelSprite.size.x, Mathf.Lerp(0, 1.7f, Mathf.InverseLerp(0, fuelMax, fuel)));
        //Fuel empty message
        if (fuel <= 0)
        {
            print(this.transform.parent.gameObject.name + " has no Fuel!");
            destroyed = true;
            StartCoroutine(DestroyWait());
            rb.isKinematic = true;
        }

        //Ammo level sprite
        ammoSprite.size = new Vector2(ammoSprite.size.x, Mathf.Lerp(0, 1.7f, Mathf.InverseLerp(0, ammoMax, ammo)));

        //Health level sprite
        healthSprite.size = new Vector2(healthSprite.size.x, Mathf.Lerp(0, 1.7f, Mathf.InverseLerp(0, healthMax, health)));
        //Health depleted, destroy tank
        if (health <= 0)
        {
            destroyed = true;
            print(this.transform.parent.gameObject.name + " has been destroyed!");
            StartCoroutine(DestroyWait());
            rb.isKinematic = true;
        }


        //Tank movement sound
        engineSound.pitch = Mathf.Abs(((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.y) + Mathf.Abs(rb.velocity.z)) / 3f) * 0.12f + 0.3f);

        //Abstract Update Function
        AITankUpdate();

        gameControllerScript.SetUIStats(health, fuel, ammo, this);
    }

    //Destroy function.
    private IEnumerator DestroyWait()
    {
        yield return new WaitForSeconds(3f);
        GameObject.Instantiate((GameObject)TankExplosionParticle.gameObject, transform.position, Quaternion.identity).GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Check for collisions.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.tag == "Projectile")
        //{
        //    if (projectileHit == false)
        //    {
        //        StartCoroutine(ProjectileHit(Vector3.Distance(collision.transform.position, this.gameObject.transform.position)));
        //    }
        //}

        if (collision.gameObject.tag == "Health")
        {
            health = Mathf.Clamp(health + 25f, 0f, healthMax);
            print(this.transform.parent.gameObject.name + " has collected Health!");
            collision.gameObject.SetActive(false);
            gameControllerScript.ConsumableCollection(collision.gameObject.tag);
        }

        if (collision.gameObject.tag == "Ammo")
        {
            ammo = Mathf.Clamp(ammo + 3, 0, ammoMax);
            print(this.transform.parent.gameObject.name + " has collected Ammo!");
            collision.gameObject.SetActive(false);
            gameControllerScript.ConsumableCollection(collision.gameObject.tag);
        }

        if (collision.gameObject.tag == "Fuel")
        {
            fuel = Mathf.Clamp(fuel + 30f, 0f, fuelMax);
            print(this.transform.parent.gameObject.name + " has collected Fuel!");
            collision.gameObject.SetActive(false);
            gameControllerScript.ConsumableCollection(collision.gameObject.tag);
        }

        if (collision.gameObject.tag == "Obstacle")
        {
            if (collisionWithObstacle == false)
            {
                StartCoroutine(CollisionWithObstacle());
            }
        }

        if (collision.gameObject.tag == "Base")
        {
            if (collisionWithObstacle == false)
            {
                StartCoroutine(CollisionWithObstacle());
            }
        }

        if (collision.gameObject.tag == "ObstacleRock")
        {
            if (collisionWithObstacleRock == false)
            {
                StartCoroutine(CollisionWithObstacleRock());
            }
        }

        AIOnCollisionEnter(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Projectile")
        {
            if (projectileHit == false)
            {
                StartCoroutine(ProjectileHit(Vector3.Distance(other.transform.position, this.gameObject.transform.position)));
            }
        }
    }

    private IEnumerator CollisionWithObstacle()
    {
        collisionWithObstacle = true;
        yield return new WaitForSeconds(1f);
        collisionWithObstacle = false;
        yield return new WaitForSeconds(4f);
    }

    private IEnumerator CollisionWithObstacleRock()
    {
        collisionWithObstacleRock = true;
        yield return new WaitForSeconds(2f);
        collisionWithObstacleRock = false;
        yield return new WaitForSeconds(5f);
    }

    private IEnumerator ProjectileHit(float hitAccuracy)
    {
        float accuracy = Mathf.InverseLerp(2.5f, 3f, hitAccuracy);
        projectileHit = true;
        health -= Mathf.Lerp(15, 10, accuracy);
        health = Mathf.Clamp(Mathf.Round(health), 0, healthMax);
        print(this.transform.parent.gameObject.name + " has been hit!");
        yield return new WaitForSeconds(0.5f);
        projectileHit = false;
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Request a path from this to pointInWorld
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    protected void a_FindPathToPoint(GameObject pointInWorld)
    {
        //Path holder
        List<Node> path = new List<Node>();

        //Find path if pointInWorld or fuel are not null
        if (pointInWorld != null && fuel > 0)
        {
            //AStar scipt holder.
            AStar tempAStar = aStarScript;
            //Request path to pointInWorldGameObject
            path = tempAStar.RequestPath(this.gameObject, pointInWorld);
        }

        //If path is not null and more than 3
        if (path != null && path.Count > 3)
        {
            //Clear old pathFound
            pathFound.Clear();
            //Populate pathFound with path
            foreach (Node item in path)
            {
                pathFound.Add(item.nodePos);
            }
        }
    }

    /// <summary>
    /// Request a path from this to pointInWorld
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    /// <param name="heuristic">Chosen heuristic for path finding</param>
    protected void a_FindPathToPoint(GameObject pointInWorld, HeuristicMode heuristic)
    {
        //Path holder
        List<Node> path = new List<Node>();

        //Find path if pointInWorld or fuel are not null
        if (pointInWorld != null && fuel > 0)
        {
            //AStar scipt holder.
            AStar tempAStar = aStarScript;
            //Request path to pointInWorldGameObject
            path = tempAStar.RequestPath(this.gameObject, pointInWorld, heuristic);
        }

        //If path is not null and more than 3
        if (path != null && path.Count > 3)
        {
            //Clear old pathFound
            pathFound.Clear();
            //Populate pathFound with path
            foreach (Node item in path)
            {
                pathFound.Add(item.nodePos);
            }
        }
    }


    /// <summary>
    ///Follow path to a target (GameObject) at speed (value between 0-1)
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    /// <param name="normalizedSpeed">Speed between 0-1</param>
    protected void a_FollowPathToPoint(GameObject pointInWorld, float normalizedSpeed)
    {
        //Random position not found.
        randomNodeFound = true;

        //Set speed of tank based on normalized speed.
        float speed = Mathf.Lerp(0f, moveSpeed, normalizedSpeed);

        //If not firing and fuel is more than 0.
        if (!firing && fuel > 0)
        {
            //If pointInWorld is not null.
            if (pointInWorld != null)
            {
                //Request Path to pointInWorld.
                a_FindPathToPoint(pointInWorld);
            }

            //If pathFound is not null
            if (pathFound != null && !destroyed)
            {
                //Move the tank over the path, at set speed.
                MoveTank(pathFound, speed);
            }

            if (pointInWorld != null)
            {
                a_FaceTurretToPoint(pointInWorld);
            }
        }
    }

    /// <summary>
    ///Follow path to a target (GameObject) at speed (value between 0-1)
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    /// <param name="normalizedSpeed">Speed between 0-1</param>
    /// <param name="heuristic">Chosen heuristic for path finding</param>
    protected void a_FollowPathToPoint(GameObject pointInWorld, float normalizedSpeed, HeuristicMode heuristic)
    {
        //Random position not found.
        randomNodeFound = true;

        //Set speed of tank based on normalized speed.
        float speed = Mathf.Lerp(0f, moveSpeed, normalizedSpeed);

        //If not firing and fuel is more than 0.
        if (!firing && fuel > 0)
        {
            //If pointInWorld is not null.
            if (pointInWorld != null)
            {
                //Request Path to pointInWorld.
                a_FindPathToPoint(pointInWorld, heuristic);
            }

            //If pathFound is not null
            if (pathFound != null && !destroyed)
            {
                //Move the tank over the path, at set speed.
                MoveTank(pathFound, speed);
            }

            if (pointInWorld != null)
            {
                a_FaceTurretToPoint(pointInWorld);
            }
        }
    }

    /// <summary>
    /// Follow path to a random target (GameObject) at speed (value between 0-1)
    /// </summary>
    /// <param name="normalizedSpeed">Speed between 0-1</param>
    protected void a_FollowPathToRandomPoint(float normalizedSpeed)
    {
        //Set speed of tank.
        float speed = Mathf.Lerp(0f, moveSpeed, normalizedSpeed);

        //If tank has found the randomNode.
        if (randomNodeFound)
        {
            StartCoroutine(GeneratingRandomPointInWorld());
            a_FindPathToPoint(randomPoint);
        }

        //Move tank tank if not firing and has fuel.
        if (!firing && fuel > 0 && !destroyed)
        {
            a_FindPathToPoint(randomPoint);
            a_ResetTurret();
            MoveTank(pathFound, speed);
        }

        //Found randomNode if distance is less than 12
        if (Vector3.Distance(transform.position, randomPoint.transform.position) < 14f)
        {
            randomNodeFound = true;
        }
    }

    /// <summary>
    /// Follow path to a random target (GameObject) at speed (value between 0-1)
    /// </summary>
    /// <param name="normalizedSpeed">Speed between 0-1</param>
    protected void a_FollowPathToRandomPoint(float normalizedSpeed, HeuristicMode heuristic)
    {
        //Set speed of tank.
        float speed = Mathf.Lerp(0f, moveSpeed, normalizedSpeed);

        //If tank has found the randomNode.
        if (randomNodeFound)
        {
            StartCoroutine(GeneratingRandomPointInWorld());
            a_FindPathToPoint(randomPoint, heuristic);
        }

        //Move tank tank if not firing and has fuel.
        if (!firing && fuel > 0 && !destroyed)
        {
            a_FindPathToPoint(randomPoint, heuristic);
            a_ResetTurret();
            MoveTank(pathFound, speed);
        }

        //Found randomNode if distance is less than 12
        if (Vector3.Distance(transform.position, randomPoint.transform.position) < 14f)
        {
            randomNodeFound = true;
        }
    }

    /// <summary>
    /// Set another random point.
    /// </summary>
    protected void a_GenerateRandomPoint()
    {
        randomNodeFound = true;
    }

    private IEnumerator GeneratingRandomPointInWorld()
    {
        AStar tempAStar = aStarScript;

        Node randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-90, 90), 0, Random.Range(-90, 90)));
        Vector3 consPos = Vector3.zero;

        while (!randomNode.traversable)
        {
            randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-90, 90), 0, Random.Range(-90, 90)));

            yield return new WaitForEndOfFrame();
        }

        randomNodeFound = false;
        randomPoint.transform.position = randomNode.nodePos;
    }

    private void MoveTank(List<Vector3> path, float speed)
    {
        if (gameControllerScript.gameStarted)
        {
            a_StartTank();
            if (path != null)
            {
                Vector3 centrePos = FindCentre(path);

                TankLookAt(centrePos);

                if (collisionWithObstacle || collisionWithObstacleRock)
                {
                    rb.AddRelativeForce((Vector3.back) * speed, ForceMode.Impulse);
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, tankMaxSpeed * 0.8f);
                }
                else
                {
                    rb.AddRelativeForce(new Vector3(0, 0, 1) * speed, ForceMode.Impulse);
                    //rb.velocity = Vector3.ClampMagnitude(rb.velocity, tankMaxSpeed);

                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, Mathf.SmoothStep(tankMaxSpeed, 0, Mathf.Abs(rb.angularVelocity.x) - 0.75f));
                }



            }
            else if (collisionWithObstacle || collisionWithObstacleRock)
            {
                Vector3 centrePos = FindCentre(path);

                rb.AddRelativeForce((Vector3.back) * speed, ForceMode.Impulse);
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, tankMaxSpeed * 1.5f);
                TankLookAt(centrePos);
            }
            else
            {
                rb.AddRelativeForce(Vector3.forward * speed, ForceMode.Impulse);
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, tankMaxSpeed * 1.5f);
            }
        }

    }

    /// <summary>
    /// Turn tank to position in world.
    /// </summary>
    private void TankLookAt(Vector3 pos)
    {
        Vector3 fwd = transform.forward;
        Vector3 vec = pos - transform.position;
        vec.Normalize();

        angle = Mathf.Acos(Vector3.Dot(fwd, vec)) * Mathf.Rad2Deg;


        if(angle > 100)
        {
            StartCoroutine(SlowTurn());
        }

        if (slowTurn && gameControllerScript.gameStarted)
        {
            transform.LookAt(Vector3.SmoothDamp(transform.position, pos, ref velocityRot, 2.8f));
        }
        else
        {
            transform.LookAt(Vector3.SmoothDamp(transform.position, pos, ref velocityRot, 1.7f));
        }
    }

    IEnumerator SlowTurn()
    {
        slowTurn = true;
        yield return new WaitForSeconds(4);
        slowTurn = false;
    }

    /// <summary>
    /// Stop tank
    /// </summary>
    protected void a_StopTank()
    {
        tankMaxSpeed = tankMaxSpeedHolder * 0.3f;
    }

    /// <summary>
    /// Start tank
    /// </summary>
    protected void a_StartTank()
    {
        tankMaxSpeed = tankMaxSpeedHolder;
    }


    /// <summary>
    /// Face turret to pointInWorld
    /// </summary>
    protected void a_FaceTurretToPoint(GameObject pointInWorld)
    {
        Vector3 faceTarget = new Vector3(pointInWorld.transform.position.x, pointInWorld.transform.position.y, pointInWorld.transform.position.z);
        //faceTarget = Vector3.SmoothDamp(turretObject.transform.position, faceTarget, ref velocityTurretRot, turrentRotationSpeed);
        //turretObject.transform.LookAt(faceTarget);

        Quaternion target_rot = Quaternion.LookRotation(faceTarget - turretObject.transform.position);
        turretObject.transform.rotation = Quaternion.Slerp(turretObject.transform.rotation, target_rot, Time.deltaTime * 5f);


    }

    /// <summary>
    /// Reset turret
    /// </summary>
    protected void a_ResetTurret()
    {
        //turretObject.transform.localRotation = turretStartRot;

        turretObject.transform.localRotation = Quaternion.Slerp(turretObject.transform.localRotation, turretStartRot, Time.deltaTime * 5f);
    }

    /// <summary>
    /// Fire at pointInWorld
    /// </summary>
    protected void a_FireAtPoint(GameObject pointInWorld)
    {
        //Stop Tank.
        a_StopTank();

        //Set RandomNodeFound to true.
        randomNodeFound = true;

        //If not firing and have ammo.
        if (!firing && ammo > 0 && !destroyed)
        {
            //Set firing to true.
            firing = true;
            //Start firing process.
            StopCoroutine("Fire");
            StartCoroutine("Fire", (pointInWorld));
        }
        else if (ammo <= 0)
        {
            //If there is no ammo print message.
            print(this.transform.parent.gameObject.name + " has no Ammo!");
        }
    }

    /// <summary>
    /// Fire at pointInWorld (target)
    /// </summary>
    private IEnumerator Fire(GameObject target)
    {
        //Fire waiting time.
        float tWait = 2f;

        //While waiting time more than 0, and target exists and ammo is more than 0
        while (tWait > 0 && target != null && ammo > 0)
        {
            //Face turret to target.
            a_FaceTurretToPoint(target);
            //decrement wait time.
            tWait -= Time.deltaTime;
            //if ammo is 0, break out of while loop.
            if (ammo == 0)
            {
                break;
            }
            yield return null;
        }

        //Play fire particle 
        fireParticle.Play();
        //Play fire sound.
        if (fireSound.isPlaying)
        {
            fireSound.Stop();
        }
        fireSound.Play();

        //Decrement ammo amount.
        ammo -= 1;

        //Print fire message
        print(this.transform.parent.gameObject.name + " has Fired!");

        //Fine position of turret
        Vector3 turPart = turretObject.transform.Find("TurretPart").position;
        //Position to fire projectile from turret.
        Rigidbody bulletClone = (Rigidbody)Instantiate(projectileObject.GetComponent<Rigidbody>(), new Vector3(turPart.x + 0.55f, turPart.y + 1.7f, turPart.z)
                                                                                     , turretObject.transform.rotation);
        //Allow for projectile to move. 
        bulletClone.isKinematic = false;
        //Add force to bullet projectile.
        bulletClone.AddRelativeForce(projectileForce, ForceMode.Impulse);

        //Wait time set
        tWait = 1f;

        //Wait
        while (tWait > 0)
        {
            tWait -= Time.deltaTime;
            yield return null;
        }

        //Fireing is now false.
        firing = false;
    }

    /// <summary>
    /// Find centre point of first 5 nodes of path
    /// </summary>
    private Vector3 FindCentre(List<Vector3> _path)
    {
        float x = 0;
        float y = 0;
        float z = 0;

        int pathCount = Mathf.Clamp(_path.Count, 0, 4);

        for (int i = 0; i < pathCount; i++)
        {
            x += _path[i].x;
            y += this.transform.position.y;
            z += _path[i].z;
        }

        x = x / pathCount;
        y = y / pathCount;
        z = z / pathCount;

        Vector3 centrePosNew = new Vector3(x, y, z);

        return centrePosNew;
    }

    /// <summary>
    /// Delay sensor 
    /// </summary>
    private IEnumerator TargetsFind(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            targetsFound.Clear();
            consumablesFound.Clear();
            basesFound.Clear();
            FindVisibleTargets();
        }
    }

    /// <summary>
    /// Find visible targets (Tanks, Bases and Consumables).
    /// </summary>
    private void FindVisibleTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(sensorPoint.transform.position, viewRadius, tankMainMask);
        Collider[] consumableInViewRadius = Physics.OverlapSphere(sensorPoint.transform.position, viewRadius, consumableMask);
        Collider[] baseInViewRadius = Physics.OverlapSphere(sensorPoint.transform.position, viewRadius, baseMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;

            Vector3 directionToTarget = (target.transform.position - sensorPoint.transform.position).normalized;

            if (Vector3.Angle(sensorPoint.transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(sensorPoint.transform.position, target.transform.position);

                if (!Physics.Raycast(sensorPoint.transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    if (target != this.gameObject && !targetsFound.ContainsKey(target))
                    {
                        targetsFound.Add(target, distanceToTarget);
                    }
                }
            }
        }

        for (int i = 0; i < consumableInViewRadius.Length; i++)
        {
            GameObject target = consumableInViewRadius[i].gameObject;

            Vector3 directionToTarget = (target.transform.position - sensorPoint.transform.position).normalized;

            if (Vector3.Angle(sensorPoint.transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(sensorPoint.transform.position, target.transform.position);

                if (!Physics.Raycast(sensorPoint.transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    if (target != this.gameObject && !consumablesFound.ContainsKey(target))
                    {
                        consumablesFound.Add(target, distanceToTarget);
                    }
                }
            }
        }

        for (int i = 0; i < baseInViewRadius.Length; i++)
        {
            GameObject target = baseInViewRadius[i].gameObject;

            Vector3 directionToTarget = (target.transform.position - sensorPoint.transform.position).normalized;

            if (Vector3.Angle(sensorPoint.transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(sensorPoint.transform.position, target.transform.position);
                if (!Physics.Raycast(sensorPoint.transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    if (target != this.gameObject && !basesFound.ContainsKey(target) && !myBases.Contains(target))
                    {
                        basesFound.Add(target, distanceToTarget);
                    }
                }
            }
        }
    }

    private Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += sensorPoint.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Visuals and Gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (uiContScript != null)
        {
            if (sensorPoint != null && uiContScript.showSensor)
            {
                Handles.color = Color.white;

                Vector3 sensorAngleA = DirectionFromAngle(-viewAngle / 2, false);
                Vector3 sensorAngleB = DirectionFromAngle(viewAngle / 2, false);

                Handles.DrawLine(sensorPoint.transform.position, sensorPoint.transform.position + sensorAngleA * viewRadius);
                Handles.DrawLine(sensorPoint.transform.position, sensorPoint.transform.position + sensorAngleB * viewRadius);

                Handles.color = Color.red;


                foreach (KeyValuePair<GameObject, float> item in targetsFound)
                {
                    if (item.Key != null)
                    {
                        Handles.DrawLine(sensorPoint.transform.position, item.Key.transform.position);
                    }
                }

                Handles.color = Color.green;


                foreach (KeyValuePair<GameObject, float> item in consumablesFound)
                {
                    if (item.Key != null)
                    {
                        Handles.DrawLine(sensorPoint.transform.position, item.Key.transform.position);
                    }
                }

                Handles.color = Color.blue;


                foreach (KeyValuePair<GameObject, float> item in basesFound)
                {
                    if (item.Key != null)
                    {
                        Handles.DrawLine(sensorPoint.transform.position, item.Key.transform.position);
                    }
                }
            }

            if (uiContScript.showPath)
            {
                foreach (Vector3 node in pathFound)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(node, new Vector3(3 * 0.9f, 0.1f, 3 * 0.9f));
                }
            }
        }
    }

    /// <summary>
    /// Returns if the tank is firing.
    /// </summary>
    protected bool a_IsFiring
    {
        get
        {
            return firing;
        }
    }

    /// <summary>
    /// Returns if the tank is destroyed.
    /// </summary>
    private bool IsDestroyed
    {
        get
        {
            return destroyed;
        }
    }

    /// <summary>
    /// Returns float value of remaining health.
    /// </summary>
    protected float a_GetHealthLevel
    {
        get
        {
            return health;
        }
    }

    /// <summary>
    /// Returns float value of remaining ammo.
    /// </summary>
    protected float a_GetAmmoLevel
    {
        get
        {
            return ammo;
        }
    }

    /// <summary>
    /// Returns float value of remaining fuel.
    /// </summary>
    protected float a_GetFuelLevel
    {
        get
        {
            return fuel;
        }
    }

    /// <summary>
    /// Returns list of friendly bases.
    /// </summary>
    protected List<GameObject> a_GetMyBases
    {
        get
        {
            return myBases;
        }
    }

    /// <summary>
    /// Returns Dictionary(GameObject target, float distance) of visible targets (tanks in TankMain LayerMask).
    /// </summary>
    protected Dictionary<GameObject, float> a_TanksFound
    {
        get
        {
            return targetsFound;
        }
    }

    /// <summary>
    /// Returns Dictionary(GameObject consumable, float distance) of visible consumables (consumables in Consumable LayerMask).
    /// </summary>
    protected Dictionary<GameObject, float> a_ConsumablesFound
    {
        get
        {
            return consumablesFound;
        }
    }

    /// <summary>
    /// Returns Dictionary(GameObject base, float distance) of visible enemy bases (bases in Base LayerMask).
    /// </summary>
    protected Dictionary<GameObject, float> a_BasesFound
    {
        get
        {
            return basesFound;
        }
    }

    /// <summary>
    /// Replaces <c>Start</c> function and should be used when inheriting.
    /// </summary>
    public abstract void AITankStart();

    /// <summary>
    /// Replaces <c>Update</c> function and should be used when inheriting.
    /// </summary>
    public abstract void AITankUpdate();

    /// <summary>
    /// Replaces <c>OnCollisionEnter</c> function and should be used when inheriting.
    /// </summary>
    public abstract void AIOnCollisionEnter(Collision collision);




}




