using UnityEngine;
using System.Collections;

public class FireBallController : MonoBehaviour
{
    private ParticleSystem fireBall;
    private ParticleSystem trailer;
    private ParticleSystem.Particle[] trailerParticles;

    private Vector3 fireBallPreLoc;
    private Vector3 fireBallCurrLoc;
    private Vector3 trailerCurrVelocity;
    private Vector3 trailerInitVelocity;

    // Use this for initialization
    void Start()
    {
        trailer = GameObject.Find("Trailer").GetComponent<ParticleSystem>();
        fireBall = GameObject.Find("FireBall").GetComponent<ParticleSystem>();

        fireBallPreLoc = fireBall.transform.position;
        fireBallCurrLoc = fireBall.transform.position;
        trailerInitVelocity = new Vector3(0f, 0f, 0.6f);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        fireBallCurrLoc = fireBall.transform.position;
        trailerCurrVelocity = (fireBallCurrLoc - fireBallPreLoc) / Time.deltaTime;
        fireBallPreLoc = fireBallCurrLoc;
        
        // Test explosion
        if (fireBall.isPlaying && (trailerCurrVelocity.z > 20f || trailerCurrVelocity.z < -20f))
        {
            fireBall.Stop();
            trailer.Stop();
        }

        /*
        if (fireBall.isStopped && (trailerCurrVelocity.z > 20f || trailerCurrVelocity.z < -20f))
        {
            fireBall.Play();
            trailer.Play();
        }*/

        //print("Velocity x: " + trailerCurrVelocity.x);
        //print("Velocity y: " + trailerCurrVelocity.y);
        //print("Velocity z: " + trailerCurrVelocity.z);

        trailerParticles = new ParticleSystem.Particle[trailer.particleCount];
        int numAlive = trailer.GetParticles(trailerParticles);

        //print("Count: " + trailer.particleCount);
        //print("numAlive: " + numAlive);

        for (int i = 0; i < numAlive; i++)
        {
            trailerParticles[i].velocity = new Vector3(
                trailerInitVelocity.x - trailerCurrVelocity.x * 0.5f,
                trailerInitVelocity.y + trailerCurrVelocity.z * 0.5f,
                trailerInitVelocity.z - trailerCurrVelocity.y * 0.5f);
        }

        trailer.SetParticles(trailerParticles, numAlive);
    }
}
