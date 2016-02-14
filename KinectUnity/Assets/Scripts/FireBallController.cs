using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class FireBallController : MonoBehaviour
{
    private static float FLYINGDURATION = 3f;

    private float flyingStartTime;

    private GameObject fireBallEffect;
    private ParticleSystem.Particle[] trailerParticles;

    private Vector3 fireBallPreLoc;
    private Vector3 fireBallCurrLoc;
    private Vector3 currVelocity;

    private Vector3 rightHandPosition;
    private Vector3 rightElbowPosition;

    private HandState handRightState;
    public FireBallState fireBallState;

    public enum FireBallState
    {
        flying, holding, extinguished
    }

    // Use this for initialization
    void Start()
    {
        rightHandPosition = new Vector3();
        rightElbowPosition = new Vector3();

        currVelocity = new Vector3();
        fireBallPreLoc = new Vector3();
        fireBallCurrLoc = new Vector3();
        
        fireBallState = FireBallState.extinguished;
        handRightState = HandState.Closed;
    }

    private void EnableFireBall()
    {
        if (!FireBallEnabled() &&
            fireBallState == FireBallState.extinguished)
        {
            fireBallEffect = Instantiate(Resources.Load<GameObject>("FireBallEffect"));
            fireBallState = FireBallState.holding;

            fireBallEffect.transform.position = rightHandPosition;
            fireBallCurrLoc = rightHandPosition;
            fireBallPreLoc = rightHandPosition;
        }
    }

    private void DisableFireBall()
    {
        if (FireBallEnabled() &&
            fireBallState != FireBallState.extinguished)
        {
            Destroy(fireBallEffect);
            fireBallState = FireBallState.extinguished;
        }
    }

    public Vector3 GetVelocity()
    {
        return currVelocity;
    }

    public void SetGestures(
        Vector3 rightHandPositionIn, 
        Vector3 rightElbowPositionIn, 
        HandState handRightStateIn)
    {
        rightHandPosition = rightHandPositionIn;
        rightElbowPosition = rightElbowPositionIn;
        handRightState = handRightStateIn;
    }

    private void ResponseToGesture()
    {
        // Update position
        if (fireBallState == FireBallState.holding)
        {
            fireBallEffect.transform.position = rightHandPosition;
        }
        else if (fireBallState == FireBallState.flying)
        {
            fireBallEffect.transform.position += currVelocity * Time.deltaTime;
        }

        if (rightHandPosition.y > rightElbowPosition.y && 
            handRightState == HandState.Open)
        {
            EnableFireBall();
        }

        if (rightElbowPosition.y > rightHandPosition.y || 
            handRightState == HandState.Closed)
        {
            DisableFireBall();
        }

        float fireBallSpeed = Mathf.Sqrt(
            currVelocity.x * currVelocity.x +
            currVelocity.y * currVelocity.y +
            currVelocity.z * currVelocity.z
            );
        
        if (FireBallEnabled() && 
            fireBallState == FireBallState.holding &&
            //Mathf.Abs(rightHandPosition.y - rightElbowPosition.y) < 0.5f &&
            fireBallSpeed > 2 && 
            handRightState == HandState.Open)
        {
            fireBallState = FireBallState.flying;
            flyingStartTime = Time.time;
        }
    }
    
    private void ComputeVelocity()
    {
        // Compute fireBall moving velocity
        fireBallCurrLoc = fireBallEffect.transform.position;
        currVelocity = (fireBallCurrLoc - fireBallPreLoc) / Time.deltaTime;
        fireBallPreLoc = fireBallCurrLoc;
    }

    private bool FireBallEnabled()
    {
        return GameObject.Find("FireBallEffect(Clone)") != null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResponseToGesture();
        
        if (fireBallState == FireBallState.holding)
        {
            ComputeVelocity();
        }
        else if (fireBallState == FireBallState.flying)
        {
            if (Time.time - flyingStartTime > FLYINGDURATION)
            {
                // Trigger explosion
                Instantiate(Resources.Load<GameObject>("Explosion"), fireBallEffect.transform.position, Quaternion.identity);
                DisableFireBall();
            }
        }
    }

    public void SetFireBallState(FireBallState fireBallStateIn)
    {
        fireBallState = fireBallStateIn;
    }
}
