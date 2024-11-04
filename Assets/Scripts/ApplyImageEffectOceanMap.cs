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
        InitialInverseFreq,
        DXFreq,
        DYFreq,
        DZFreq,
        Butterfly,
        PingPong0,
        PingPong1,
        Height,
    }
    public OceanMapGenerator oceanMap;
    public ViewType viewType = ViewType.InitialFreq;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        switch (viewType)
        {
            case ViewType.Source:
                Graphics.Blit(source, destination);
                break;
            case ViewType.InitialFreq:
                Graphics.Blit(oceanMap.h0k_buffer, destination);
                break;
            case ViewType.InitialInverseFreq:
                Graphics.Blit(oceanMap.h0k_inv_buffer, destination);
                break;
            case ViewType.DXFreq:
                Graphics.Blit(oceanMap.htk_dx_buffer, destination);
                break;
            case ViewType.DYFreq:
                Graphics.Blit(oceanMap.htk_dy_buffer, destination);
                break;
            case ViewType.DZFreq:
                Graphics.Blit(oceanMap.htk_dz_buffer, destination);
                break;
            case ViewType.Butterfly:
                Graphics.Blit(oceanMap.butterfly_buffer, destination);
                break;
            case ViewType.PingPong0:
                Graphics.Blit(oceanMap.ping_pong0_buffer, destination);
                break;
            case ViewType.PingPong1:
                Graphics.Blit(oceanMap.ping_pong1_buffer, destination);
                break;
            case ViewType.Height:
                Graphics.Blit(oceanMap.height_buffer, destination);
                break;

        }
    }
}
