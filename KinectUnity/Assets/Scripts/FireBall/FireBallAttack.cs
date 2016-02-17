using UnityEngine;
using System.Collections;

public class FireBallAttack : MonoBehaviour
{
    private FireBallController fireBallControllerScript;
    private GameObject fireBallController;

	// Use this for initialization
	void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    void OnCollisionEnter(Collision col)
    {
        Destroy(gameObject);

        fireBallController = GameObject.Find("FireBallController");

        if (fireBallController == null)
        {
            print("Error: FireBallController game object not found");

            return;
        }

        fireBallControllerScript = fireBallController.GetComponent<FireBallController>();

        if (fireBallControllerScript == null)
        {
            print("Error: FireBallController script not found");

            return;
        }

        fireBallControllerScript.SetFireBallState(FireBallController.FireBallState.extinguished);

        // Stop audio playing
        fireBallControllerScript.StopFlyingAudioPlaying();

        if (col.gameObject.tag == "Enemy")
        {
            Destroy(col.gameObject);
            
            // Trigger explosion
            Instantiate(Resources.Load<GameObject>("Explosion"), col.transform.position, Quaternion.identity);
            Instantiate(Resources.Load<GameObject>("Debris"), col.transform.position, Quaternion.identity);
        }
        else
        {
            // Trigger explosion
            Instantiate(Resources.Load<GameObject>("Explosion"), transform.position, Quaternion.identity);
        }
    }
}
