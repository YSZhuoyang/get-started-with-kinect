using UnityEngine;
using System.Collections;

public class LightningAttack : MonoBehaviour
{
    private static ushort NUM_SEGMENTS = 12;
    private static float OFFSET_RANGE = 0.1f;
    private static float END_POINT_OFFSET = 0.1f;
    private static float INTERVAL = 0.05f;
    private static float LIFETIME = 1.3f;

    private GameObject lightningController;
    private GameObject billboard;

    private LightningController lightningControllerScript;
    private HitCounter hitCounter;

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
        startPos = new Vector3();
        endPos = new Vector3();

        billboard = GameObject.Find("Billboard");
    }

    private void GenerateLightnings()
    {
        points[0] = startPos;
        points[NUM_SEGMENTS - 1] = endPos + new Vector3(
            Random.Range(-END_POINT_OFFSET, END_POINT_OFFSET),
            Random.Range(-END_POINT_OFFSET, END_POINT_OFFSET),
            Random.Range(-END_POINT_OFFSET, END_POINT_OFFSET));
        
        lightningVec = points[NUM_SEGMENTS - 1] - points[0];

        for (ushort i = 1; i < NUM_SEGMENTS - 1; i++)
        {
            points[i] = points[0] + i * lightningVec / (NUM_SEGMENTS - 1f) + new Vector3(
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE),
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE),
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE));
        }

        GetComponent<LineRenderer>().SetPositions(points);

        // Collision detection of lightnings through ray cast
        RaycastHit hit;
        Vector3 direction = points[NUM_SEGMENTS - 1] - points[0];
        float distance = Mathf.Sqrt(
            direction.x * direction.x + 
            direction.y * direction.y + 
            direction.z* direction.z);

        if (Physics.Raycast(points[0], direction, out hit, distance))
        {
            if (hit.transform.gameObject.tag == "Enemy")
            {
                Destroy(hit.transform.gameObject);

                // Trigger explosion
                Instantiate(
                    Resources.Load<GameObject>("Debris"),
                    hit.transform.gameObject.transform.position,
                    Quaternion.identity);

                // Update billboard
                hitCounter = billboard.GetComponent<HitCounter>();

                if (hitCounter != null)
                {
                    hitCounter.FireBallHit();
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Time.time - startTime > LIFETIME)
        {
            Destroy(gameObject);

            lightningController = GameObject.Find("LightningController");

            if (lightningController == null)
            {
                print("Error: FireBallController game object not found");

                return;
            }

            lightningControllerScript = lightningController.GetComponent<LightningController>();

            if (lightningControllerScript == null)
            {
                print("Error: FireBallController script not found");

                return;
            }

            lightningControllerScript.SetLightningState(LightningController.LightningState.extinguished);
        }
        else
        {
            if (Time.time - lastFrameTime > INTERVAL)
            {
                GenerateLightnings();
                lastFrameTime = Time.time;
            }
        }
    }
    
    public void SetStartPosAndEndPos(Vector3 startPosIn, Vector3 endPosIn)
    {
        startPos = startPosIn;
        endPos = endPosIn;
    }
}
