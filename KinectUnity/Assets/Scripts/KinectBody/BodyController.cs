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

    public GameObject root;
    
    private BodyManager bodyManagerScript;
    private Dictionary<ulong, GameObject> bodyMap = new Dictionary<ulong, GameObject>();

    // <child, parent>
    private Dictionary<JointType, JointType> jointHierarchy = new Dictionary<JointType, JointType>()
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
    };

    /*private Dictionary<JointType, JointType> jointHierarchy = new Dictionary<JointType, JointType>()
    {
        { JointType.FootLeft, JointType.AnkleLeft },
        { JointType.AnkleLeft, JointType.KneeLeft },
        { JointType.KneeLeft, JointType.HipLeft },
        { JointType.HipLeft, JointType.SpineBase },

        { JointType.FootRight, JointType.AnkleRight },
        { JointType.AnkleRight, JointType.KneeRight },
        { JointType.KneeRight, JointType.HipRight },
        { JointType.HipRight, JointType.SpineBase },

        { JointType.HandTipLeft, JointType.HandLeft }, // Need this for HandSates
        { JointType.ThumbLeft, JointType.HandLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.WristLeft, JointType.ElbowLeft },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.ShoulderLeft, JointType.SpineShoulder },

        { JointType.HandTipRight, JointType.HandRight }, // Need this for Hand State
        { JointType.ThumbRight, JointType.HandRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.ShoulderRight, JointType.SpineShoulder },

        { JointType.SpineBase, JointType.SpineMid },
        { JointType.SpineMid, JointType.SpineShoulder },
        { JointType.SpineShoulder, JointType.Neck },
        { JointType.Neck, JointType.Head }
    };*/

    void Start()
    {
        root.transform.position = new Vector3(
            -root.transform.position.x,
            root.transform.position.y,
            root.transform.position.z);
    }

    private GameObject CreateBodyObj(ulong id)
    {
        GameObject body = new GameObject("Body: " + id);

        for (JointType jointType = JointType.SpineBase; jointType <= JointType.ThumbRight; jointType++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lineRender = jointObj.AddComponent<LineRenderer>();
            lineRender.SetVertexCount(2);
            lineRender.SetWidth(0.05f, 0.05f);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jointType.ToString();
            jointObj.transform.parent = body.transform;
        }

        return body;
    }

    private void RefreshBodyObj(Body body, GameObject bodyObj)
    {
        // Breath first Traversal
        for (JointType jointType = JointType.SpineBase; jointType <= JointType.ThumbRight; jointType++)
        {
            //Windows.Kinect.Joint sourceJoint = body.Joints[jointType];
            // What does that Joint? mean?
            //Windows.Kinect.Joint? targetJoint = null;
            
            Quaternion localRotation = new Quaternion();
            Quaternion parentRotation = new Quaternion();

            // Root joint
            if (jointType == JointType.SpineBase)
            {
                JointType root = jointType;

                localRotation = new Quaternion(
                    body.JointOrientations[root].Orientation.X,
                    body.JointOrientations[root].Orientation.Y,
                    body.JointOrientations[root].Orientation.Z,
                    body.JointOrientations[root].Orientation.W);
            }
            // Has parent
            else
            {
                //targetJoint = body.Joints[jointHierarchy[jointType]];
                
                //Transform jointObj = bodyObj.transform.FindChild(jointType.ToString());
                //jointObj.localPosition = GetVector3FromJoint(sourceJoint);
                
                JointType parent = jointHierarchy[jointType];
                JointType child = jointType;

                localRotation = new Quaternion(
                    body.JointOrientations[child].Orientation.X,
                    body.JointOrientations[child].Orientation.Y,
                    body.JointOrientations[child].Orientation.Z,
                    body.JointOrientations[child].Orientation.W);

                parentRotation = new Quaternion(
                    body.JointOrientations[parent].Orientation.X,
                    body.JointOrientations[parent].Orientation.Y,
                    body.JointOrientations[parent].Orientation.Z,
                    body.JointOrientations[parent].Orientation.W);
            }

            // Testing body control
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
                    jointArmShoulderUpperLeft.transform.Rotate(new Vector3(0, 180, 0));
                    break;

                case JointType.ShoulderRight:
                    jointArmShoulderUpperRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmShoulderUpperRight.transform.Rotate(new Vector3(0, 90, 0));
                    break;
                    
                case JointType.ElbowLeft:
                    jointArmShoulderLowerLeft.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmShoulderLowerLeft.transform.Rotate(new Vector3(0, 180, 0));
                    
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

                    print("l x: " + jointArmElbowLeft.transform.rotation.eulerAngles.x);
                    print("l y: " + jointArmElbowLeft.transform.rotation.eulerAngles.y);
                    print("l z: " + jointArmElbowLeft.transform.rotation.eulerAngles.z);

                    break;

                // X need to be locked
                case JointType.WristRight:
                    jointArmElbowRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(localRotation);
                    jointArmElbowRight.transform.Rotate(new Vector3(0, 90, 0));
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

                /*case JointType.AnkleLeft:
                    jointLegAnkleLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.AnkleRight:
                    jointLegAnkleRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.FootLeft:
                    jointLegToesLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.FootRight:
                    jointLegToseRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                    */
            }
        }
    }

    private static Quaternion LockXRotation(Quaternion rotationIn)
    {
        /*Quaternion rotationOut = new Quaternion();
        rotationOut.eulerAngles = new Vector3(
            0, // z
            rotationIn.eulerAngles.y, // x
            rotationIn.eulerAngles.z); // y
        */

        Quaternion rotationOut = new Quaternion(
            0, 
            rotationIn.y, 
            rotationIn.z, 
            rotationIn.w);
        
        return rotationOut;
    }

    private static Quaternion ConvertCoordSysFromKinectToUnity(Quaternion rotationIn)
    {
        Quaternion rotationOut = new Quaternion(
            rotationIn.x, 
            -rotationIn.y, 
            -rotationIn.z, 
            rotationIn.w);
        
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

        foreach (var body in bodyData)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!bodyMap.ContainsKey(body.TrackingId))
                {
                    bodyMap[body.TrackingId] = CreateBodyObj(body.TrackingId);
                }

                RefreshBodyObj(body, bodyMap[body.TrackingId]);
            }
        }
    }
}
