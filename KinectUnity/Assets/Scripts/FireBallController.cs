using UnityEngine;
using System.Collections;

public class FireBallController : MonoBehaviour
{
    private ParticleSystem fireBall;
    private ParticleSystem trailer;
    private Light fireLight;
    private ParticleSystem.EmissionModule fireBallEmission;
    private ParticleSystem.EmissionModule trailerEmission;
    private ParticleSystem.Particle[] trailerParticles;

    private Vector3 fireBallPreLoc;
    private Vector3 fireBallCurrLoc;
    private Vector3 trailerCurrVelocity;
    private Vector3 trailerInitVelocity;

    private Vector3 rightHandPosition;
    private Vector3 rightElbowPosition;

    // Use this for initialization
    void Start()
    {
        trailer = GameObject.Find("Trailer").GetComponent<ParticleSystem>();
        fireBall = GameObject.Find("FireBall").GetComponent<ParticleSystem>();
        fireLight = GameObject.Find("FireLight").GetComponent<Light>();

        rightHandPosition = new Vector3();
        rightElbowPosition = new Vector3();
        trailerInitVelocity = new Vector3(0f, 0f, trailer.startSpeed);

        fireBallEmission = fireBall.emission;
        trailerEmission = trailer.emission;

        fireBallEmission.enabled = false;
        trailerEmission.enabled = false;
        fireLight.enabled = false;

        fireBallPreLoc = fireBall.transform.position;
        fireBallCurrLoc = fireBall.transform.position;
    }

    private void EnableFireBall()
    {
        if (!trailerEmission.enabled && 
            !fireBallEmission.enabled && 
            !fireLight.enabled)
        {
            fireBallEmission.enabled = true;
            trailerEmission.enabled = true;
            fireLight.enabled = true;
        }
    }

    private void DisableFireBall()
    {
        if (trailerEmission.enabled && 
            fireBallEmission.enabled && 
            fireLight.enabled)
        {
            fireBallEmission.enabled = false;
            trailerEmission.enabled = false;
            fireLight.enabled = false;
        }
    }

    public void SetGestures(Vector3 rightHandPositionIn, Vector3 rightElbowPositionIn)
    {
        rightHandPosition = rightHandPositionIn;
        rightElbowPosition = rightElbowPositionIn;
    }

    public void ResponseToGesture()
    {
        if (rightHandPosition.y > rightElbowPosition.y)
        {
            EnableFireBall();
        }

        if (rightElbowPosition.y > rightHandPosition.y)
        {
            DisableFireBall();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResponseToGesture();

        fireBallCurrLoc = fireBall.transform.position;
        trailerCurrVelocity = (fireBallCurrLoc - fireBallPreLoc) / Time.deltaTime;
        fireBallPreLoc = fireBallCurrLoc;
        
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
