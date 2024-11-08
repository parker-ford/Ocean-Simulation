using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanMesh : MonoBehaviour
{
    public MeshFilter meshFilter;
    public OceanMapGenerator oceanMapGenerator;
    public Shader oceanShader;
    public int size = 10;
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
            float x = (i % m) * (1.0f / (m - 1.0f)) * width * 10 - width * 10.0f / 2.0f;
            float z = (i / m) * (1.0f / (n - 1.0f)) * height * 10 - height * 10.0f / 2.0f;
            v[i] = new Vector3(x, 0, z);
        }
        return v;
    }
    Vector3[] generateMeshNormalsPlane(int m, int n)
    {
        //All up Vector for now
        Vector3[] normals = new Vector3[m * n];
        for (int i = 0; i < m * n; i++)
        {
            normals[i] = Vector3.up;
        }

        return normals;
    }
    int[] generateMeshTriangles(int m, int n)
    {

        int numT = (m - 1) * (n - 1) * 2;
        int[] t = new int[numT * 3];

        int index = 0;
        for (int i = 0; i < (m * n) - m - 1; i++)
        {

            //skips end vertices
            if (i % m == m - 1)
            {
                continue;
            }

            //bot triangle of current rect
            t[index] = i;
            t[index + 1] = i + m;
            t[index + 2] = i + m + 1;
            index += 3;

            //top triangle of current rect
            t[index] = i;
            t[index + 1] = i + m + 1;
            t[index + 2] = i + 1;
            index += 3;

        }

        return t;
    }
    private Vector2[] generateMeshUVs(int widthVertices, int heightVertices)
    {
        Vector2[] uvs = new Vector2[widthVertices * heightVertices];

        for (int y = 0; y < heightVertices; y++)
        {
            for (int x = 0; x < widthVertices; x++)
            {
                int index = y * widthVertices + x;
                float u = (float)x / (widthVertices - 1);
                float v = (float)y / (heightVertices - 1);

                uvs[index] = new Vector2(u, v);
            }
        }

        return uvs;
    }

    // Start is called before the first frame update
    void Start()
    {

        oceanMaterial = new Material(oceanShader);
        oceanMaterial.SetTexture("_DisplacementTex", oceanMapGenerator.initialSpectrum);
        GetComponent<MeshRenderer>().material = oceanMaterial;

        vertices = generateMeshVerticesPlane((int)oceanMapGenerator.mapResolution, (int)oceanMapGenerator.mapResolution, size, size);
        normals = generateMeshNormalsPlane((int)oceanMapGenerator.mapResolution, (int)oceanMapGenerator.mapResolution);
        triangles = generateMeshTriangles((int)oceanMapGenerator.mapResolution, (int)oceanMapGenerator.mapResolution);
        uvs = generateMeshUVs((int)oceanMapGenerator.mapResolution, (int)oceanMapGenerator.mapResolution);

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
