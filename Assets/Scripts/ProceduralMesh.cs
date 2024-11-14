using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProceduralMesh
{
    public static Mesh Plane(int n, int m)
    {
        Mesh mesh = new Mesh
        {
            name = "Procedural Plane",
            indexFormat = IndexFormat.UInt32,
        };

        Vector3[] vertices = new Vector3[(m + 1) * (n + 1) + n * m];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        int[] triangles = new int[n * m * 4 * 3];

        //Generate Lattice Points
        for (int z = 0, index = 0; z <= m; z++, index += n) // may need to be n + 1
        {
            for (int x = 0; x <= n; x++, index++)
            {
                vertices[index] = new Vector3(
                    x,
                    0.0f,
                    -z
                );
                vertices[index].Scale(new Vector3(1.0f / m, 1.0f, 1.0f / n));

                uvs[index] = new Vector2(
                    (float)x / m,
                    (float)z / n
                );

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }

        //Generate Center Points
        for (int z = 0, index = n + 1; z < m; z++, index += n + 1) // may need to be n + 2
        {
            for (int x = 0; x < n; x++, index++)
            {
                vertices[index] = new Vector3(
                    x + 0.5f,
                    0.0f,
                    -z - 0.5f
                );
                vertices[index].Scale(new Vector3(1.0f / m, 1.0f, 1.0f / n));

                uvs[index] = new Vector2(
                    ((float)x + 0.5f) / m,
                    ((float)z + 0.5f) / n
                );

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }


        for (int z = 0, vertex = 0, index = 0; z < m; z++, vertex += (n + 1))
        {
            for (int x = 0; x < n; x++, vertex++)
            {
                int p1 = vertex;
                int p2 = vertex + 1;
                int p3 = vertex + n + n + 2;
                int p4 = vertex + n + n + 1;
                int c = vertex + n + 1;

                triangles[index++] = p1;
                triangles[index++] = p2;
                triangles[index++] = c;

                triangles[index++] = p2;
                triangles[index++] = p3;
                triangles[index++] = c;

                triangles[index++] = p3;
                triangles[index++] = p4;
                triangles[index++] = c;

                triangles[index++] = p4;
                triangles[index++] = p1;
                triangles[index++] = c;

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
