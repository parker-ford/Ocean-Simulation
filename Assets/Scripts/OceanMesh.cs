using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OceanMesh : MonoBehaviour
{
    public MeshFilter meshFilter;
    public OceanMapGenerator oceanMapGenerator;
    public Shader oceanShader;
    public Light sun;
    public int meshLength = 10;
    private Mesh mesh;
    private Material oceanMaterial;
    public float clipmapScale;
    public int vertexDensity;
    public int clipmapLevels;
    public float clipMapMorphDistance = 8.0f;
    public Texture2D testTexture;

    // Start is called before the first frame update
    void Start()
    {
        // mesh = ProceduralMesh.Plane(sideSegments, sideSegments);
        mesh = ProceduralMesh.Clipmap(vertexDensity, clipmapLevels);
        mesh.name = "Ocean Surface";
        GetComponent<MeshFilter>().mesh = mesh;

        oceanMaterial = new Material(oceanShader);
        // oceanMaterial.SetTexture("_DisplacementTex", testTexture);
        oceanMaterial.SetTexture("_DisplacementTex", oceanMapGenerator.displacement);
        oceanMaterial.SetTexture("_SlopeTex", oceanMapGenerator.slope);

        // oceanMaterial = new Material(Shader.Find("Standard"));

        GetComponent<MeshRenderer>().material = oceanMaterial;

    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Find a better place for this
        Shader.SetGlobalVector("_CameraPosition", Camera.main.transform.position);
        Shader.SetGlobalFloat("_ClipMap_Scale", clipmapScale);
        Shader.SetGlobalFloat("_ClipMap_LevelHalfSize", (vertexDensity + 1) * 4 - 1);
        Shader.SetGlobalFloat("_ClipMap_MorphDistance", clipMapMorphDistance);
        oceanMaterial.SetVector("_LightDir", -sun.gameObject.transform.forward);

    }

    void OnApplicationQuit()
    {
        if (oceanMaterial)
        {
            Destroy(oceanMaterial);
            oceanMaterial = null;
        }
        if (mesh)
        {
            Destroy(mesh);
            mesh = null;
        }
    }
}

//viewerPosition is camera.main.transform

// public static void SetGlobalShaderVariables(Vector3 viewerPosition, int vertexDensity, float minMeshScale)
//                     Mathf.Log(Mathf.Abs(viewerPosition.y) / (2 * minMeshScale), 2) + 1));
//             Shader.SetGlobalVector(GlobalShaderVariables.ViewerPosition, viewerPosition);
