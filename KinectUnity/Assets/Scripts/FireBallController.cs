using UnityEngine;
using System.Collections;

public class FireBallController : MonoBehaviour
{
    private ParticleSystem trailer;
    private ParticleSystem.Particle[] particles;

    private Vector3 preLoc;
    private Vector3 currLoc;
    private Vector3 velocity;
    private Vector3 initVelocity;

    // Use this for initialization
    void Start()
    {
        preLoc = transform.position;
        currLoc = transform.position;

        trailer = GameObject.Find("Trailer").GetComponent<ParticleSystem>();
        initVelocity = new Vector3(0f, 0f, 0.6f);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        currLoc = transform.position;
        velocity = (currLoc - preLoc) / Time.deltaTime;
        preLoc = currLoc;

        //print("Velocity x: " + velocity.x);
        //print("Velocity y: " + velocity.y);
        //print("Velocity z: " + velocity.z);

        particles = new ParticleSystem.Particle[trailer.particleCount];
        int numAlive = trailer.GetParticles(particles);

        //print("Count: " + trailer.particleCount);
        //print("numAlive: " + numAlive);

        for (int i = 0; i < numAlive; i++)
        {
            particles[i].velocity = new Vector3(
                initVelocity.x - velocity.x * 0.5f, 
                initVelocity.y + velocity.z * 0.5f, 
                initVelocity.z - velocity.y * 0.5f);
        }

        trailer.SetParticles(particles, numAlive);
    }
}
