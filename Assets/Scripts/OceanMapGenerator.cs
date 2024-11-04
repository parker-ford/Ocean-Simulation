using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanMapGenerator : MonoBehaviour
{

    public int mapResolution = 256;
    public ComputeShader initialFrequencyShader;
    public ComputeShader timeFrequencyShader;
    public ComputeShader butterflyShader;
    public ComputeShader pingPongShader;
    public ComputeShader inversionShader;
    public ComputeShader fillPingPongShader;
    public float windSpeed = 1;
    public Vector2 windDirection;
    public int L = 1;
    public float A = 1;


    [NonSerialized]
    public RenderTexture h0k_buffer;
    [NonSerialized]
    public RenderTexture h0k_inv_buffer;
    [NonSerialized]
    public RenderTexture htk_dx_buffer;
    [NonSerialized]
    public RenderTexture htk_dy_buffer;
    [NonSerialized]
    public RenderTexture htk_dz_buffer;
    [NonSerialized]
    public RenderTexture butterfly_buffer;
    [NonSerialized]
    public RenderTexture ping_pong0_buffer;
    [NonSerialized]
    public RenderTexture ping_pong1_buffer;
    [NonSerialized]
    public RenderTexture height_buffer;


    private ComputeBuffer bit_reversed_buffer;

    void GenerateInitialFrequencyValues()
    {
        initialFrequencyShader.SetInt("_L", L);
        initialFrequencyShader.SetFloat("_A", A);
        initialFrequencyShader.SetFloat("_WindSpeed", windSpeed);
        initialFrequencyShader.SetVector("_WindDirection", windDirection);
        initialFrequencyShader.Dispatch(0, mapResolution, mapResolution, 1);
    }

    void GenerateTimeFrequencyValues()
    {
        timeFrequencyShader.SetInt("_L", L);
        timeFrequencyShader.Dispatch(0, mapResolution, mapResolution, 1);
    }

    void GenerateButterflyValues()
    {
        butterflyShader.Dispatch(0, (int)Mathf.Log(mapResolution, 2), mapResolution, 1);
    }

    void GenerateBitReverseIndex()
    {
        int[] reversedBits = new int[(int)mapResolution];
        int bits = (int)Mathf.Log(mapResolution, 2);
        for (int num = 0; num < mapResolution; num++)
        {
            int reversed = 0;
            int temp = num;
            for (int i = 0; i < bits; i++)
            {
                reversed <<= 1;

                reversed |= (temp & 1);

                temp >>= 1;
            }
            reversedBits[num] = reversed;
        }
        bit_reversed_buffer.SetData(reversedBits);
    }

    void GeneratePingPongValues()
    {

        fillPingPongShader.SetTexture(0, "freqData", htk_dy_buffer);
        fillPingPongShader.Dispatch(0, mapResolution, mapResolution, 1);


        int pingPong = 0;

        pingPongShader.SetInt("_Direction", 0);
        pingPongShader.SetInt("_PingPong", pingPong);

        for (int i = 0; i < (int)MathF.Log(mapResolution, 2); i++)
        {
            pingPongShader.SetInt("_Stage", i);
            pingPongShader.Dispatch(0, mapResolution, mapResolution, 1);
            pingPong = (pingPong + 1) % 2;
            pingPongShader.SetInt("_PingPong", pingPong);
        }

        pingPongShader.SetInt("_Direction", 1);

        for (int i = 0; i < (int)MathF.Log(mapResolution, 2); i++)
        {
            pingPongShader.SetInt("_Stage", i);
            pingPongShader.Dispatch(0, mapResolution, mapResolution, 1);
            pingPong = (pingPong + 1) % 2;
            pingPongShader.SetInt("_PingPong", pingPong);

        }

        inversionShader.SetInt("_PingPong", pingPong);
        inversionShader.Dispatch(0, mapResolution, mapResolution, 1);

    }

    RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat format, bool useMipMaps = false)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear);
        rt.useMipMap = useMipMaps;
        rt.autoGenerateMips = false;
        // rt.anisoLevel = 6;
        // rt.filterMode 
        rt.filterMode = FilterMode.Point;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    void Awake()
    {
        //Initialize Buffers
        h0k_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.RGFloat);
        h0k_inv_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.RGFloat);
        htk_dx_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.RGFloat);
        htk_dy_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.RGFloat);
        htk_dz_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.RGFloat);
        butterfly_buffer = CreateRenderTexture((int)Mathf.Log(mapResolution, 2), mapResolution, RenderTextureFormat.ARGBFloat);
        bit_reversed_buffer = new ComputeBuffer(mapResolution, sizeof(int));
        ping_pong0_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.ARGBFloat);
        ping_pong1_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.ARGBFloat);
        height_buffer = CreateRenderTexture(mapResolution, mapResolution, RenderTextureFormat.ARGBFloat);

        //Generate bit reversed indices buffer
        GenerateBitReverseIndex();

        //Initialize Shader varialbes
        initialFrequencyShader.SetFloat("resolution", mapResolution);
        initialFrequencyShader.SetTexture(0, "h0k_buffer", h0k_buffer);
        initialFrequencyShader.SetTexture(0, "h0k_inv_buffer", h0k_inv_buffer);

        timeFrequencyShader.SetFloat("resolution", mapResolution);
        timeFrequencyShader.SetTexture(0, "h0k_buffer", h0k_buffer);
        timeFrequencyShader.SetTexture(0, "h0k_inv_buffer", h0k_inv_buffer);
        timeFrequencyShader.SetTexture(0, "htk_dx_buffer", htk_dx_buffer);
        timeFrequencyShader.SetTexture(0, "htk_dy_buffer", htk_dy_buffer);
        timeFrequencyShader.SetTexture(0, "htk_dz_buffer", htk_dz_buffer);

        butterflyShader.SetFloat("resolution", mapResolution);
        butterflyShader.SetTexture(0, "butterfly_buffer", butterfly_buffer);
        butterflyShader.SetBuffer(0, "bit_reversed_buffer", bit_reversed_buffer);

        pingPongShader.SetFloat("resolution", mapResolution);
        pingPongShader.SetTexture(0, "butterfly_buffer", butterfly_buffer);
        pingPongShader.SetTexture(0, "pingpong0", ping_pong0_buffer);
        pingPongShader.SetTexture(0, "pingpong1", ping_pong1_buffer);

        inversionShader.SetFloat("resolution", mapResolution);
        inversionShader.SetTexture(0, "pingpong0", ping_pong0_buffer);
        inversionShader.SetTexture(0, "pingpong1", ping_pong1_buffer);
        inversionShader.SetTexture(0, "height_buffer", height_buffer);

        fillPingPongShader.SetFloat("resolution", mapResolution);
        fillPingPongShader.SetTexture(0, "pingpong", ping_pong0_buffer);

        //Generate Butterfly Buffer
        GenerateButterflyValues();

        //Generate Initial Fequencies
        GenerateInitialFrequencyValues();

        //Generate Time Frequencies
        GenerateTimeFrequencyValues();

        //Generate Height map
        GeneratePingPongValues();

    }

    // Update is called once per frame
    void Update()
    {
        //Generate Initial Fequencies
        GenerateInitialFrequencyValues();

        //Generate Time Frequencies
        GenerateTimeFrequencyValues();

        //Generate Height map
        GeneratePingPongValues();
    }

    void OnApplicationQuit()
    {
        h0k_buffer.Release();
        h0k_inv_buffer.Release();
        htk_dx_buffer.Release();
        htk_dy_buffer.Release();
        htk_dz_buffer.Release();
        butterfly_buffer.Release();
        bit_reversed_buffer.Release();
        ping_pong0_buffer.Release();
        ping_pong1_buffer.Release();
        height_buffer.Release();
    }

}
