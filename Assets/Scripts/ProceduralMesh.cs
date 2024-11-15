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

        // High fidelity center mesh
        combine[0].mesh = Plane(
            2 * clipLevelHalfSize + overlap,
            2 * clipLevelHalfSize + overlap,
            triangulation: PlaneTriangulation.Diagonal,
            pivotIn: (Vector3.right + Vector3.forward) * (clipLevelHalfSize + 1),
            offsetUvs: true,
            setY: true // Set Y is used for determining clip map level in vertex shader
        );
        combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        // Incrementally larger mesh rings
        Mesh clipMapRing = ClipMapRing(clipLevelHalfSize, overlap);
        for (int i = 1; i <= clipMapLevels; i++)
        {
            combine[i].mesh = clipMapRing;
            combine[i].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * Mathf.Pow(2, i));
        }

        combine[clipMapLevels + 1].mesh = ClipMapSkirt(clipLevelHalfSize, 10, overlap);
        combine[clipMapLevels + 1].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * Mathf.Pow(2, clipMapLevels));

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

        // Length of edge not shared with center mesh 
        int shortSide = (clipLevelHalfSize + 1) / 2 + overlap;
        // Length of edge shared with center mesh
        int longSide = clipLevelHalfSize - 1;

        bool shortMorphShift = (shortSide / 2) % 2 == 1;

        CombineInstance[] combine = new CombineInstance[8];

        // Pivot is at the cent of clip map
        Vector3 pivot = (Vector3.right + Vector3.forward) * (clipLevelHalfSize + 1);
        PlaneTriangulation triangulation = PlaneTriangulation.Diagonal;

        //Bottom left
        combine[0].mesh = Plane(
            shortSide,
            shortSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: false,
            morphShiftZ: false,
            setY: true
        );
        combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        //Middle left
        combine[1].mesh = Plane(
            shortSide,
            longSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: false,
            morphShiftZ: shortMorphShift,
            setY: true
        );
        combine[1].transform = Matrix4x4.TRS(Vector3.forward * shortSide, Quaternion.identity, Vector3.one);

        //Top left
        combine[2].mesh = Plane(
            shortSide,
            shortSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: false,
            morphShiftZ: !shortMorphShift,
            setY: true
        );
        combine[2].transform = Matrix4x4.TRS(Vector3.forward * (shortSide + longSide), Quaternion.identity, Vector3.one);

        //Top middle
        combine[3].mesh = Plane(
            longSide,
            shortSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: shortMorphShift,
            morphShiftZ: !shortMorphShift,
            setY: true
        );
        combine[3].transform = Matrix4x4.TRS(Vector3.forward * (shortSide + longSide) + Vector3.right * shortSide, Quaternion.identity, Vector3.one);

        //Top right
        combine[4].mesh = Plane(
            shortSide,
            shortSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: !shortMorphShift,
            morphShiftZ: !shortMorphShift,
            setY: true
        );
        combine[4].transform = Matrix4x4.TRS(Vector3.forward * (shortSide + longSide) + Vector3.right * (shortSide + longSide), Quaternion.identity, Vector3.one);

        //Middle right
        combine[5].mesh = Plane(
            shortSide,
            longSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: !shortMorphShift,
            morphShiftZ: shortMorphShift,
            setY: true
        );
        combine[5].transform = Matrix4x4.TRS(Vector3.forward * shortSide + Vector3.right * (shortSide + longSide), Quaternion.identity, Vector3.one);

        //Bottom right
        combine[6].mesh = Plane(
            shortSide,
            shortSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: !shortMorphShift,
            morphShiftZ: false,
            setY: true
        );
        combine[6].transform = Matrix4x4.TRS(Vector3.right * (shortSide + longSide), Quaternion.identity, Vector3.one);

        //Bottom middle
        combine[7].mesh = Plane(
            longSide,
            shortSide,
            triangulation: triangulation,
            pivotIn: pivot,
            offsetUvs: true,
            morphShiftX: shortMorphShift,
            morphShiftZ: false,
            setY: true
        );
        combine[7].transform = Matrix4x4.TRS(Vector3.right * shortSide, Quaternion.identity, Vector3.one);

        mesh.CombineMeshes(combine, true);

        return mesh;
    }

    private static Mesh ClipMapSkirt(int clipLevelHalfSize, float outerBorderScale, int overlap)
    {
        Mesh mesh = new Mesh
        {
            name = "Clip Map Skirt",
            indexFormat = IndexFormat.UInt32
        };

        CombineInstance[] combine = new CombineInstance[8];

        int borderVertCount = clipLevelHalfSize + overlap;
        int scale = 2;

        Vector3 pivot = new Vector3(-1f, 0, -1f) * borderVertCount * (1 + 2 * outerBorderScale) + new Vector3(1, 0, 1);

        Mesh quad = Plane(1, 1, setY: true);
        Mesh hStrip = Plane(borderVertCount, 1, setY: true);
        Mesh vStrip = Plane(1, borderVertCount, setY: true);

        outerBorderScale *= borderVertCount * scale;
        Vector3 cornerQuadScale = new Vector3(outerBorderScale, 1, outerBorderScale);
        Vector3 stripScaleVert = new Vector3(scale, 1, outerBorderScale);
        Vector3 stripScaleHor = new Vector3(outerBorderScale, 1, scale);

        combine[0].mesh = quad;
        combine[0].transform = Matrix4x4.TRS(pivot + Vector3.zero, Quaternion.identity, cornerQuadScale);

        combine[1].mesh = hStrip;
        combine[1].transform = Matrix4x4.TRS(pivot + Vector3.right * outerBorderScale, Quaternion.identity, stripScaleVert);

        combine[2].mesh = quad;
        combine[2].transform = Matrix4x4.TRS(pivot + Vector3.right * (outerBorderScale + borderVertCount * scale), Quaternion.identity, cornerQuadScale);

        combine[3].mesh = vStrip;
        combine[3].transform = Matrix4x4.TRS(pivot + Vector3.forward * outerBorderScale, Quaternion.identity, stripScaleHor);

        combine[4].mesh = vStrip;
        combine[4].transform = Matrix4x4.TRS(pivot + Vector3.right * (outerBorderScale + borderVertCount * scale) + Vector3.forward * outerBorderScale, Quaternion.identity, stripScaleHor);

        combine[5].mesh = quad;
        combine[5].transform = Matrix4x4.TRS(pivot + Vector3.forward * (outerBorderScale + borderVertCount * scale), Quaternion.identity, cornerQuadScale);

        combine[6].mesh = hStrip;
        combine[6].transform = Matrix4x4.TRS(pivot + Vector3.right * outerBorderScale + Vector3.forward * (outerBorderScale + borderVertCount * scale), Quaternion.identity, stripScaleVert);

        combine[7].mesh = quad;
        combine[7].transform = Matrix4x4.TRS(pivot + Vector3.right * (outerBorderScale + borderVertCount * scale) + Vector3.forward * (outerBorderScale + borderVertCount * scale), Quaternion.identity, cornerQuadScale);

        mesh.CombineMeshes(combine, true);

        return mesh;
    }
    public enum PlaneTriangulation
    {
        Diagonal,
        Centroid
    }

    private static void FillPlaneVerticesDiagonal(int n, int m, Vector3[] vertices, Vector2[] uvs, Vector4[] tangents, Vector3 pivot, bool offsetUvs, bool morphShiftX, bool morphShiftZ, bool setY)
    {
        for (int z = 0, index = 0; z <= m; z++)
        {
            for (int x = 0; x <= n; x++, index++)
            {
                Vector3 pos = new Vector3(
                    x,
                    setY ? 1.0f : 0.0f,
                    z
                );

                vertices[index] = pos - pivot;

                Vector2 uv = new Vector2(pos.x, pos.z);
                if (uv.x % 2 != 0)
                {
                    uv.x += morphShiftX ^ uv.x % 4 == 3 ? 1 : -1;
                }
                if (uv.y % 2 != 0)
                {
                    uv.y += morphShiftZ ^ uv.y % 4 == 3 ? 1 : -1;
                }

                if (offsetUvs)
                {
                    uvs[index] = new Vector2(
                        uv.x - pos.x,
                        uv.y - pos.z
                    );
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

    private static void FillPlaneVerticesCentroid(int n, int m, Vector3[] vertices, Vector2[] uvs, Vector4[] tangents, Vector3 pivot, bool offsetUvs, bool morphShiftX, bool morphShiftZ, bool setY)
    {
        //Generate Lattice Points
        for (int z = 0, index = 0; z <= m; z++, index += n)
        {
            for (int x = 0; x <= n; x++, index++)
            {

                Vector3 pos = new Vector3(
                    x,
                    setY ? 1.0f : 0.0f,
                    z
                );

                vertices[index] = pos - pivot;

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
                    setY ? 1.0f : 0.0f,
                    z - 0.5f
                );

                vertices[index] = pos - pivot;

                uvs[index] = new Vector2(
                    ((float)x + 0.5f) / m,
                    ((float)z + 0.5f) / n
                );

                tangents[index] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }
    }

    private static void FillPlaneTrianglesDiagonal(int n, int m, int[] triangles, int triangleShift = 0)
    {
        for (int z = 0, vertex = 0, index = 0; z < m; z++, vertex++)
        {
            for (int x = 0; x < n; x++, vertex++)
            {
                int p1 = vertex;
                int p2 = vertex + 1;
                int p3 = vertex + n + 2;
                int p4 = vertex + n + 1;

                if ((vertex + triangleShift) % 2 == 0)
                {
                    triangles[index++] = p3;
                    triangles[index++] = p2;
                    triangles[index++] = p1;

                    triangles[index++] = p1;
                    triangles[index++] = p4;
                    triangles[index++] = p3;
                }
                else
                {
                    triangles[index++] = p4;
                    triangles[index++] = p2;
                    triangles[index++] = p1;

                    triangles[index++] = p4;
                    triangles[index++] = p3;
                    triangles[index++] = p2;
                }
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

                triangles[index++] = c;
                triangles[index++] = p2;
                triangles[index++] = p1;

                triangles[index++] = c;
                triangles[index++] = p3;
                triangles[index++] = p2;

                triangles[index++] = c;
                triangles[index++] = p4;
                triangles[index++] = p3;

                triangles[index++] = c;
                triangles[index++] = p1;
                triangles[index++] = p4;

            }
        }
    }

    public static Mesh Plane(int n, int m, PlaneTriangulation triangulation = PlaneTriangulation.Diagonal, Vector3? pivotIn = null, bool offsetUvs = false, bool morphShiftX = false, bool morphShiftZ = false, bool setY = false)
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
                FillPlaneVerticesDiagonal(n, m, vertices, uvs, tangents, pivot, offsetUvs, morphShiftX, morphShiftZ, setY);
                break;
            case PlaneTriangulation.Centroid:
                vertices = new Vector3[(m + 1) * (n + 1) + n * m];
                uvs = new Vector2[vertices.Length];
                tangents = new Vector4[vertices.Length];
                FillPlaneVerticesCentroid(n, m, vertices, uvs, tangents, pivot, offsetUvs, morphShiftX, morphShiftZ, setY);
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
