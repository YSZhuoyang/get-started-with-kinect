using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;


public class BodyController : MonoBehaviour
{
    public Material boneMaterial;
    public GameObject bodyManager;

    public GameObject jointHead;
    public GameObject jointNeck;
    public GameObject jointSpineUpper;
    public GameObject jointSpineMiddle;
    public GameObject jointSpineLower;
    public GameObject jointPelvis;
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

    }

    private GameObject CreateBodyObj(ulong id)
    {
        GameObject body = new GameObject("Body: " + id);

        for (JointType jointType = JointType.SpineBase; jointType <= JointType.ThumbRight; jointType++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lineRender = jointObj.AddComponent<LineRenderer>();
            lineRender.SetVertexCount(2);
            lineRender.material = boneMaterial;
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

                parentRotation = Quaternion.identity;
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
                case JointType.Head:
                    //jointHead.transform.rotation = initialHeadOrientation;
                    //jointHead.transform.Rotate(GetRotationVector(body.JointOrientations[JointType.Head]));
                    //jointHead.transform.localRotation = new Quaternion(0, 0, 0, 0);

                    //jointHead.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.Neck:

                    // Option 1
                    jointNeck.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(parentRotation) *
                        ConvertCoordSysFromKinectToUnity(localRotation) *
                        jointNeck.transform.parent.rotation;

                    // Option 2
                    //jointNeck.transform.localRotation = ConvertCoordSysFromKinectToUnity(localRotation);

                    break;
                /*case JointType.SpineBase:
                    jointSpineLower.transform.position = GetVector3FromJoint(sourceJoint);
                    jointPelvis.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.SpineMid:
                    jointSpineMiddle.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.SpineShoulder:
                    jointSpineUpper.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.ShoulderLeft:
                    jointArmShoulderUpperLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;*/
                case JointType.ShoulderRight:

                    jointArmShoulderLowerRight.transform.rotation =
                        //initialShoulderRightRotation *
                        //ConvertCoordSysFromKinectToUnity(parentRotation) *
                        ConvertCoordSysFromKinectToUnity(localRotation);// *
                    //jointArmShoulderLowerRight.transform.parent.rotation;// *
                    //jointArmShoulderLowerRight.transform.parent.rotation;

                    //jointArmShoulderLowerRight.transform.rotation =
                    //    ConvertCoordSysFromKinectToUnity(localRotation) * jointArmShoulderLowerRight.transform.parent.rotation;

                    break;
                /*case JointType.ElbowLeft:
                    jointArmElbowLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;*/
                case JointType.ElbowRight:

                    //jointArmElbowRight.transform.rotation =
                    //initialShoulderRightRotation *
                    //ConvertCoordSysFromKinectToUnity(parentRotation) *
                    //ConvertCoordSysFromKinectToUnity(parentRotation);

                    /*jointArmElbowRight.transform.rotation =
                        ConvertCoordSysFromKinectToUnity(parentRotation) *
                        ConvertCoordSysFromKinectToUnity(localRotation);// *
                        //jointArmElbowRight.transform.parent.rotation;*/

                    break;
                /*case JointType.WristLeft:
                    jointArmWristLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.WristRight:
                    jointArmWristRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;

                case JointType.HipLeft:
                    jointLegThighLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.HipRight:
                    jointLegThighRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.KneeLeft:
                    jointLegKneeLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.KneeRight:
                    jointLegKneeRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                case JointType.AnkleLeft:
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
    
    private static Quaternion ConvertCoordSysFromKinectToUnity(Quaternion rotationIn)
    {
        // Option 1
        Quaternion rotationOut = new Quaternion();
        rotationOut.eulerAngles = new Vector3(
            rotationIn.eulerAngles.z, // z
            rotationIn.eulerAngles.x, // x
            rotationIn.eulerAngles.y); // y
        
        // Option 2
        //Quaternion rotationOut = new Quaternion(rotationIn.x, rotationIn.y, rotationIn.z, rotationIn.w);

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
