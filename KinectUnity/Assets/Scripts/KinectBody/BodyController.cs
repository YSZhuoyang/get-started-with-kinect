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
    //public GameObject jointArmElbowLeft;
    //public GameObject jointArmElbowRight;

    private Quaternion initialHeadOrientation;
    private Quaternion initialNeckOrientation;
    private Quaternion initialShoulderRightOrientation;
    private Quaternion initialElbowRightOrientation;

    private BodyManager bodyManagerScript;
    private Dictionary<ulong, GameObject> bodyMap = new Dictionary<ulong, GameObject>();
    private Dictionary<JointType, JointType> boneMap = new Dictionary<JointType, JointType>()
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
    };

    void Start()
    {
        initialHeadOrientation = jointHead.transform.rotation;
        initialNeckOrientation = jointNeck.transform.rotation;
        initialShoulderRightOrientation = jointArmShoulderLowerRight.transform.rotation;
        initialElbowRightOrientation = jointArmElbowRight.transform.rotation;
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
        for (JointType jointType = JointType.SpineBase; jointType <= JointType.ThumbRight; jointType++)
        {
            Windows.Kinect.Joint sourceJoint = body.Joints[jointType];
            // What does that Joint? mean?
            Windows.Kinect.Joint? targetJoint = null;

            if (boneMap.ContainsKey(jointType))
            {
                targetJoint = body.Joints[boneMap[jointType]];
            }

            //Transform jointObj = bodyObj.transform.FindChild(jointType.ToString());
            //jointObj.localPosition = GetVector3FromJoint(sourceJoint);

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
                    //print("X: " + jointNeck.transform.eulerAngles.x);
                    //print("Y: " + jointNeck.transform.eulerAngles.y);
                    //print("Z: " + jointNeck.transform.eulerAngles.z);
                    
					/*jointNeck.transform.rotation = Quaternion.FromToRotation (
						new Vector3 (0, 1, 0),
						GetRotationVector (body.JointOrientations [JointType.Neck]));
					jointNeck.transform.Rotate (initialNeckOrientation.eulerAngles);
					*/
				jointNeck.transform.rotation = jointNeck.parent.transform.rotation * body.JointOrientations [JointType.Neck].Orientation;
                    
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
                    print("s X: " + GetRotationVector(body.JointOrientations[JointType.ShoulderRight]).x);
                    print("s Y: " + GetRotationVector(body.JointOrientations[JointType.ShoulderRight]).y);
                    print("s Z: " + GetRotationVector(body.JointOrientations[JointType.ShoulderRight]).z);

                    jointArmShoulderLowerRight.transform.rotation = Quaternion.FromToRotation(
                        new Vector3(-1f, 1f, 0),
                        GetRotationVector(body.JointOrientations[JointType.ShoulderRight]));
                    jointArmShoulderLowerRight.transform.Rotate(initialShoulderRightOrientation.eulerAngles);

                    //jointArmShoulderUpperRight.transform.position = GetVector3FromJoint(sourceJoint);
                    break;
                /*case JointType.ElbowLeft:
                    jointArmElbowLeft.transform.position = GetVector3FromJoint(sourceJoint);
                    break;*/
                case JointType.ElbowRight:
                    //print("e X: " + GetRotationVector(body.JointOrientations[JointType.ElbowRight]).x);
                    //print("e Y: " + GetRotationVector(body.JointOrientations[JointType.ElbowRight]).y);
                    //print("e Z: " + GetRotationVector(body.JointOrientations[JointType.ElbowRight]).z);

                    jointArmElbowRight.transform.rotation = Quaternion.FromToRotation(
                        new Vector3(0f, 0f, 7f),
                        GetRotationVector(body.JointOrientations[JointType.ElbowRight]));
                    jointArmElbowRight.transform.Rotate(initialElbowRightOrientation.eulerAngles);
                    
                    //jointArmElbowRight.transform.position = GetVector3FromJoint(sourceJoint);
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

            /*
            LineRenderer lineRenderer = jointObj.GetComponent<LineRenderer>();

            if (targetJoint.HasValue)
            {
                lineRenderer.SetPosition(0, jointObj.localPosition);
                lineRenderer.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lineRenderer.SetColors(
                    GetColorForState(sourceJoint.TrackingState),
                    GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lineRenderer.enabled = false;
            }*/
        }
    }

    private static Vector3 GetRotationVector(Windows.Kinect.JointOrientation orientation)
    {
		//print ("Ori w: " + orientation.Orientation.W);

        return new Vector3(
			-orientation.Orientation.X / orientation.Orientation.W,
			orientation.Orientation.Y / orientation.Orientation.W,
			orientation.Orientation.Z / orientation.Orientation.W);
        //orientation.Orientation.W);
    }

    private static Color GetColorForState(TrackingState state)
    {
        switch (state)
        {
            case TrackingState.Tracked:
                return Color.green;

            case TrackingState.Inferred:
                return Color.red;

            default:
                return Color.black;
        }
    }

    private static Vector3 GetVector3FromJoint(Windows.Kinect.Joint joint)
    {
        return new Vector3(
            joint.Position.X * 4,
            joint.Position.Y * 4,
            joint.Position.Z * 4);
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
