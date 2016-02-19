using UnityEngine;
using Windows.Kinect;
using System.Collections;

public class LightningController : MonoBehaviour
{
    private static ushort NUM_LIGHTNINGS = 8;
    private static float DISTANCE_BETWEEN_ELBOW_AND_HAND_Y = 0.4f;

    private Vector3 leftHandPosition;
    private Vector3 leftElbowPosition;

    private GameObject lightningBall;
    private GameObject[] lightnings;
    private UnityEngine.AudioSource lightningAudio;

    private HandState handLeftState;
    private LightningState lightningState;

    public enum LightningState
    {
        exploding, holding, extinguished, sleeping
    }

    // Use this for initialization
    void Start()
    {
        leftHandPosition = new Vector3();
        leftElbowPosition = new Vector3();
        
        lightnings = new GameObject[NUM_LIGHTNINGS];
        lightningAudio = GetComponent<UnityEngine.AudioSource>();

        handLeftState = HandState.Closed;
        lightningState = LightningState.extinguished;
    }

    private void EnableLightningBall()
    {
        if (!IsLightningBallEnabled() &&
            lightningState == LightningState.extinguished)
        {
            lightningBall = (GameObject) Instantiate(
                Resources.Load<GameObject>("LightningBall"), 
                leftHandPosition, 
                Quaternion.identity);
        }
    }

    private void DisableLightningBall()
    {
        if (IsLightningBallEnabled() &&
            lightningState != LightningState.extinguished)
        {
            Destroy(lightningBall);
        }
    }

    private void EnableLightning()
    {
        if (!IsLightningEnabled() && 
            lightningState == LightningState.holding)
        {
            for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
            {
                lightnings[i] = Instantiate(Resources.Load<GameObject>("Lightning"));
                lightnings[i].GetComponent<LightningAttack>().SetStartPosAndEndPos(
                    leftHandPosition,
                    leftHandPosition + (leftHandPosition - leftElbowPosition) * 15f);
            }
        }
    }

    private void DisableLightning()
    {
        if (IsLightningEnabled() &&
            lightningState == LightningState.exploding)
        {
            for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
            {
                Destroy(lightnings[i]);
            }
        }
    }

    private void ResponseToGesture()
    {
        if (leftHandPosition.y > (leftElbowPosition.y - DISTANCE_BETWEEN_ELBOW_AND_HAND_Y) && 
            handLeftState == HandState.Open)
        {
            EnableLightningBall();
            lightningState = LightningState.holding;

            if (Mathf.Abs(leftHandPosition.y - leftElbowPosition.y) < DISTANCE_BETWEEN_ELBOW_AND_HAND_Y)
            {
                EnableLightning();
                lightningState = LightningState.exploding;
            }
            else if (lightningState == LightningState.exploding)
            {
                DisableLightning();
                lightningState = LightningState.holding;
            }
        }
        
        if (leftHandPosition.y < (leftElbowPosition.y - DISTANCE_BETWEEN_ELBOW_AND_HAND_Y) ||
            handLeftState == HandState.Closed)
        {
            DisableLightning();
            DisableLightningBall();
            lightningState = LightningState.extinguished;
        }
    }

    private bool IsLightningBallEnabled()
    {
        return GameObject.Find("LightningBall(Clone)") != null;
    }

    private bool IsLightningEnabled()
    {
        return GameObject.Find("Lightning(Clone)") != null;
    }

    // Update is called once per frame
    void Update()
    {
        ResponseToGesture();

        if (lightningState != LightningState.extinguished)
        {
            lightningBall.transform.position = leftHandPosition;

            if (lightningState == LightningState.exploding)
            {
                for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
                {
                    lightnings[i].GetComponent<LightningAttack>().SetStartPosAndEndPos(
                        leftHandPosition,
                        leftHandPosition + (leftHandPosition - leftElbowPosition) * 4f);
                }
            }
        }
    }

    public void SetLightningState(LightningState lightningStateIn)
    {
        lightningState = lightningStateIn;
    }

    public void SetGestures(
        Vector3 leftHandPositionIn,
        Vector3 leftElbowPositionIn,
        HandState handLeftStateIn)
    {
        leftHandPosition = leftHandPositionIn;
        leftElbowPosition = leftElbowPositionIn;
        handLeftState = handLeftStateIn;
    }
}
