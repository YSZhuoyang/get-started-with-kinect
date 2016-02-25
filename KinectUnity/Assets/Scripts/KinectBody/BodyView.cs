using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;


// For displaying skeleton data
public class BodyView : MonoBehaviour
{
    public Material boneMaterial;
    public GameObject sourceManager;

    private SourceManager sourceManagerScript;
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

            Transform jointObj = bodyObj.transform.FindChild(jointType.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

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
            }
        }
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
            joint.Position.X * 10, 
            joint.Position.Y * 10, 
            joint.Position.Z * 10);
    }

    // Use this for initialization
    void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
        //int state = 0;

        if (sourceManager == null)
        {
            return;
        }

        sourceManagerScript = sourceManager.GetComponent<SourceManager>();

        if (sourceManagerScript == null)
        {
            return;
        }

        Body[] bodyData = sourceManagerScript.GetBodyData();

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
