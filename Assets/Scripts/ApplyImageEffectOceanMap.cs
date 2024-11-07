using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ApplyImageEffectOceanMap : MonoBehaviour
{
    public enum ViewType
    {
        Source,
        InitialFreq,
        WavesData,
        DxDz,
        DyDxy,
        DyxDyz,
        DxxDzz,
        Butterfly,
        Buffer,
        Displacement
    }
    public OceanMapGenerator oceanMap;
    public ViewType viewType = ViewType.InitialFreq;
    private Material viewTextureArray;
    private Material viewFloat4RG;

    void Start()
    {
        // viewTextureArray = new Material(Shader.Find("Parker/TextureArray"));
        // viewTextureArray.SetTexture("_TextureArray", oceanMap.spectrum);

        viewFloat4RG = new Material(Shader.Find("Parker/Float4RG"));
        viewFloat4RG.SetTexture("_Tex", oceanMap.initialSpectrum);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        switch (viewType)
        {
            case ViewType.Source:
                Graphics.Blit(source, destination);
                break;
            case ViewType.InitialFreq:
                Graphics.Blit(source, destination, viewFloat4RG);
                break;
            case ViewType.WavesData:
                Graphics.Blit(oceanMap.wavesData, destination);
                break;
            case ViewType.DxDz:
                // viewTextureArray.SetFloat("_Layer", 0);
                Graphics.Blit(oceanMap.spectrumDxDz, destination);
                break;
            case ViewType.DyDxy:
                // viewTextureArray.SetFloat("_Layer", 1);
                Graphics.Blit(oceanMap.spectrumDyDxy, destination);
                break;
            case ViewType.DyxDyz:
                // viewTextureArray.SetFloat("_Layer", 2);
                Graphics.Blit(oceanMap.spectrumDyxDyz, destination);
                break;
            case ViewType.DxxDzz:
                // viewTextureArray.SetFloat("_Layer", 3);
                Graphics.Blit(oceanMap.spectrumDxxDzz, destination);
                break;
            case ViewType.Butterfly:
                Graphics.Blit(oceanMap.butterfly, destination);
                break;
            case ViewType.Buffer:
                Graphics.Blit(oceanMap.buffer, destination);
                break;
            case ViewType.Displacement:
                Graphics.Blit(oceanMap.displacement, destination);
                break;
                // case ViewType.PingPong0:
                //     Graphics.Blit(oceanMap.ping_pong0_buffer, destination);
                //     break;
                // case ViewType.PingPong1:
                //     Graphics.Blit(oceanMap.ping_pong1_buffer, destination);
                //     break;
                // case ViewType.Height:
                //     Graphics.Blit(oceanMap.height_buffer, destination);
                //     break;

        }
    }
}
