using UnityEngine;
using System.Collections;

public class HDFaceController : MonoBehaviour
{
    private Mesh[] meshes;
    private int NUM_POINTS_PER_MESH = 60000;
    private int numPoints;
    private int totalNumPoints;
    private int numMesh;
    private bool meshCreated;

    // Use this for initialization
    void Start()
    {
        meshCreated = false;
    }

    public void InitGameObjects(int numTotal)
    {
        totalNumPoints = numTotal;
        numMesh = totalNumPoints / NUM_POINTS_PER_MESH + 1;
        meshes = new Mesh[numMesh];

        for (int i = 0; i < numMesh; i++)
        {
            meshes[i] = new Mesh();
            GameObject meshRenderer = Instantiate(Resources.Load<GameObject>("PointCloudRenderer"));
            meshRenderer.GetComponent<MeshFilter>().mesh = meshes[i];
        }

        meshCreated = true;
    }

    // Rendering points using more than one meshes
    /*public void CreateMesh(Vector3[] pointsIn) //, UnityEngine.Color[] colorsIn
    {
        Vector3[] points;
        int[] indecies;
        UnityEngine.Color[] colors;

        if (!meshCreated)
        {
            InitGameObjects(pointsIn.Length);
        }

        for (int i = 0; i < numMesh; i++)
        {
            if (i < numMesh - 1)
            {
                numPoints = NUM_POINTS_PER_MESH;

                points = new Vector3[numPoints];
                indecies = new int[numPoints];
                colors = new UnityEngine.Color[numPoints];
            }
            else
            {
                numPoints = totalNumPoints % NUM_POINTS_PER_MESH;

                points = new Vector3[numPoints];
                colors = new UnityEngine.Color[numPoints];
                indecies = new int[numPoints];
            }

            for (int j = 0; j < numPoints; ++j)
            {
                points[j] = pointsIn[i * NUM_POINTS_PER_MESH + j];
                indecies[j] = j;
                colors[j] = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 1.0f);
            }

            meshes[i].vertices = points;
            meshes[i].colors = colors;
            meshes[i].SetIndices(indecies, MeshTopology.Points, i);
        }
    }*/

    // For rendering only one mesh
    public void CreateMesh(Vector3[] pointsIn, uint[] indicesIn, UnityEngine.Color[] colorsIn)
    {
        Vector3[] points;
        int[] indices;
        UnityEngine.Color[] colors;

        if (!meshCreated)
        {
            InitGameObjects(pointsIn.Length);
        }

        for (int i = 0; i < numMesh; i++)
        {
            if (i < numMesh - 1)
            {
                numPoints = NUM_POINTS_PER_MESH;

                points = new Vector3[numPoints];
                colors = new UnityEngine.Color[numPoints];
            }
            else
            {
                numPoints = totalNumPoints % NUM_POINTS_PER_MESH;

                points = new Vector3[numPoints];
                colors = new UnityEngine.Color[numPoints];
            }

            for (int j = 0; j < numPoints; ++j)
            {
                points[j] = pointsIn[i * NUM_POINTS_PER_MESH + j];
                colors[j] = colorsIn[j];
            }

            indices = new int[indicesIn.Length];

            for (int j = 0; j < indicesIn.Length; j++)
            {
                indices[j] = (int)indicesIn[j];
            }

            meshes[i].vertices = points;
            meshes[i].colors = colorsIn;
            meshes[i].SetIndices(indices, MeshTopology.Triangles, i);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
