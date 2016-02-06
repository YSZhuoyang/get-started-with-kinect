using UnityEngine;
using System.Collections;
using Windows.Kinect;

// Receiving (body frame) data from kinect
public class BodyManager : MonoBehaviour
{
    private const ushort BYTEPERPIXEL = 4;

    private KinectSensor sensor;
    //private BodyFrameReader bodyFramrReader;
    private MultiSourceFrameReader reader;

    private Body[] bodyData;
    private byte[] colorData;
    private ushort[] depthData;
    CameraSpacePoint[] camPoints;

    private uint depthWidth;
    private uint depthHeight;

    CoordinateMapper coordMapper;

    public Body[] GetBodyData()
    {
        return bodyData;
    }

    public byte[] GetColorData()
    {
        return colorData;
    }

    public ushort[] GetDepthData()
    {
        return depthData;
    }

    public uint GetDepthWidth()
    {
        return depthWidth;
    }

    public uint GetDepthHeight()
    {
        return depthHeight;
    }

    public CoordinateMapper GetCoordMapper()
    {
        return coordMapper;
    }

	// Use this for initialization
	void Start ()
    {
        bodyData = null;
        colorData = null;
        depthData = null;
        camPoints = null;

        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            //bodyFramrReader = sensor.BodyFrameSource.OpenReader();
            reader = sensor.OpenMultiSourceFrameReader(
                //FrameSourceTypes.Infrared |
                FrameSourceTypes.Color |
                FrameSourceTypes.Depth |
                //FrameSourceTypes.BodyIndex |
                FrameSourceTypes.Body);

            coordMapper = sensor.CoordinateMapper;

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Read frame data
        if (reader != null)
        {
            var frame = reader.AcquireLatestFrame();
            
            if (frame != null)
            {
                if (bodyData == null)
                {
                    bodyData = new Body[sensor.BodyFrameSource.BodyCount];
                }

                if (colorData == null)
                {
                    FrameDescription colorFD = sensor.ColorFrameSource.FrameDescription;
                    colorData = new byte[colorFD.LengthInPixels * BYTEPERPIXEL];
                }

                if (depthData == null)
                {
                    FrameDescription depthFD = sensor.DepthFrameSource.FrameDescription;
                    depthData = new ushort[depthFD.LengthInPixels * BYTEPERPIXEL];
                    depthWidth = (uint) depthFD.Width;
                    depthHeight = (uint) depthFD.Height;
                }
                
                BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame();
                bodyFrame.GetAndRefreshBodyData(bodyData);
                
                ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame();
                colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);

                DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame();
                depthFrame.CopyFrameDataToArray(depthData);

                bodyFrame.Dispose();
                colorFrame.Dispose();
                depthFrame.Dispose();

                bodyFrame = null;
                colorFrame = null;
                depthFrame = null;
                
                frame = null;
            }
        }
        
        /*if (bodyFramrReader != null)
        {
            var frame = bodyFramrReader.AcquireLatestFrame();

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
        }*/
    }

    void OnApplicationQuit()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        /*if (bodyFramrReader != null)
        {
            bodyFramrReader.Dispose();
            bodyFramrReader = null;
        }*/

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
