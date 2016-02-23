using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision col)
    {
        Destroy(gameObject);
        Instantiate(Resources.Load<GameObject>("Debris"), transform.position, Quaternion.identity);
    }
}
