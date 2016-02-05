using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;


public class BodyController : MonoBehaviour
{
    public GameObject bodyManager;

    public GameObject jointHead;
    public GameObject jointNeck;
    public GameObject jointSpineUpper;
    public GameObject jointSpineMiddle;
    public GameObject jointSpineLower;
    public GameObject jointSpineBase;
    public GameObject jointLegThighLeft;
    public GameObject jointLegThighRight;
    public GameObject jointLegKneeLeft;
    public GameObject jointLegKneeRight;
    public GameObject jointLegAnkleLeft;
    public GameObject jointLegAnkleRight;
    public GameObject jointLegToesLeft;
    public GameObject jointLegToseRight;
    public GameObject jointArmShoulderUpperLeft;
    public GameObject jointArmShoulderUpperRight;
    public GameObject jointArmShoulderLowerLeft;
    public GameObject jointArmShoulderLowerRight;
    public GameObject jointArmElbowLeft;
    public GameObject jointArmElbowRight;
    public GameObject jointArmWristLeft;
    public GameObject jointArmWristRight;
    
    private BodyManager bodyManagerScript;
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

    }

    // To be used for creating more than one controlled avatars
    private void CreateBodyObj(ulong id)
    {
        
    }

    // Update joint data (orientations) of the body
    private void RefreshBodyObj(Body body)
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

                    //print("l x: " + jointArmElbowLeft.transform.rotation.eulerAngles.x);
                    //print("l y: " + jointArmElbowLeft.transform.rotation.eulerAngles.y);
                    //print("l z: " + jointArmElbowLeft.transform.rotation.eulerAngles.z);

                    break;

                // X need to be locked
                case JointType.WristRight:
                    jointArmElbowRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmElbowRight.transform.Rotate(new Vector3(0, 90, 0));
                    jointArmElbowRight.transform.rotation =
                        LockXRotation(jointArmElbowRight.transform.rotation);
                    break;
                    
                /*case JointType.HipLeft:
                    jointArmElbowRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmElbowRight.transform.Rotate(new Vector3(0, 90, 0));
                    //jointLegThighLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;

                case JointType.HipRight:
                    //jointLegThighRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                    */
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
                    break;
                    */
                default:
                    break;
            }
        }
    }

    // Lock euler angle ([0, 45] and [330, 360])
    private static Quaternion LockXRotation(Quaternion rotationIn)
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
    private static Quaternion ConvertCoordSysFromKinectToUnity(Quaternion rotationIn)
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

        Body[] bodyData = bodyManagerScript.GetBodyData();

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

                RefreshBodyObj(body);
            }
        }
    }
}
