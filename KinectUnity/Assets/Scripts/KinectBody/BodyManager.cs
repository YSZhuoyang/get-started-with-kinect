using UnityEngine;
using System.Collections;
using Windows.Kinect;

// Receiving (body frame) data from kinect
public class BodyManager : MonoBehaviour
{
    private KinectSensor sensor;
    private BodyFrameReader reader;
    private Body[] bodyData;

    public Body[] GetBodyData()
    {
        return bodyData;
    }

	// Use this for initialization
	void Start ()
    {
        bodyData = null;
        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            reader = sensor.BodyFrameSource.OpenReader();

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (reader != null)
        {
            var frame = reader.AcquireLatestFrame();

            if (frame != null)
            {
                if (bodyData == null)
                {
                    bodyData = new Body[sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(bodyData);
                frame.Dispose();
                frame = null;
            }
        }
	}

    void OnApplicationQuit()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            sensor = null;
        }
    }
}
