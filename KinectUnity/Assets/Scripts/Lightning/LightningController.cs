using UnityEngine;
using Windows.Kinect;
using System.Collections;

public class LightningController : MonoBehaviour
{
    private static ushort NUM_LIGHTNINGS = 8;
    private static float SLEEPING_PERIOD = 0.5f;

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

    private void EnableLightning()
    {
        if (!IsLightningEnabled() &&
            lightningState == LightningState.extinguished)
        {
            lightningBall = (GameObject) Instantiate(
                Resources.Load<GameObject>("LightningBall"), 
                leftHandPosition, 
                Quaternion.identity);

            for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
            {
                lightnings[i] = Instantiate(Resources.Load<GameObject>("Lightning"));
                lightnings[i].GetComponent<LightningAttack>().SetStartPosAndEndPos(
                    leftHandPosition, 
                    leftHandPosition + (leftHandPosition - leftElbowPosition) * 8f);
            }

            lightningState = LightningState.exploding;
        }
    }

    private void DisableLightning()
    {
        if (IsLightningEnabled() &&
            lightningState != LightningState.extinguished)
        {
            for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
            {
                Destroy(lightnings[i]);
            }

            lightningState = LightningState.extinguished;
        }
    }

    private void ResponseToGesture()
    {
        if (Mathf.Abs(leftHandPosition.y - leftElbowPosition.y) < 1f &&
            handLeftState == HandState.Open &&
            lightningState == LightningState.extinguished)
        {
            EnableLightning();
        }

        if ((Mathf.Abs(leftHandPosition.y - leftElbowPosition.y) > 1f ||
            handLeftState == HandState.Closed) &&
            lightningState == LightningState.exploding)
        {
            DisableLightning();
        }
    }

    private bool IsLightningEnabled()
    {
        return GameObject.Find("Lightning(Clone)") != null;
    }

    // Update is called once per frame
    void Update()
    {
        ResponseToGesture();

        if (lightningState == LightningState.holding)
        {
            // Update fire ball position based on right hand position
            //fireBallEffect.transform.position = fireBallCurrLoc;
        }
        else if (lightningState == LightningState.exploding)
        {
            for (ushort i = 0; i < NUM_LIGHTNINGS; i++)
            {
                lightnings[i].GetComponent<LightningAttack>().SetStartPosAndEndPos(
                    leftHandPosition,
                    leftHandPosition + (leftHandPosition - leftElbowPosition) * 4f);
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
