using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>GameController</c> handles game logic. The class displays the tank stats, 
/// generates random consumables on the playfield and deals with win/loss conditions.
/// </summary>
public class GameController : MonoBehaviour
{
    List<AITank> aiTanks = new List<AITank>(); /*!< <c>aiTanks</c> stores a list of type <c>AITank</c>. These are all tanks in the scene. */

    private float gameStartTime = 3f; /*!< <c>gameStartTime</c> stores the wait time before game starts. */

    [HideInInspector]
    public bool gameStarted = false; /*!< <c>gameStarted</c> stores if the game has been started. */

    private AStar aStar; /*!< <c>aStar</c> stores A star script which stores the playfield as a grid. Used to place consumables. */

    private GameObject healthGameObject; /*!< <c>healthGameObject</c> stores health consumable prefab reference. */
    private GameObject ammoGameObject; /*!< <c>ammoGameObject</c> stores ammo consumable prefab reference. */
    private GameObject fuelGameObject; /*!< <c>fuelGameObject</c> stores fuel consumable prefab reference. */
    private AudioSource consumableAudio;
    private AudioClip conGenAudio;
    private AudioClip healthAudio;
    private AudioClip ammoAudio;
    private AudioClip fuelAudio;
    private AudioClip explodeSound;

    List<GameObject> consumable = new List<GameObject>(); /*!< <c>cameraStartRot</c> stores all consumables as a list */

    public UIControllerScript uiControllerScript; /*!< <c>cameraStartRot</c> stores  reference to <c>UIControllerScript</c> used to handle UI.*/

    /// <summary>
    /// Function <c>Start</c> initialises class variables, sets target framerate, consumables and tank references.
    /// </summary>
    void Start()
    {
        Application.targetFrameRate = 60; /* Locked to avoid issues with fual consumption and physics. */

        healthGameObject = transform.Find("Health").gameObject;
        ammoGameObject = transform.Find("Ammo").gameObject;
        fuelGameObject = transform.Find("Fuel").gameObject;
        consumableAudio = GetComponent<AudioSource>();
        conGenAudio = consumableAudio.clip;
        healthAudio = healthGameObject.GetComponent<AudioSource>().clip;
        ammoAudio = ammoGameObject.GetComponent<AudioSource>().clip;
        fuelAudio = fuelGameObject.GetComponent<AudioSource>().clip;
        explodeSound = transform.Find("Projectile/ExplosionSound").gameObject.GetComponent<AudioSource>().clip;

        aStar = GameObject.Find("AStarPlane").GetComponent<AStar>();

        uiControllerScript.gameObject.SetActive(true);
        uiControllerScript.winsText.text = "";

        consumable.Add(healthGameObject);
        consumable.Add(healthGameObject);
        consumable.Add(healthGameObject);
        consumable.Add(ammoGameObject); 
        consumable.Add(ammoGameObject);
        consumable.Add(fuelGameObject);
        consumable.Add(fuelGameObject);


        foreach (GameObject cons in consumable)
        {
            cons.SetActive(false);
        }

        StartCoroutine(GameStart(gameStartTime));
        StartCoroutine("GenerateRandomConsumable");

        GameObject[] aiTanksTemp = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < aiTanksTemp.Length; i++)
        {
            aiTanks.Add(aiTanksTemp[i].GetComponent<AITank>());
        }

        if(aiTanks.Count > 2)
        {
            uiControllerScript.DisableStatsUi();
        }

    }

    /// <summary>
    /// Function <c>GameStart</c> handles game start wait.
    /// </summary>
    IEnumerator GameStart(float gameStartTime)
    {
        yield return new WaitForSeconds(gameStartTime);
        gameStarted = true;
    }

    /// <summary>
    /// Function <c>GenerateRandomConsumable</c> handles generation of consumables on the field. Consumables are generated at random,
    /// at randome times and in random positions in the playfield.
    /// </summary>
    IEnumerator GenerateRandomConsumable()
    {
        foreach (GameObject cons in consumable)
        {
            cons.SetActive(false);
        }

        yield return new WaitForSeconds(Random.Range(1f, 4f));
        Node randomNode = aStar.NodePositionInGrid(new Vector3(Random.Range(-92, 92), 0, Random.Range(-92, 92)));
        Vector3 consPos = Vector3.zero;


        while (!randomNode.traversable)
        {
            randomNode = aStar.NodePositionInGrid(new Vector3(Random.Range(-95, 95), 0, Random.Range(-95, 95)));

            yield return new WaitForEndOfFrame();
        }


        consPos = randomNode.nodePos;

        int randCons = Random.Range(0, consumable.Count);

        consumable[randCons].transform.position = new Vector3(consPos.x, 3, consPos.z) ;

        yield return new WaitForSeconds(1f);

        consumable[randCons].SetActive(true);
        consumableAudio.clip = conGenAudio;
        consumableAudio.Play();


        yield return new WaitForSeconds(Random.Range(10f, 20f));

        StartCoroutine("GenerateRandomConsumable");
    }

    /// <summary>
    /// Function <c>ConsumableCollection</c> handles collection of consumable. Specifically to avoid coroutine running.
    /// </summary>
    public void ConsumableCollection(string consumableString)
    {
        if(consumableString == "Health")
        {
            consumableAudio.clip = healthAudio;
        }
        else if (consumableString == "Ammo")
        {
            consumableAudio.clip = ammoAudio;
        }
        else if (consumableString == "Fuel")
        {
            consumableAudio.clip = fuelAudio;
        }
        consumableAudio.Play();
        StopCoroutine("GenerateRandomConsumable");
        StartCoroutine("GenerateRandomConsumable");
    }

    /// <summary>
    /// Function <c>Update</c> handles mainly win/loss conditions.
    /// </summary>
    private void Update()
    {
        List<AITank> aiTanksTemp = aiTanks;

        for (int i = 0; i < aiTanksTemp.Count; i++)
        {
            if(aiTanksTemp[i] == null)
            {
                aiTanksTemp.RemoveAt(i);
            }
            if(aiTanksTemp.Count == 1)
            {
                break;
            }
        }

        if(aiTanksTemp.Count == 1 && gameStarted)
        {
            print(aiTanksTemp[0].transform.parent.name + " Wins!");
            gameStarted = false;
            uiControllerScript.winsText.text = aiTanksTemp[0].transform.parent.name + " Wins!";
            consumableAudio.clip = explodeSound;
            consumableAudio.Play();
        }
    }

    /// <summary>
    /// Function <c>SetUIStats</c> handles update of UI.
    /// </summary>
    /// <param name="health">Tan health.</param>
    /// <param name="fuel">Tank fuel level.</param>
    /// <param name="ammo">Tank ammo.</param>
    /// <param name="tank">Reference to Tank object used to capture tank name.</param>
    public void SetUIStats(float health, float fuel, float ammo, AITank tank)
    {
        if(aiTanks.Count == 2)
        {
            if(aiTanks[0] == tank)
            {
                uiControllerScript.tankOneName.text = tank.transform.parent.name;
                uiControllerScript.tankOneHealth.text = "Health: " + health.ToString();
                uiControllerScript.tankOneFuel.text = "Fuel: " + Mathf.Clamp(fuel, 0, fuel).ToString("00.0");
                uiControllerScript.tankOneAmmo.text = "Ammo: " + ammo.ToString();

            }

            if (aiTanks[1] == tank)
            {
                uiControllerScript.tankTwoName.text = tank.transform.parent.name;
                uiControllerScript.tankTwoHealth.text = "Health: " + health.ToString();
                uiControllerScript.tankTwoFuel.text = "Fuel: " + Mathf.Clamp(fuel, 0, fuel).ToString("00.0");
                uiControllerScript.tankTwoAmmo.text = "Ammo: " + ammo.ToString();
            }
        }
    }

}
