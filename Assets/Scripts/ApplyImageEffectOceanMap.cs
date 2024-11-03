using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ApplyImageEffectOceanMap : MonoBehaviour
{
    public enum ViewType
    {
        InitialFreq,
        InitialInverseFreq,
        DXFreq,
        DYFreq,
        DZFreq,
        Butterfly
    }
    public OceanMapGenerator oceanMap;
    public ViewType viewType = ViewType.InitialFreq;
    private ViewType lastViewType;


    private Material material;

    void UpdateViewType()
    {
        if (material != null)
        {
            Destroy(material);
        }

        if (viewType == ViewType.InitialFreq)
        {
            material = new Material(Shader.Find("Parker/ComputeFloat2"));
            material.SetBuffer("viewBuffer", oceanMap.h0k_buffer);
        }
        else if (viewType == ViewType.InitialInverseFreq)
        {
            material = new Material(Shader.Find("Parker/ComputeFloat2"));
            material.SetBuffer("viewBuffer", oceanMap.h0k_inv_buffer);
        }
        else if (viewType == ViewType.DXFreq)
        {
            material = new Material(Shader.Find("Parker/ComputeFloat2"));
            material.SetBuffer("viewBuffer", oceanMap.htk_dx_buffer);
        }
        else if (viewType == ViewType.DYFreq)
        {
            material = new Material(Shader.Find("Parker/ComputeFloat2"));
            material.SetBuffer("viewBuffer", oceanMap.htk_dy_buffer);
        }
        else if (viewType == ViewType.DZFreq)
        {
            material = new Material(Shader.Find("Parker/ComputeFloat2"));
            material.SetBuffer("viewBuffer", oceanMap.htk_dz_buffer);
        }
        else if (viewType == ViewType.Butterfly)
        {
            material = new Material(Shader.Find("Parker/ComputeButterfly"));
            material.SetBuffer("viewBuffer", oceanMap.butterfly_buffer);
        }

        material.SetFloat("resolution", oceanMap.mapResolution);
    }

    void Start()
    {
        lastViewType = viewType;
        UpdateViewType();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastViewType != viewType)
        {
            UpdateViewType();
        }

        lastViewType = viewType;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            Graphics.Blit(source, destination, material);
        }
        else
        {
            Debug.Log("No material set");
            Graphics.Blit(source, destination);
        }
    }
}
