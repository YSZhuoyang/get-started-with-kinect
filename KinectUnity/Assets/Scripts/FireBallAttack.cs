using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class FireBallAttack : MonoBehaviour
{
    private GameObject cube;
    private GameObject explosion;
    private GameObject debris;

    // Use this for initialization
    void Start()
    {
        cube = GameObject.FindGameObjectWithTag("Enemy");
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
            Instantiate(Resources.Load<GameObject>("Explosion"), col.transform.position, Quaternion.identity);
            Instantiate(Resources.Load<GameObject>("Debris"), col.transform.position, Quaternion.identity);
            
            // Sleep and destroy explosion
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
	}
}
