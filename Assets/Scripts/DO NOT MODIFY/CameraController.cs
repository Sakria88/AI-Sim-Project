using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Class <c>CameraController</c> handles the camera in the scene. Handles following the tanks or top view mode.
/// </summary>
public class CameraController : MonoBehaviour
{
    List<AITank> aiTanks = new List<AITank>(); /*!< <c>aiTanks</c> stores list of tanks in the scene. */
    int currentlyVeiwing = 0; /*!< <c>currentlyVeiwing</c> stores which tanks is currently being viewed. */

    Vector3 cameraStartPos; /*!< <c>cameraStartPos</c> stores camera start position. */
    Quaternion cameraStartRot; /*!< <c>cameraStartRot</c> stores camera start rotation. */
    float startFOV;
    Camera mainCam; /*!< <c>mainCam</c> stores reference to the camera in the scene. */
    public float distance; /*!< <c>distance</c> stores the distance away from the target tanks. */
    public float maxDriftRange; /*!< <c>maxDriftRange</c> stores how far are we allowed to drift from the target position. */
    public float angleX; /*!< <c>angleX</c> stores angle to pitch up on top of the target. */  
    public float angleY; /*!< <c>angleY</c> stores angle to yaw around the target. */    
    public GameObject mapCam; /*!< <c>mapCam</c> stores the minimap camera object. */

    private Transform m_transform_cache;    //cache for our transform component

    public Transform target; /*!< <c>target</c> stores the target when following a single target. */
    public float smoothTime = 0.3F; /*!< <c>smoothTime</c> stores the smoothing/dampening level. */
    private Vector3 velocity = Vector3.zero; /*!< <c>velocity</c> stores the velocity. */

    /// <summary>
    /// Function <c>Start</c> initialises class variables, sets default settings for the camera and finds all tanks in the scene.
    /// </summary>
    void Start()
    {
        mainCam = GetComponent<Camera>();
        RenderSettings.fog = false;
        cameraStartPos = transform.position;
        cameraStartRot = transform.localRotation;
        startFOV = mainCam.fieldOfView;
        GameObject[] aiTanksTemp = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < aiTanksTemp.Length; i++)
        {
            aiTanks.Add(aiTanksTemp[i].GetComponent<AITank>());
        }
    }

    /// <summary>
    /// Function <c>LateUpdate</c> used mainly to avoid camera jitter. Also checks to see space bar press for camera view change.
    /// </summary>
    void LateUpdate()
    {
        if (currentlyVeiwing != 0 && currentlyVeiwing <= aiTanks.Count)
        {
            if(aiTanks[currentlyVeiwing-1] != null)
            {
                Vector3 targetPos = GetTargetPos();
                //calculate drift theta
                float t = Vector3.Distance(myTransform.position, targetPos) / maxDriftRange;

                //smooth camera position using drift theta
                myTransform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
                //look at our targetPos
                myTransform.LookAt(target);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {

            if(currentlyVeiwing == 0)
            {
                currentlyVeiwing = 1;
                RenderSettings.fog = false;
                mainCam.transform.position = cameraStartPos;
                mainCam.transform.rotation = cameraStartRot;
            }

            else if(currentlyVeiwing < aiTanks.Count)
            {
                currentlyVeiwing++;
                RenderSettings.fog = true;
            }
            else if (currentlyVeiwing == aiTanks.Count)
            {
                currentlyVeiwing = 3;
                RenderSettings.fog = false;
                mainCam.transform.position = cameraStartPos;
                mainCam.transform.rotation = cameraStartRot;
                mainCam.fieldOfView = startFOV;
            }
            else if (currentlyVeiwing == 3)
            {
                currentlyVeiwing = 0;
            }
        }



        CameraView();
    }

    /// <summary>
    /// Function <c>CameraView</c> sets the camera view. 0 sets camera to default topdown position, else follows a selected target tank.
    /// </summary>
    void CameraView()
    {
        if(currentlyVeiwing == 0)
        {
            tankTargets.Clear();
            foreach (var ai in aiTanks)
            {
                if(ai != null)
                {
                    tankTargets.Add(ai.transform);
                }
            }

            foreach (var cons in consTargets)
            {
                if(cons.activeInHierarchy)
                {
                    tankTargets.Add(cons.transform);
                }
            }

            if (tankTargets.Count == 0)
            {
                return;
            }
            
            Move();
            Zoom();

            mapCam.SetActive(false);
        }
        else if(currentlyVeiwing <= aiTanks.Count)
        {
            if (aiTanks[currentlyVeiwing - 1] != null)
            {
                target = aiTanks[currentlyVeiwing - 1].gameObject.transform;
            }
            else if (aiTanks[currentlyVeiwing - 1] == null)
            {
                aiTanks.RemoveAt(currentlyVeiwing - 1);
                currentlyVeiwing = aiTanks.Count - 1;
            }
            mapCam.SetActive(true);

        }
        else
        {
            mainCam.transform.position = cameraStartPos;
            mainCam.transform.rotation = cameraStartRot;
            mapCam.SetActive(false);

        }
    }

    /// <summary>
    /// Function <c>myTransform</c> gets the camera transform.
    /// </summary>
    /// <returns>Transform</returns>
    private Transform myTransform
    {//use this instead of transform
        get
        {//myTransform is guarunteed to return our transform component, but faster than just transform alone
            if (m_transform_cache == null)
            {//if we don't have it cached, cache it
                m_transform_cache = transform;
            }
            return m_transform_cache;
        }
    }

    /// <summary>
    /// Function <c>OnValidate</c> runs when values are changed in the inspector.
    /// </summary>
    void OnValidate()
    {
        if (target != null)
        {//we have a target, move the camera to target position for preview purposes
            Vector3 targetPos = GetTargetPos();
            //update position
            myTransform.position = targetPos;
            //look at our target
            myTransform.LookAt(target);
        }
    }

    /// <summary>
    /// Function <c>GetTargetPos</c> adds angular offset to targetPosition.
    /// </summary>
    /// <returns>Vector3 target position.</returns>
    private Vector3 GetTargetPos()
    {
        if (target != null)
        {
            //returns where the camera should aim to be
            //opposite of (-forward) * distance
            Vector3 targetPos = new Vector3(0, 0, -distance);
            //calculate pitch and yaw
            targetPos = Quaternion.Euler(angleX, angleY, 0) * targetPos;
            //return angled target position relative to target.position
            return target.position + (target.rotation * targetPos);
        }
        return Vector3.zero;
    }



    public List<Transform> tankTargets;
    public List<GameObject> consTargets;


    public Vector3 offset;
    public float smoothTimeTargets = .5f;

    public float minZoom = 40f;
    public float maxZoom = 10f;
    public float zoomLimiter = 50f;

    private Vector3 velocityTargets;



    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, newZoom, Time.deltaTime);
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(tankTargets[0].position, Vector3.zero);
        for (int i = 0; i < tankTargets.Count; i++)
        {

            bounds.Encapsulate(tankTargets[i].position);
        }
        return bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (tankTargets.Count == 1)
        {
            return tankTargets[0].position;
        }

        var bounds = new Bounds(tankTargets[0].position, Vector3.zero);
        for (int i = 0; i < tankTargets.Count; i++)
        {
            bounds.Encapsulate(tankTargets[i].position);
        }

        return bounds.center;
    }


}

