using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class CubeController : MonoBehaviour
{
    private KinectSensor sensor;
    private BodyFrameReader reader;
    private Body[] bodyData = null;
    
	// Use this for initialization
	void Start()
    {
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

    // Update is called once per frame
    void Update()
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

                int idx = -1;

                for (int i = 0; i < sensor.BodyFrameSource.BodyCount; i++)
                {
                    //print("trached idx: " + idx);

                    if (bodyData[i].IsTracked)
                    {
                        idx = i;
                    }
                }

                if (idx > -1)
                {
                    //print("idx: " + idx);

                    /*if (bodyData[idx].HandRightState != HandState.Closed)
                    {
                        float posRightHandX = (float)(bodyData[idx].Joints[JointType.HandRight].Position.X);
                        float posRightHandY = (float)(bodyData[idx].Joints[JointType.HandRight].Position.Y);
                        //float posRightHandZ = (float)(bodyData[idx].Joints[JointType.HandRight].Position.Z);
                        
                        transform.position = new Vector3(
                            transform.position.x + posRightHandX * 0.1f,
                            transform.position.y + posRightHandY * 0.1f,
                            transform.position.z);
                    }*/

                    if (bodyData[idx].HandLeftState != HandState.Closed)
                    {
                        float posLeftHandX = (float)(bodyData[idx].Joints[JointType.HandLeft].Position.X);
                        float posLeftHandY = (float)(bodyData[idx].Joints[JointType.HandLeft].Position.Y);
                        float posLeftHandZ = (float)(bodyData[idx].Joints[JointType.HandLeft].Position.Z);

                        this.gameObject.transform.rotation = Quaternion.Euler(
                            this.gameObject.transform.rotation.x + posLeftHandX * 10f,
                            this.gameObject.transform.rotation.y + posLeftHandY * 10f,
                            this.gameObject.transform.rotation.z + posLeftHandZ * 10f);
                    }
                }
            }
        }
	}
}
