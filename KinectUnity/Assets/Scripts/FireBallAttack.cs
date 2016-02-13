using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class FireBallAttack : MonoBehaviour
{
    private GameObject cube;
    private ParticleSystem explosion;
    private ParticleSystem sputtering;
    private ParticleSystem debris;

    // Use this for initialization
    void Start()
    {
        cube = GameObject.FindGameObjectWithTag("Enemy");
        explosion = GameObject.Find("Explosion").GetComponent<ParticleSystem>();
        sputtering = GameObject.Find("Sputtering").GetComponent<ParticleSystem>();
        debris = GameObject.Find("Debris").GetComponent<ParticleSystem>();

    }

    void SpawnCube()
    {
        Instantiate(cube, new Vector3(), Quaternion.identity);
        //CancelInvoke();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            print("Collision detected");

            Destroy(gameObject);
            Destroy(col.gameObject);

            // Trigger explosion
            Instantiate(explosion, col.transform.position, Quaternion.identity);

            // Sleep and destroy explosion

            //ParticleSystem.EmissionModule explosionEmission = explosion.emission;
            //ParticleSystem.EmissionModule sputteringEmission = sputtering.emission;
            //ParticleSystem.EmissionModule debrisEmission = debris.emission;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
	}
}
