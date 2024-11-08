using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class OceanMesh : MonoBehaviour
{
    public MeshFilter meshFilter;
    public OceanMapGenerator oceanMapGenerator;
    public Shader oceanShader;
    public int meshLength = 10;
    private int meshResolution;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private int[] triangles;
    private Vector2[] uvs;
    private Material oceanMaterial;

    Vector3[] generateMeshVerticesPlane(int m, int n, float width, float height)
    {
        Vector3[] v = new Vector3[m * n];
        for (int i = 0; i < m * n; i++)
        {
            float x = (i % m) * (width / (m - 1.0f)) - width / 2.0f;
            float z = (i / m) * (height / (n - 1.0f)) - height / 2.0f;
            v[i] = new Vector3(x, 0, z);
        }
        return v;
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = ProceduralMesh.Plane((int)oceanMapGenerator.mapResolution, (int)oceanMapGenerator.mapResolution, meshLength, meshLength);
        mesh.name = "Ocean Surface";
        GetComponent<MeshFilter>().mesh = mesh;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
