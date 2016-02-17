using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{

	// Use this for initialization
	void Start()
    {
	    
	}
	
	// Update is called once per frame
	void Update()
    {
	    if (GameObject.FindGameObjectWithTag("Enemy") == null)
        {
            // Wait for several seconds


            // Generate a new cube
            Instantiate(
                Resources.Load<GameObject>("Cube"),
                new Vector3(Random.Range(0f, 2f), Random.Range(0.2f, 1f), -6f), 
                Quaternion.identity);
        }
	}
}
