using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>BaseScript</c> handles the Tank Base behaviour, specifically the destruction process.
/// </summary>
public class BaseScript : MonoBehaviour
{
    bool baseHit = false; /*!< <c>baseHit</c> stores if the base has been hit. */
    private ParticleSystem destroyPart; /*!< <c>destroyPart</c> stores particle system for when destroying base */
    private GameObject modelGO; /*!< <c>modelGO</c> stores the base tent model gameobject. */
    private Rigidbody rBody; /*!< <c>rBody</c> stores the base rigid body */
    private BoxCollider bCollider; /*!< <c>bCollider</c> stores base collider. */

    /// <summary>
    /// Function <c>Start</c> initialises the base variables.
    /// </summary>
    private void Start()
    {
        destroyPart = transform.Find("DestroyParticle").GetComponent<ParticleSystem>();
        modelGO = transform.Find("Model").gameObject;
        rBody = GetComponent<Rigidbody>();
        bCollider = GetComponent<BoxCollider>();
        rBody.isKinematic = true;
        bCollider.isTrigger = true;
    }

    /// <summary>
    /// Function <c>OnTriggerEnter</c> checks to see if a collider has entered this gameobject collider, specifically "Projectile"..
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Projectile" && baseHit == false)
        {
            bCollider.isTrigger = false;
            print(this.transform.parent.gameObject.name + " base has been hit!");
            baseHit = true;
            StartCoroutine(BaseDestroyed());
            modelGO.SetActive(false);
        }
    }

    /// <summary>
    /// Function <c>BaseDestroyed</c> has wait time before base is destroyed.
    /// </summary>
    IEnumerator BaseDestroyed()
    {
        destroyPart.Play();
        yield return new WaitForSeconds(0.75f);
        Destroy(this.gameObject);
    }
}
