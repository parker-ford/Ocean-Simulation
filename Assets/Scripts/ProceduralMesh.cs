using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProceduralMesh
{
    public static Mesh Plane(int xVertexCount, int zVertexCount, int xLength, int zLength)
    {
        Mesh mesh = new Mesh
        {
            name = "Procedural Plane",
            indexFormat = IndexFormat.UInt32,
        };

        Vector3[] vertices = new Vector3[(xVertexCount + 1) * (zVertexCount + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        int[] triangles = new int[xVertexCount * zVertexCount * 6];

        for (int x = 0, index = 0; x <= xVertexCount; x++)
        {
            for (int z = 0; z <= zVertexCount; z++, index++)
            {
                vertices[index] = new Vector3(
                    ((float)x / xVertexCount * xLength) - (xLength / 2.0f),
                    0.0f,
                    ((float)z / zVertexCount * zLength) - (zLength / 2.0f)
                );

                uvs[index] = new Vector2(
                    (float)x / xVertexCount,
                    (float)z / zVertexCount
                );

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }

        for (int x = 0, triangleIndex = 0, vertexIndex = 0; x < xVertexCount; x++)
        {
            for (int z = 0; z < zVertexCount; z++, triangleIndex += 6, vertexIndex++)
            {
                //Top Triangle
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + zVertexCount + 2;
                //Bot Triangle
                triangles[triangleIndex + 3] = vertexIndex;
                triangles[triangleIndex + 4] = vertexIndex + zVertexCount + 2;
                triangles[triangleIndex + 5] = vertexIndex + zVertexCount + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.tangents = tangents;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
