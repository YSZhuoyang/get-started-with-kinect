using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;


public class BodyController : MonoBehaviour
{
    private GameObject bodyManager;
    private GameObject fireBallController;

    private GameObject jointHead;
    private GameObject jointNeck;
    private GameObject jointSpineUpper;
    private GameObject jointSpineMiddle;
    private GameObject jointSpineLower;
    private GameObject jointSpineBase;
    private GameObject jointLegThighLeft;
    private GameObject jointLegThighRight;
    private GameObject jointLegKneeLeft;
    private GameObject jointLegKneeRight;
    private GameObject jointLegAnkleLeft;
    private GameObject jointLegAnkleRight;
    //private GameObject jointLegToesLeft;
    //private GameObject jointLegToseRight;
    private GameObject jointArmShoulderUpperLeft;
    private GameObject jointArmShoulderUpperRight;
    private GameObject jointArmShoulderLowerLeft;
    private GameObject jointArmShoulderLowerRight;
    private GameObject jointArmElbowLeft;
    private GameObject jointArmElbowRight;
    private GameObject jointArmWristLeft;
    private GameObject jointArmWristRight;

    private BodyManager bodyManagerScript;
    private FireBallController fireBallControllerScript;
    private CoordinateMapper coordMapper;

    private Body[] bodyData;
    private byte[] colorData;
    private ushort[] depthData;
    private CameraSpacePoint[] camPoints;

    //private Vector3 rightHandPreLoc;
    //private Vector3 rightHandCurrLoc;

    private uint depthWidth;
    private uint depthHeight;

    //private Dictionary<ulong, GameObject> bodyMap = new Dictionary<ulong, GameObject>();

    // <child, parent> Note that the joint data from kinect is different from that 
    // in the avatars used in Unity (e.g. the orientation data of parent joint from 
    // kinect maps to the orientation data of child joint of avatars), the orientation 
    // data obtained from kinect is absolute orientation data

    // Used to get relevant orientation
    /*private Dictionary<JointType, JointType> jointHierarchy = new Dictionary<JointType, JointType>()
    {
        { JointType.HipRight ,JointType.SpineBase },
        { JointType.KneeRight, JointType.HipRight },
        { JointType.AnkleRight, JointType.KneeRight },
        { JointType.FootRight, JointType.AnkleRight },

        { JointType.HipLeft, JointType.SpineBase },
        { JointType.KneeLeft, JointType.HipLeft },
        { JointType.AnkleLeft, JointType.KneeLeft },
        { JointType.FootLeft, JointType.AnkleLeft },

        { JointType.SpineMid, JointType.SpineBase },
        { JointType.SpineShoulder, JointType.SpineMid },

        { JointType.Neck, JointType.SpineShoulder },
        { JointType.Head, JointType.Neck },

        { JointType.ShoulderRight, JointType.SpineShoulder },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.HandTipRight, JointType.HandRight },
        { JointType.ThumbRight, JointType.WristRight },

        { JointType.ShoulderLeft, JointType.SpineShoulder },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.WristLeft, JointType.ElbowLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.HandTipLeft, JointType.HandLeft },
        { JointType.ThumbLeft, JointType.WristLeft },
    };*/

    void Start()
    {
        bodyManager = GameObject.Find("BodyManager");
        fireBallController = GameObject.Find("FireBallController");

        jointHead = GameObject.Find("mixamorig:Head");
        jointNeck = GameObject.Find("mixamorig:Neck");
        jointSpineUpper = GameObject.Find("mixamorig:Spine2");
        jointSpineMiddle = GameObject.Find("mixamorig:Spine1");
        jointSpineLower = GameObject.Find("mixamorig:Spine");
        jointSpineBase = GameObject.Find("mixamorig:Hips");
        jointLegThighLeft = GameObject.Find("mixamorig:LeftUpLeg");
        jointLegThighRight = GameObject.Find("mixamorig:RightUpLeg");
        jointLegKneeLeft = GameObject.Find("mixamorig:LeftLeg");
        jointLegKneeRight = GameObject.Find("mixamorig:RightLeg");
        jointLegAnkleLeft = GameObject.Find("mixamorig:LeftFoot");
        jointLegAnkleRight = GameObject.Find("mixamorig:RightFoot");

        //jointLegToesLeft = GameObject.Find("mixamorig:LeftToeBase");
        //jointLegToseRight = GameObject.Find("mixamorig:RightToeBase");

        jointArmShoulderUpperLeft = GameObject.Find("mixamorig:LeftShoulder");
        jointArmShoulderUpperRight = GameObject.Find("mixamorig:RightShoulder");
        jointArmShoulderLowerLeft = GameObject.Find("mixamorig:LeftArm");
        jointArmShoulderLowerRight = GameObject.Find("mixamorig:RightArm");
        jointArmElbowLeft = GameObject.Find("mixamorig:LeftForeArm");
        jointArmElbowRight = GameObject.Find("mixamorig:RightForeArm");
        jointArmWristLeft = GameObject.Find("mixamorig:LeftHand");
        jointArmWristRight = GameObject.Find("mixamorig:RightHand");

        //rightHandPreLoc = new Vector3();
        //rightHandCurrLoc = new Vector3();
    }

    // To be used for creating more than one controlled avatars
    private void CreateBodyObj(ulong id)
    {
        
    }

    // Update joint data (orientations) of the body
    private void RefreshJointOrientation(Body body)
    {
        // Breath first Traversal
        for (JointType jointType = JointType.SpineBase; jointType <= JointType.ThumbRight; jointType++)
        {
            //Windows.Kinect.Joint sourceJoint = body.Joints[jointType];
            // What does that Joint? mean?
            //Windows.Kinect.Joint? targetJoint = null;

            //JointType parent;
            JointType child;

            Quaternion localRotation = new Quaternion();
            //Quaternion parentRotation = new Quaternion();

            // Root joint
            if (jointType == JointType.SpineBase)
            {
                child = jointType;

                localRotation = new Quaternion(
                    body.JointOrientations[child].Orientation.X,
                    body.JointOrientations[child].Orientation.Y,
                    body.JointOrientations[child].Orientation.Z,
                    body.JointOrientations[child].Orientation.W);
            }
            // Has parent
            else
            {
                //parent = jointHierarchy[jointType];
                child = jointType;

                localRotation = new Quaternion(
                    body.JointOrientations[child].Orientation.X,
                    body.JointOrientations[child].Orientation.Y,
                    body.JointOrientations[child].Orientation.Z,
                    body.JointOrientations[child].Orientation.W);

                //parentRotation = ...
            }

            // Apply joint orientation to each joint, still not clear why 
            // some joints need to be rotated around x axis after applying
            // the orientation data
            switch (jointType)
            {
                case JointType.SpineBase:
                    jointSpineBase.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    break;

                case JointType.SpineMid:
                    jointSpineLower.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    break;

                case JointType.SpineShoulder:
                    jointSpineMiddle.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    break;

                case JointType.Neck:
                    jointSpineUpper.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    break;

                case JointType.Head:
                    jointNeck.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    break;
                    
                case JointType.ShoulderLeft:
                    jointArmShoulderUpperLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmShoulderUpperLeft.transform.Rotate(new Vector3(0, -90, 0));
                    break;

                case JointType.ShoulderRight:
                    jointArmShoulderUpperRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmShoulderUpperRight.transform.Rotate(new Vector3(0, 90, 0));
                    break;
                    
                case JointType.ElbowLeft:
                    jointArmShoulderLowerLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmShoulderLowerLeft.transform.Rotate(new Vector3(0, -180, 0));
                    break;
                    
                case JointType.ElbowRight:
                    jointArmShoulderLowerRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmShoulderLowerRight.transform.Rotate(new Vector3(0, 180, 0));
                    break;

                // X need to be locked
                case JointType.WristLeft:
                    jointArmElbowLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmElbowLeft.transform.Rotate(new Vector3(0, -90, 0));
                    jointArmElbowLeft.transform.rotation =
                        LockXRotation(jointArmElbowLeft.transform.rotation);
                    break;

                // X need to be locked
                case JointType.WristRight:
                    jointArmElbowRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmElbowRight.transform.Rotate(new Vector3(0, 90, 0));
                    jointArmElbowRight.transform.rotation =
                        LockXRotation(jointArmElbowRight.transform.rotation);
                    break;

                case JointType.HandLeft:
                    jointArmWristLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmWristLeft.transform.Rotate(new Vector3(0, -90, 0));
                    //jointArmWristLeft.transform.rotation =
                    //    LockXRotation(jointArmWristLeft.transform.rotation);
                    break;

                case JointType.HandRight:
                    jointArmWristRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmWristRight.transform.Rotate(new Vector3(0, 90, 0));
                    //jointArmWristRight.transform.rotation =
                    //    LockXRotation(jointArmWristRight.transform.rotation);
                    break;

                /*case JointType.HipLeft:
                    jointArmElbowRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmElbowRight.transform.Rotate(new Vector3(0, 90, 0));
                    //jointLegThighLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;

                case JointType.HipRight:
                    //jointLegThighRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;*/

                case JointType.KneeLeft:
                    jointLegThighLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointLegThighLeft.transform.Rotate(new Vector3(0, -90, 0));
                    break;

                case JointType.KneeRight:
                    jointLegThighRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointLegThighRight.transform.Rotate(new Vector3(0, 90, 0));
                    break;

                case JointType.AnkleLeft:
                    jointLegKneeLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointLegKneeLeft.transform.Rotate(new Vector3(0, -90, 0));
                    break;

                case JointType.AnkleRight:
                    jointLegKneeRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointLegKneeRight.transform.Rotate(new Vector3(0, 90, 0));
                    break;

                /*case JointType.FootLeft:
                    jointLegAnkleLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    //jointLegAnkleLeft.transform.Rotate(new Vector3(0, -90, 0));
                    break;

                case JointType.FootRight:
                    jointLegAnkleRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    //jointLegAnkleRight.transform.Rotate(new Vector3(0, 90, 0));
                    break;*/

                default:
                    break;
            }
        }
    }

    // Gizmos throws an exception
    private void TestProjectionMapping()
    {
        CameraSpacePoint camPoint = new CameraSpacePoint();
        camPoint.X = 0.1f;
        camPoint.Y = 0.1f;
        camPoint.Z = 1;
        
        // Render the point being projected
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(camPoint.X, camPoint.Y, camPoint.Z), 1);
        
        DepthSpacePoint depthPoint = coordMapper.MapCameraPointToDepthSpace(camPoint);

        print("Depth point x: " + depthPoint.X);
        print("Depth point y: " + depthPoint.Y);
        
        // Get room 3d vertices data
        if (camPoints == null)
        {
            camPoints = new CameraSpacePoint[depthData.Length];
        }

        coordMapper.MapDepthFrameToCameraSpace(depthData, camPoints);

        CameraSpacePoint projectedPoint = coordMapper.MapDepthPointToCameraSpace(
            depthPoint,
            depthData[(uint)depthPoint.Y * depthWidth + (uint)depthPoint.X]);

        print("Project 3d point x: " + projectedPoint.X);
        print("Project 3d point y: " + projectedPoint.Y);
        print("Project 3d point z: " + projectedPoint.Z);

        // Render the projected point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(projectedPoint.X, projectedPoint.Y, projectedPoint.Z), 1);
    }

    // Lock euler angle ([0, 45] and [330, 360])
    private Quaternion LockXRotation(Quaternion rotationIn)
    {
        Quaternion rotationOut = new Quaternion();

        float x = rotationIn.eulerAngles.x;

        if (rotationIn.eulerAngles.x >= 45f && rotationIn.eulerAngles.x < 180f)
        {
            x = 45f;
        }
        else if (rotationIn.eulerAngles.x > 180f && rotationIn.eulerAngles.x <= 330f)
        {
            x = 330f;
        }

        rotationOut.eulerAngles = new Vector3(
            x, // z
            rotationIn.eulerAngles.y, // x
            rotationIn.eulerAngles.z); // y
        
        return rotationOut;
    }

    // Convert the coordinate system from kinect camera space to 
    // Unity world space by flipping x axis
    private Quaternion ConvertCoordSysFromKinectToUnity(Quaternion rotationIn)
    {
        Quaternion rotationOut = new Quaternion(
            rotationIn.x, 
            -rotationIn.y, 
            -rotationIn.z, 
            rotationIn.w);

        /*Quaternion rotationOut = new Quaternion();

        rotationOut.x = 1.414f * 0.5f * (rotationIn.x + rotationIn.w);
        rotationOut.y = 1.414f * 0.5f * (rotationIn.y - rotationIn.z);
        rotationOut.z = 1.414f * 0.5f * (rotationIn.z + rotationIn.y);
        rotationOut.w = 1.414f * 0.5f * (rotationIn.w - rotationIn.x);*/
        
        return rotationOut;
    }

    /*private Vector3 GetRightHandMovingVel(Body body)
    {
        rightHandCurrLoc.x = body.Joints[JointType.HandRight].Position.X;
        rightHandCurrLoc.y = body.Joints[JointType.HandRight].Position.Y;
        rightHandCurrLoc.z = body.Joints[JointType.HandRight].Position.Z;

        Vector3 rightHandMovingVel = rightHandCurrLoc - rightHandPreLoc;

        rightHandPreLoc = rightHandCurrLoc;

        return rightHandMovingVel;
    }*/

    private void UpdateFireBall(Body body)
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
            jointArmWristRight.transform.position,
            jointArmElbowRight.transform.position,
            //GetRightHandMovingVel(body),
            body.HandRightState);

        /*Vector3 fireBallPositionVec = new Vector3(
            jointArmWristRight.transform.position.x,
            jointArmWristRight.transform.position.y + 0.5f,
            jointArmWristRight.transform.position.z);

        fireBallEffect.transform.position = fireBallPositionVec;*/
    }
    
    // Update is called once per frame
    void Update()
    {
        //int state = 0;

        if (bodyManager == null)
        {
            return;
        }

        bodyManagerScript = bodyManager.GetComponent<BodyManager>();

        if (bodyManagerScript == null)
        {
            return;
        }

        bodyData = bodyManagerScript.GetBodyData();
        colorData = bodyManagerScript.GetColorData();
        depthData = bodyManagerScript.GetDepthData();

        depthWidth = bodyManagerScript.GetDepthWidth();
        depthHeight = bodyManagerScript.GetDepthHeight();
        coordMapper = bodyManagerScript.GetCoordMapper();

        if (bodyData == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();

        foreach (var body in bodyData)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }
        /*
        List<ulong> knownIds = new List<ulong>(bodyMap.Keys);

        // Delete untracked bodies
        foreach (ulong knownId in knownIds)
        {
            if (!trackedIds.Contains(knownId))
            {
                Destroy(bodyMap[knownId]);
                bodyMap.Remove(knownId);
            }
        }
        */
        foreach (var body in bodyData)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                /*if (!bodyMap.ContainsKey(body.TrackingId))
                {
                    bodyMap[body.TrackingId] = CreateBodyObj(body.TrackingId);
                }*/

                RefreshJointOrientation(body);
                UpdateFireBall(body);
            }
        }

        //TestProjectionMapping();
    }
}
