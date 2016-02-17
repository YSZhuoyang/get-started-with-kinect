using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    private static ushort NUM_LIGHTNINGS = 8;

    private GameObject[] lightnings;

    // Use this for initialization
    void Start()
    {
        lightnings = new GameObject[NUM_LIGHTNINGS];

        for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
        {
            lightnings[i] = Instantiate(Resources.Load<GameObject>("Lightning"));
        }
    }

    // Update is called once per frame
    void Update()
    {
	    
	}
}
