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
    public int sideSegments;

    // Start is called before the first frame update
    void Start()
    {
        mesh = ProceduralMesh.Plane(sideSegments, sideSegments);
        mesh.name = "Ocean Surface";
        GetComponent<MeshFilter>().mesh = mesh;

        // oceanMaterial = new Material(oceanShader);
        // oceanMaterial.SetTexture("_DisplacementTex", oceanMapGenerator.displacement);
        // oceanMaterial.SetTexture("_SlopeTex", oceanMapGenerator.slope);
        // oceanMaterial.SetVector("_LightDir", -sun.gameObject.transform.forward);

        oceanMaterial = new Material(Shader.Find("Standard"));

        GetComponent<MeshRenderer>().material = oceanMaterial;

    }

    // Update is called once per frame
    void Update()
    {

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
