using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System.Collections.Generic;

// Receiving (frame) data from kinect
public class BodyManager : MonoBehaviour
{
    private const ushort BYTEPERPIXEL = 4;

    private GameObject faceController;
    private HDFaceController faceControllerScript;

    private KinectSensor sensor;
    private MultiSourceFrameReader reader;

    private HighDefinitionFaceFrameReader faceReader;
    private HighDefinitionFaceFrameSource faceFrameSource;
    private FaceAlignment faceAlignment;
    private FaceModel faceModel;

    private Body[] bodyData;
    private byte[] colorData;
    private ushort[] depthData;
    private CameraSpacePoint[] camPoints;
    private CameraSpacePoint[] faceCamPoints;

    private uint depthWidth;
    private uint depthHeight;

    private uint colorWidth;
    private uint colorHeight;

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

    public CameraSpacePoint[] GetCameraSpaceData()
    {
        return camPoints;
    }

    public CameraSpacePoint[] GetFacePointCloud()
    {
        return faceCamPoints;
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
        faceCamPoints = null;

        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            // Obtain color, depth, body skeleton data from multi sources reader
            reader = sensor.OpenMultiSourceFrameReader(
                //FrameSourceTypes.Infrared |
                FrameSourceTypes.Color |
                FrameSourceTypes.Depth |
                //FrameSourceTypes.BodyIndex |
                FrameSourceTypes.Body);

            FrameDescription colorFD = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            colorData = new byte[colorFD.LengthInPixels * BYTEPERPIXEL];
            colorWidth = (uint) colorFD.Width;
            colorHeight = (uint) colorFD.Height;

            FrameDescription depthFD = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFD.LengthInPixels];
            depthWidth = (uint) depthFD.Width;
            depthHeight = (uint) depthFD.Height;

            // Get face frame source data
            faceFrameSource = HighDefinitionFaceFrameSource.Create(sensor);
            faceReader = faceFrameSource.OpenReader();

            faceModel = FaceModel.Create();
            faceAlignment = FaceAlignment.Create();

            coordMapper = sensor.CoordinateMapper;

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }

        faceController = GameObject.Find("HDFaceController");
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

                if (bodyFrame != null && bodyData != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodyData);
                    bodyFrame.Dispose();

                    // Assume only one body and one face detected
                    for (int i = 0; i < bodyData.Length; i++)
                    {
                        if (bodyData[i].IsTracked)
                        {
                            if (!faceFrameSource.IsTrackingIdValid)
                            {
                                faceFrameSource.TrackingId = bodyData[i].TrackingId;
                            }
                        }
                    }
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

                    camPoints = new CameraSpacePoint[depthData.Length];
                    coordMapper.MapDepthFrameToCameraSpace(depthData, camPoints);
                }

                // Get faceHD data
                if (faceReader != null)
                {
                    var faceFrame = faceReader.AcquireLatestFrame();

                    if (faceFrame != null && faceFrame.IsFaceTracked)
                    {
                        faceFrame.GetAndRefreshFaceAlignmentResult(faceAlignment);
                        UpdateFaceData();

                        faceFrame.Dispose();
                        faceFrame = null;
                    }
                }

                bodyFrame = null;
                colorFrame = null;
                depthFrame = null;
                frame = null;
            }
        }
    }

    private void UpdateFaceData()
    {
        if (faceModel == null)
        {
            return;
        }

        IList<CameraSpacePoint> facePointList = faceModel.CalculateVerticesForAlignment(faceAlignment);

        faceCamPoints = new CameraSpacePoint[facePointList.Count];
        Vector3[] facePoints = new Vector3[facePointList.Count];
        ColorSpacePoint[] colorPoints = new ColorSpacePoint[facePointList.Count];
        UnityEngine.Color[] colors = new UnityEngine.Color[facePointList.Count];
        uint[] indices = new uint[FaceModel.TriangleIndices.Count];
        FaceModel.TriangleIndices.CopyTo(indices, 0);
        int baseIndex;

        facePointList.CopyTo(faceCamPoints, 0);
        coordMapper.MapCameraPointsToColorSpace(faceCamPoints, colorPoints);
        
        for (int i = 0; i < facePointList.Count; i++)
        {
            facePoints[i].x = faceCamPoints[i].X;
            facePoints[i].y = faceCamPoints[i].Y;
            facePoints[i].z = faceCamPoints[i].Z;
            
            baseIndex = ((int) colorPoints[i].X + (int) colorPoints[i].Y * (int) colorWidth) * 4;

            colors[i].r = colorData[baseIndex] / 255f;
            colors[i].g = colorData[baseIndex + 1] / 255f;
            colors[i].b = colorData[baseIndex + 2] / 255f;
            colors[i].a = 1.0f;
        }
        
        faceControllerScript = faceController.GetComponent<HDFaceController>();

        if (faceControllerScript == null)
        {
            return;
        }

        faceControllerScript.CreateMesh(facePoints, indices, colors);
    }

    void OnApplicationQuit()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        if (faceReader != null)
        {
            faceReader.Dispose();
            faceReader = null;
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
