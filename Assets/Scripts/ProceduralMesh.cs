using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProceduralMesh
{
    public static Mesh Clipmap(int vertexDensity, int clipMapLevels, int overlap = 2)
    {
        int clipLevelHalfSize = (vertexDensity + 1) * 4 - 1;

        Mesh mesh = new Mesh
        {
            name = "Procedural Clipmap",
            indexFormat = IndexFormat.UInt32,
        };

        CombineInstance[] combine = new CombineInstance[clipMapLevels + 2];

        combine[0].mesh = Plane(2 * clipLevelHalfSize + overlap, 2 * clipLevelHalfSize + overlap, PlaneTriangulation.Diagonal, (Vector3.right - Vector3.forward) * (clipLevelHalfSize + 1));
        // combine[0].mesh = Plane(3, 3, PlaneTriangulation.Diagonal);
        combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        //Mesh clipmapRing = ClipMapRing(clipLevelHalfSize, overlap);

        // for (int i = 1; i <= clipMapLevels; i++)
        // {
        //     combine[i].mesh = clipmapRing;
        //     combine[i].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * Mathf.Pow(2, i));
        // }

        mesh.CombineMeshes(combine, true);
        return mesh;

    }

    private static Mesh ClipMapRing(int clipLevelHalfSize, int overlap)
    {
        Mesh mesh = new Mesh
        {
            name = "Clipmap Ring",
            indexFormat = IndexFormat.UInt32,
        };

        int k = clipLevelHalfSize;

        int shortSide = (k + 1) / 2 + overlap;
        int longSide = k - 1;

        bool shortMorphShift = (shortSide / 2) % 2 == 1;

        CombineInstance[] combine = new CombineInstance[8];

        Vector3 pivot = (Vector3.right - Vector3.forward) * (k + 1);

        // //Top left
        // combine[0].mesh = Plane(shortSide, shortSide, pivot);
        // combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        // //Middle left
        // combine[1].mesh = Plane(shortSide, longSide, pivot);
        // combine[1].transform = Matrix4x4.TRS(-Vector3.forward * shortSide, Quaternion.identity, Vector3.one);

        // //Bottom left
        // combine[2].mesh = Plane(shortSide, shortSide, pivot);
        // combine[2].transform = Matrix4x4.TRS(-Vector3.forward * (shortSide + longSide), Quaternion.identity, Vector3.one);

        // //Bottom middle
        // combine[3].mesh = Plane(longSide, shortSide, pivot);
        // combine[3].transform = Matrix4x4.TRS(-Vector3.forward * (shortSide + longSide) + Vector3.right * shortSide, Quaternion.identity, Vector3.one);

        // //Bottom right
        // combine[4].mesh = Plane(shortSide, shortSide, pivot);
        // combine[4].transform = Matrix4x4.TRS(-Vector3.forward * (shortSide + longSide) + Vector3.right * (shortSide + longSide), Quaternion.identity, Vector3.one);

        // //Middle right
        // combine[5].mesh = Plane(shortSide, longSide, pivot);
        // combine[5].transform = Matrix4x4.TRS(-Vector3.forward * shortSide + Vector3.right * (shortSide + longSide), Quaternion.identity, Vector3.one);

        // //Top right
        // combine[6].mesh = Plane(shortSide, shortSide, pivot);
        // combine[6].transform = Matrix4x4.TRS(Vector3.right * (shortSide + longSide), Quaternion.identity, Vector3.one);

        // //Top middle
        // combine[7].mesh = Plane(longSide, shortSide, pivot);
        // combine[7].transform = Matrix4x4.TRS(Vector3.right * shortSide, Quaternion.identity, Vector3.one);

        mesh.CombineMeshes(combine, true);

        return mesh;
    }

    public enum PlaneTriangulation
    {
        Diagonal,
        Centroid
    }

    private static void FillPlaneVerticesDiagonal(int n, int m, Vector3[] vertices, Vector2[] uvs, Vector4[] tangents, Vector3 pivot, bool offsetUvs, bool morphShiftX = false, bool morphShiftZ = false)
    {
        for (int z = 0, index = 0; z <= m; z++)
        {
            for (int x = 0; x <= n; x++, index++)
            {
                Vector3 pos = new Vector3(
                    x,
                    0.0f,
                    -z
                );

                vertices[index] = pos - pivot;

                if (offsetUvs)
                {

                }
                else
                {
                    uvs[index] = new Vector2(
                        (float)x / m,
                        (float)z / n
                    );
                }

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }
    }

    private static void FillPlaneVerticesCentroid(int n, int m, Vector3[] vertices, Vector2[] uvs, Vector4[] tangents, Vector3 pivot, bool offsetUvs, bool morphShiftX = false, bool morphShiftZ = false)
    {
        //Generate Lattice Points
        for (int z = 0, index = 0; z <= m; z++, index += n)
        {
            for (int x = 0; x <= n; x++, index++)
            {

                Vector3 pos = new Vector3(
                    x,
                    0.0f,
                    -z
                );


                pos -= pivot;


                vertices[index] = pos;
                // vertices[index].Scale(new Vector3(1.0f / m, 1.0f, 1.0f / n));

                uvs[index] = new Vector2(
                    (float)x / m,
                    (float)z / n
                );

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }

        //Generate Center Points
        for (int z = 0, index = n + 1; z < m; z++, index += n + 1)
        {
            for (int x = 0; x < n; x++, index++)
            {
                Vector3 pos = new Vector3(
                    x + 0.5f,
                    0.0f,
                    -z - 0.5f
                );

                pos -= pivot;
                // vertices[index].Scale(new Vector3(1.0f / m, 1.0f, 1.0f / n));
                vertices[index] = pos;

                uvs[index] = new Vector2(
                    ((float)x + 0.5f) / m,
                    ((float)z + 0.5f) / n
                );

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }
    }

    private static void FillPlaneTrianglesDiagonal(int n, int m, int[] triangles)
    {
        for (int z = 0, vertex = 0, index = 0; z < m; z++, vertex++)
        {
            for (int x = 0; x < n; x++, vertex++)
            {
                int p1 = vertex;
                int p2 = vertex + 1;
                int p3 = vertex + n + 2;
                int p4 = vertex + n + 1;

                triangles[index++] = p1;
                triangles[index++] = p2;
                triangles[index++] = p3;

                triangles[index++] = p3;
                triangles[index++] = p4;
                triangles[index++] = p1;

            }
        }

    }

    private static void FillPlaneTrianglesCentroid(int n, int m, int[] triangles)
    {
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
    }

    public static Mesh Plane(int n, int m, PlaneTriangulation triangulation = PlaneTriangulation.Diagonal, Vector3? pivotIn = null, bool offsetUvs = false, bool morphShiftX = false, bool morphShiftZ = false)
    {
        Mesh mesh = new Mesh
        {
            name = "Procedural Plane",
            indexFormat = IndexFormat.UInt32,
        };

        Vector3 pivot = pivotIn.GetValueOrDefault(Vector3.zero);

        Vector3[] vertices;
        Vector2[] uvs;
        Vector4[] tangents;

        switch (triangulation)
        {
            case PlaneTriangulation.Diagonal:
                vertices = new Vector3[(m + 1) * (n + 1)];
                uvs = new Vector2[vertices.Length];
                tangents = new Vector4[vertices.Length];
                FillPlaneVerticesDiagonal(n, m, vertices, uvs, tangents, pivot, offsetUvs, morphShiftX, morphShiftZ);
                break;
            case PlaneTriangulation.Centroid:
                vertices = new Vector3[(m + 1) * (n + 1) + n * m];
                uvs = new Vector2[vertices.Length];
                tangents = new Vector4[vertices.Length];
                FillPlaneVerticesCentroid(n, m, vertices, uvs, tangents, pivot, offsetUvs, morphShiftX, morphShiftZ);
                break;
            default:
                vertices = new Vector3[0];
                uvs = new Vector2[0];
                tangents = new Vector4[0];
                break;
        }

        int[] triangles;
        switch (triangulation)
        {
            case PlaneTriangulation.Diagonal:
                triangles = new int[n * m * 2 * 3];
                FillPlaneTrianglesDiagonal(n, m, triangles);
                break;
            case PlaneTriangulation.Centroid:
                triangles = new int[n * m * 4 * 3];
                FillPlaneTrianglesCentroid(n, m, triangles);
                break;
            default:
                triangles = new int[0];
                break;
        }


        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.tangents = tangents;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
