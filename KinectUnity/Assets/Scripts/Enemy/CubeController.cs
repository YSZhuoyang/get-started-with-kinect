using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    private static ushort START_TIME = 3;
    private static ushort MAX_NUMBER = 10;

    private ushort numAlive;

	// Use this for initialization
	void Start()
    {
        numAlive = 0;
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Time.time > START_TIME && numAlive <= MAX_NUMBER)
        {
            Instantiate(
                Resources.Load<GameObject>("Cube"),
                new Vector3(Random.Range(-5f, 5f), 
                            Random.Range(2f, 8f), 
                            Random.Range(-5f, 5f)),
                Quaternion.identity);
        }

        numAlive = (ushort) GameObject.FindGameObjectsWithTag("Enemy").Length;
	}
}
