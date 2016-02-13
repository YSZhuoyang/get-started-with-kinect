using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class FireBallAttack : MonoBehaviour
{
    private GameObject explosion;
    private GameObject debris;

    // Use this for initialization
    void Start()
    {
        
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
            
            // Destroy explosion
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
	}
}
