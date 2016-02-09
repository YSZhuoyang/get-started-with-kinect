using UnityEngine;
using System.Collections;
using Windows.Kinect;

// Receiving (frame) data from kinect
public class BodyManager : MonoBehaviour
{
    private const ushort BYTEPERPIXEL = 4;

    private KinectSensor sensor;
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
            reader = sensor.OpenMultiSourceFrameReader(
                //FrameSourceTypes.Infrared |
                FrameSourceTypes.Color |
                FrameSourceTypes.Depth |
                //FrameSourceTypes.BodyIndex |
                FrameSourceTypes.Body);

            FrameDescription colorFD = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            colorData = new byte[colorFD.LengthInPixels * BYTEPERPIXEL];

            FrameDescription depthFD = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFD.LengthInPixels];
            depthWidth = (uint)depthFD.Width;
            depthHeight = (uint)depthFD.Height;

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
        // Read frame data from multiple sources
        if (reader != null)
        {
            var frame = reader.AcquireLatestFrame();
            
            if (frame != null)
            {
                if (bodyData == null)
                {
                    bodyData = new Body[sensor.BodyFrameSource.BodyCount];
                }
                
                BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame();

                if (bodyData != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodyData);
                    bodyFrame.Dispose();
                }
                
                ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame();

                if (colorFrame != null)
                {
                    colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
                    colorFrame.Dispose();
                }

                DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame();

                if (depthFrame != null)
                {
                    depthFrame.CopyFrameDataToArray(depthData);
                    depthFrame.Dispose();
                }
                
                bodyFrame = null;
                colorFrame = null;
                depthFrame = null;
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
