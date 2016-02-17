using UnityEngine;
using System.Collections;

public class LightningAttack : MonoBehaviour
{
    private static ushort NUM_SEGMENTS = 12;
    private static float OFFSET_RANGE = 0.08f;
    private static float INTERVAL = 0.05f;
    private static float LIFETIME = 1.3f;

    private float startTime;
    private float lastFrameTime;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 lightningVec;

    private LineRenderer lineRenderer;
    private GameObject[] lightnings;
    private Vector3[] points;

	// Use this for initialization
	void Start()
    {
        startTime = Time.time;
        lastFrameTime = 0f;
        
        points = new Vector3[NUM_SEGMENTS];
        startPos = new Vector3(0f, 0f, 0f);
        endPos = new Vector3(0f, 3f, 0f);
        lightningVec = endPos - startPos;

        points[0] = startPos;
        points[NUM_SEGMENTS - 1] = endPos;
	}

    private void GenerateLightnings()
    {
        for (ushort i = 1; i < NUM_SEGMENTS - 1; i++)
        {
            points[i] = points[i - 1] + lightningVec / 11f + new Vector3(
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE),
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE),
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE));
        }

        GetComponent<LineRenderer>().SetPositions(points);
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Time.time - startTime > LIFETIME)
        {
            Destroy(gameObject);
        }
        else if (Time.time - lastFrameTime > INTERVAL)
        {
            GenerateLightnings();
            lastFrameTime = Time.time;
        }
    }
}
