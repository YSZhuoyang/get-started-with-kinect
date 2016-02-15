using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class RoomMeshRenderer : MonoBehaviour
{
    private static int NUMVERTICESPERMESH = 60000;

    private GameObject bodyManager;
    private BodyManager bodyManagerScript;
    
    public Material roomMaterial;

    private CameraSpacePoint[] camPoints;
    private int[] indices;
    private Vector3[] vertices;
    private Color[] colors;
    private Mesh[] meshs;

    private CoordinateMapper coordMapper;

    // Use this for initialization
    void Start ()
    {
        bodyManager = GameObject.Find("BodyManager");
    }

    private void BuildMeshs(CameraSpacePoint[] camPoints)
    {
        int numMesh = camPoints.Length / NUMVERTICESPERMESH + 1;
        //vertices = new Vector3[camPoints.Length];
        //colors = new Color[camPoints.Length];

        meshs = new Mesh[numMesh];

        // For testing
        //indices = new int[NUMVERTICESPERMESH];
        //vertices = new Vector3[NUMVERTICESPERMESH];
        //colors = new Color[NUMVERTICESPERMESH];

        int numVertices = 0;

        for (int im = 0; im < meshs.Length; im++)
        {
            meshs[im] = new Mesh();

            if (camPoints.Length - im * NUMVERTICESPERMESH < NUMVERTICESPERMESH)
            {
                numVertices = camPoints.Length - im * NUMVERTICESPERMESH;
            }
            else
            {
                numVertices = NUMVERTICESPERMESH;
            }

            indices = new int[numVertices];
            vertices = new Vector3[numVertices];
            colors = new Color[numVertices];

            for (int i = 0; i < numVertices; i++)
            {
                indices[i] = i;

                vertices[i].x = camPoints[im * NUMVERTICESPERMESH + i].X;
                vertices[i].y = camPoints[im * NUMVERTICESPERMESH + i].Y;
                vertices[i].z = camPoints[im * NUMVERTICESPERMESH + i].Z;

                colors[i] = Color.white;
            }
            
            meshs[im].vertices = vertices;
            meshs[im].colors = colors;
            //meshs[im].triangles = indices;
        }
        


    }

    private void DrawRoomMesh()
    {
        for (int i = 0; i < meshs.Length; i++)
        {
            //roomMaterial.SetPass(0);
            //Graphics.DrawMeshNow(roomMesh, new Vector3(0, 0, 0), Quaternion.identity);
            Graphics.DrawMesh(meshs[i], new Vector3(0, 0, 0), Quaternion.identity, roomMaterial, 0);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (bodyManager == null)
        {
            return;
        }

        bodyManagerScript = bodyManager.GetComponent<BodyManager>();

        if (bodyManagerScript == null)
        {
            print("Error: bodyManager script not found");

            return;
        }

        coordMapper = bodyManagerScript.GetCoordMapper();
        camPoints = bodyManagerScript.GetCameraSpaceData();

        if (camPoints == null)
        {
            return;
        }

        //BuildMeshs(camPoints);
        //DrawRoomMesh();
    }
}
