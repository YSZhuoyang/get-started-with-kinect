using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodyStub : MonoBehaviour
{
    private GameObject fireBallController;
    private FireBallController fireBallControllerScript;

    // Use this for initialization
    void Start ()
    {
        fireBallController = GameObject.Find("FireBallController");
    }

    // Update is called once per frame
    void Update ()
    {
        if (fireBallController == null)
        {
            return;
        }

        fireBallControllerScript = fireBallController.GetComponent<FireBallController>();

        if (fireBallController == null)
        {
            print("Error: FireBallController script not found");

            return;
        }

        fireBallControllerScript.SetGestures(
            new Vector3(0f, 5f - Time.time * 2f, 1f),
            new Vector3(0f, 0f, 0f),
            HandState.Open);
    }
}
