using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>Projectile</c> handles the projectiles fired from the tanks, primarily visuals and sound.
/// </summary>
public class Projectile : MonoBehaviour
{
    public GameObject projectile; /*!< <c>projectile</c> stores projectile prefab. */
    public ParticleSystem explosionParticle; /*!< <c>explosionParticle</c> stores reference of particleFX. */
    public AudioSource explosionSound; /*!< <c>explosionSound</c> stores reference of particleFX . */
    float t = 0.02f; /*!< <c>t</c> stores wait time value. */
    Rigidbody rb; /*!< <c>rb</c> stores projectile <c>Rigidbody</c> component reference. */
    bool destroy = false; /*!< <c>destroy</c> stores if the projectile is to be destroyed in scene. */

    /// <summary>
    /// Function <c>Awake</c> initialises class variables.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        if (!rb.isKinematic)
        {
            t -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Function <c>OnCollisionEnter</c> deals with projectile collision with any collider.
    /// </summary>
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(t < 0 && destroy == false)
    //    {
    //        explosionParticle.Play();
    //        rb.isKinematic = true;
    //        projectile.SetActive(false);
    //        StartCoroutine(DestoryThis());
    //        destroy = true;
    //        explosionSound.Play();
    //    }

    //}

    private void OnTriggerEnter(Collider other)
    {
        if (t < 0 && destroy == false)
        {
            explosionParticle.Play();
            rb.isKinematic = true;
            projectile.SetActive(false);
            StartCoroutine(DestoryThis());
            destroy = true;
            explosionSound.pitch = Random.Range(0.3f,0.4f);
            explosionSound.Play();
        }
    }

    /// <summary>
    /// Function <c>DestoryThis</c> destroying the collider and removal from scene.
    /// </summary>
    IEnumerator DestoryThis()
    {
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
