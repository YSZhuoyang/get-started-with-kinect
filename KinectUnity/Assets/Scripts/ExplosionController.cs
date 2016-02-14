using UnityEngine;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
    private static float LIFETIME = 2f;
    private float startTime;

	// Use this for initialization
	void Start ()
    {
        startTime = Time.time;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameObject.Find("Explosion(Clone)") != null && Time.time  - startTime > LIFETIME)
        {
            Destroy(gameObject);
        }
	}
}
