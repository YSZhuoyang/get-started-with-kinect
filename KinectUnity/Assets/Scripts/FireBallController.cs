using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class FireBallController : MonoBehaviour
{
    private ParticleSystem fireBall;
    private ParticleSystem trailer;
    private Light fireLight;

    // To be deleted
    private ParticleSystem.EmissionModule fireBallEmission;
    private ParticleSystem.EmissionModule trailerEmission;
    private ParticleSystem.Particle[] trailerParticles;

    private Vector3 fireBallPreLoc;
    private Vector3 fireBallCurrLoc;
    private Vector3 trailerCurrVelocity;
    private Vector3 trailerInitVelocity;

    //private Vector3 rightHandVelocity;
    private Vector3 rightHandPosition;
    private Vector3 rightElbowPosition;

    private HandState handRightState;
    private FireBallState fireBallState;

    private enum FireBallState
    {
        flying, holding, extinguished
    }

    // Use this for initialization
    void Start()
    {
        trailer = GameObject.Find("Trailer").GetComponent<ParticleSystem>();
        fireBall = GameObject.Find("FireBall").GetComponent<ParticleSystem>();
        fireLight = GameObject.Find("FireLight").GetComponent<Light>();

        rightHandPosition = new Vector3();
        rightElbowPosition = new Vector3();
        trailerCurrVelocity = new Vector3();
        trailerInitVelocity = new Vector3(0f, 0f, trailer.startSpeed);

        // To be deleted
        fireBallEmission = fireBall.emission;
        trailerEmission = trailer.emission;

        fireBallEmission.enabled = false;
        trailerEmission.enabled = false;
        fireLight.enabled = false;

        fireBallPreLoc = fireBall.transform.position;
        fireBallCurrLoc = fireBall.transform.position;

        fireBallState = FireBallState.extinguished;
        handRightState = HandState.Closed;
    }

    private void EnableFireBall()
    {
        if (!trailerEmission.enabled && 
            !fireBallEmission.enabled && 
            !fireLight.enabled && 
            fireBallState == FireBallState.extinguished)
        {
            fireBallEmission.enabled = true;
            trailerEmission.enabled = true;
            fireLight.enabled = true;
        }

        // Replace the above code after testing
        /*if (GameObject.Find("FireBallEffect") == null &&
            fireBallState == FireBallState.extinguished)
        {
            Instantiate(Resources.Load<GameObject>("FireBallEffect"));
        }*/
    }

    private void DisableFireBall()
    {
        if (trailerEmission.enabled && 
            fireBallEmission.enabled && 
            fireLight.enabled &&
            fireBallState == FireBallState.holding)
        {
            fireBallEmission.enabled = false;
            trailerEmission.enabled = false;
            fireLight.enabled = false;
        }

        // Replace the above code after testing
        /*if (GameObject.Find("FireBallEffect") != null &&
            fireBallState == FireBallState.extinguished)
        {
            Destroy(GameObject.Find("FireBallEffect"));
        }*/
    }

    public void SetGestures(
        Vector3 rightHandPositionIn, 
        Vector3 rightElbowPositionIn, 
        //Vector3 rightHandVelocity, 
        HandState handRightStateIn)
    {
        rightHandPosition = rightHandPositionIn;
        rightElbowPosition = rightElbowPositionIn;
        handRightState = handRightStateIn;
    }

    private void ResponseToGesture()
    {
        if (rightHandPosition.y > rightElbowPosition.y && 
            handRightState == HandState.Open)
        {
            EnableFireBall();
        }

        if (rightElbowPosition.y > rightHandPosition.y && 
            handRightState == HandState.Closed)
        {
            DisableFireBall();
        }

        float fireBallSpeed = Mathf.Sqrt(
            trailerCurrVelocity.x * trailerCurrVelocity.x +
            trailerCurrVelocity.y * trailerCurrVelocity.y +
            trailerCurrVelocity.z * trailerCurrVelocity.z
            );
        
        if (FireBallEnabled() && 
            fireBallState == FireBallState.holding &&
            //Mathf.Abs(rightHandPosition.y - rightElbowPosition.y) < 0.5f &&
            fireBallSpeed > 2 && 
            handRightState == HandState.Open)
        {
            fireBallState = FireBallState.flying;
        }
    }
    
    private void ShootFireBall()
    {
        trailerParticles = new ParticleSystem.Particle[trailer.particleCount];
        int numAlive = trailer.GetParticles(trailerParticles);

        for (int i = 0; i < numAlive; i++)
        {
            trailerParticles[i].velocity = new Vector3(
                trailerInitVelocity.x - trailerCurrVelocity.x * 10f,
                trailerInitVelocity.y + trailerCurrVelocity.z * 10f,
                trailerInitVelocity.z - trailerCurrVelocity.y * 10f);
        }

        trailer.SetParticles(trailerParticles, numAlive);
    }

    private void MoveFireBallWithHand()
    {
        // Compute fireBall moving velocity
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

    private bool FireBallEnabled()
    {
        // Replace with the code below
        //return GameObject.Find("FireBallEffect") != null
        if (trailerEmission.enabled &&
            fireBallEmission.enabled &&
            fireLight.enabled)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResponseToGesture();

        if (fireBallState == FireBallState.holding)
        {
            MoveFireBallWithHand();
        }
        else if (fireBallState == FireBallState.flying)
        {
            ShootFireBall();
        }

        /*fireBallCurrLoc = fireBall.transform.position;
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

        trailer.SetParticles(trailerParticles, numAlive);*/
    }
}
