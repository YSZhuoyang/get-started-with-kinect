using UnityEngine;
using System.Collections;

public class FireBallTrailerController : MonoBehaviour
{
    private static float TRAILERSTARTSPEED = 2f;
    
    private Vector3 trailerCurrVelocity;
    private Vector3 trailerInitVelocity;

    private ParticleSystem trailer;
    private ParticleSystem.Particle[] trailerParticles;

    private FireBallController fireBallControllerScript;

    // Use this for initialization
    void Start ()
    {
        trailerInitVelocity = new Vector3(0f, 0f, TRAILERSTARTSPEED);

        trailer = GameObject.Find("Trailer").GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        fireBallControllerScript = GameObject.Find("FireBallController").GetComponent<FireBallController>();

        if (fireBallControllerScript == null)
        {
            return;
        }

        trailerCurrVelocity = fireBallControllerScript.GetVelocity();

        trailerParticles = new ParticleSystem.Particle[trailer.particleCount];
        int numAlive = trailer.GetParticles(trailerParticles);

        for (int i = 0; i < numAlive; i++)
        {
            trailerParticles[i].velocity = new Vector3(
                trailerInitVelocity.x - trailerCurrVelocity.x,
                trailerInitVelocity.y + trailerCurrVelocity.z,
                trailerInitVelocity.z - trailerCurrVelocity.y);
        }

        trailer.SetParticles(trailerParticles, numAlive);
    }
}
