using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour
{
    private static float LIFETIME = 2f;

    private float time;

    // Use this for initialization
    void Start()
    {
        time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - time > LIFETIME)
        {
            Destroy(gameObject);
            Instantiate(Resources.Load<GameObject>("Debris"), transform.position, Quaternion.identity);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Destroy(gameObject);
        Instantiate(Resources.Load<GameObject>("Debris"), transform.position, Quaternion.identity);
    }
}
