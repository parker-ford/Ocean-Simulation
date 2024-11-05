using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanMapGenerator : MonoBehaviour
{

    public enum MapResolution
    {
        Res_64x64 = 64,
        Res_124x124 = 124,
        Res_256x256 = 256,
        Res_512x512 = 512,
        Res_1024x1024 = 1024,
        Res_2048x2048 = 2048
    }

    public MapResolution mapResolution = MapResolution.Res_256x256;
    public ComputeShader oceanagraphicSpectraShader;
    public ComputeShader fftShader;
    // public ComputeShader initialFrequencyShader;
    // public ComputeShader timeFrequencyShader;
    // public ComputeShader butterflyShader;
    // public ComputeShader pingPongShader;
    // public ComputeShader inversionShader;
    // public ComputeShader fillPingPongShader;
    public float windSpeed = 1;
    public Vector2 windDirection;
    public int L = 1;
    public float A = 1;
    public float depth = 1;


    [NonSerialized]
    public RenderTexture initialSpectrum;
    [NonSerialized]
    public RenderTexture spectrum;
    [NonSerialized]
    public RenderTexture wavesData;
    [NonSerialized]
    public RenderTexture butterfly;
    [NonSerialized]
    public RenderTexture pingPong0;
    [NonSerialized]
    public RenderTexture pingPong1;
    // [NonSerialized]
    // public RenderTexture htk_dx_buffer;
    // [NonSerialized]
    // public RenderTexture htk_dy_buffer;
    // [NonSerialized]
    // public RenderTexture htk_dz_buffer;
    // [NonSerialized]
    // public RenderTexture butterfly_buffer;
    // [NonSerialized]
    // public RenderTexture ping_pong0_buffer;
    // [NonSerialized]
    // public RenderTexture ping_pong1_buffer;
    // [NonSerialized]
    // public RenderTexture height_buffer;
    // private ComputeBuffer bit_reversed_buffer;

    private int size;
    private int logSize;
    private int threadGroups;

    //Kernel IDs
    int KERNEL_INIT_SPECTRUM;
    int KERNEL_UPDATE_SPECTRUM;
    int KERNEL_PRECOMPUTE_BUTTERFLY;
    int KERNEL_HORIZONTAL_IFFT;
    int KERNEL_VERTICAL_IFFT;


    // void GenerateInitialFrequencyValues()
    // {
    //     initialFrequencyShader.SetInt("_L", L);
    //     initialFrequencyShader.SetFloat("_A", A);
    //     initialFrequencyShader.SetFloat("_WindSpeed", windSpeed);
    //     initialFrequencyShader.SetVector("_WindDirection", windDirection);
    //     initialFrequencyShader.Dispatch(0, resolution, resolution, 1);
    // }

    // void GenerateTimeFrequencyValues()
    // {
    //     timeFrequencyShader.SetInt("_L", L);
    //     timeFrequencyShader.Dispatch(0, resolution, resolution, 1);
    // }

    // void GenerateButterflyValues()
    // {
    //     butterflyShader.Dispatch(0, (int)Mathf.Log(resolution, 2), resolution, 1);
    // }

    // void GenerateBitReverseIndex()
    // {
    //     int[] reversedBits = new int[(int)resolution];
    //     int bits = (int)Mathf.Log(resolution, 2);
    //     for (int num = 0; num < resolution; num++)
    //     {
    //         int reversed = 0;
    //         int temp = num;
    //         for (int i = 0; i < bits; i++)
    //         {
    //             reversed <<= 1;

    //             reversed |= (temp & 1);

    //             temp >>= 1;
    //         }
    //         reversedBits[num] = reversed;
    //     }
    //     bit_reversed_buffer.SetData(reversedBits);
    // }

    // void GeneratePingPongValues()
    // {

    //     for (int axis = 0; axis < 3; axis++)
    //     {
    //         if (axis == 0)
    //         {
    //             fillPingPongShader.SetTexture(0, "freqData", htk_dx_buffer);
    //         }
    //         else if (axis == 1)
    //         {
    //             fillPingPongShader.SetTexture(0, "freqData", htk_dy_buffer);
    //         }
    //         else if (axis == 2)
    //         {
    //             fillPingPongShader.SetTexture(0, "freqData", htk_dz_buffer);
    //         }

    //         fillPingPongShader.Dispatch(0, resolution, resolution, 1);


    //         int pingPong = 0;

    //         pingPongShader.SetInt("_Direction", 0);
    //         pingPongShader.SetInt("_PingPong", pingPong);

    //         for (int i = 0; i < (int)MathF.Log(resolution, 2); i++)
    //         {
    //             pingPongShader.SetInt("_Stage", i);
    //             pingPongShader.Dispatch(0, resolution, resolution, 1);
    //             pingPong = (pingPong + 1) % 2;
    //             pingPongShader.SetInt("_PingPong", pingPong);
    //         }

    //         pingPongShader.SetInt("_Direction", 1);

    //         for (int i = 0; i < (int)MathF.Log(resolution, 2); i++)
    //         {
    //             pingPongShader.SetInt("_Stage", i);
    //             pingPongShader.Dispatch(0, resolution, resolution, 1);
    //             pingPong = (pingPong + 1) % 2;
    //             pingPongShader.SetInt("_PingPong", pingPong);

    //         }

    //         inversionShader.SetInt("_Axis", axis);
    //         inversionShader.SetInt("_PingPong", pingPong);
    //         inversionShader.Dispatch(0, resolution, resolution, 1);
    //     }

    // }

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

    RenderTexture CreateRenderTexture(int width, int height, int depth, RenderTextureFormat format, bool useMipMaps = false)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear);
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        rt.useMipMap = useMipMaps;
        rt.autoGenerateMips = false;
        rt.filterMode = FilterMode.Point;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.volumeDepth = depth;
        rt.Create();
        return rt;
    }

    void IFFT()
    {
        bool pingPongFlag = false;
        for (int i = 0; i < logSize; i++)
        {
            pingPongFlag = !pingPongFlag;
            fftShader.SetInt("_Step", i);
            fftShader.SetBool("_PingPongFlag", pingPongFlag);
            fftShader.Dispatch(KERNEL_HORIZONTAL_IFFT, threadGroups, threadGroups, 1);
        }

        for (int i = 0; i < logSize; i++)
        {
            pingPongFlag = !pingPongFlag;
            fftShader.SetInt("_Step", i);
            fftShader.SetBool("_PingPongFlag", pingPongFlag);
            fftShader.Dispatch(KERNEL_VERTICAL_IFFT, threadGroups, threadGroups, 1);
        }

        //Permute

    }

    void Awake()
    {
        KERNEL_INIT_SPECTRUM = oceanagraphicSpectraShader.FindKernel("CS_InitializeSpectrum");
        KERNEL_UPDATE_SPECTRUM = oceanagraphicSpectraShader.FindKernel("CS_UpdateSpectrum");
        KERNEL_PRECOMPUTE_BUTTERFLY = fftShader.FindKernel("CS_PrecomputeButtefly");
        KERNEL_HORIZONTAL_IFFT = fftShader.FindKernel("CS_HorzontalStepIFFT");
        KERNEL_VERTICAL_IFFT = fftShader.FindKernel("CS_VerticalStepIFFT");

        size = (int)mapResolution;
        logSize = (int)Mathf.Log(size, 2.0f);
        threadGroups = Mathf.CeilToInt(size / 8.0f);

        //TODO: Experiment with ARGBHalf
        initialSpectrum = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
        wavesData = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
        spectrum = CreateRenderTexture(size, size, 4, RenderTextureFormat.RGFloat);
        butterfly = CreateRenderTexture(logSize, size, RenderTextureFormat.ARGBFloat);

        //TODO: Make this a texture array
        pingPong0 = CreateRenderTexture(size, size, RenderTextureFormat.RFloat);
        pingPong1 = CreateRenderTexture(size, size, RenderTextureFormat.RFloat);

        oceanagraphicSpectraShader.SetInt("_L", L);
        oceanagraphicSpectraShader.SetInt("_MapSize", size);
        oceanagraphicSpectraShader.SetFloat("_A", A);
        oceanagraphicSpectraShader.SetFloat("_WindSpeed", windSpeed);
        oceanagraphicSpectraShader.SetVector("_WindDirection", windDirection);
        oceanagraphicSpectraShader.SetFloat("_Depth", depth);

        oceanagraphicSpectraShader.SetTexture(KERNEL_INIT_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        oceanagraphicSpectraShader.SetTexture(KERNEL_INIT_SPECTRUM, "_WavesData", wavesData);
        oceanagraphicSpectraShader.Dispatch(KERNEL_INIT_SPECTRUM, threadGroups, threadGroups, 1);

        oceanagraphicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_Spectrum", spectrum);
        oceanagraphicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_WavesData", wavesData);
        oceanagraphicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        oceanagraphicSpectraShader.Dispatch(KERNEL_UPDATE_SPECTRUM, threadGroups, threadGroups, 1);

        fftShader.SetInt("_MapSize", size);

        fftShader.SetTexture(KERNEL_PRECOMPUTE_BUTTERFLY, "_Butterfly", butterfly);
        fftShader.Dispatch(KERNEL_PRECOMPUTE_BUTTERFLY, logSize, size, 1);

        fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_PingPong0", pingPong0);
        fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_PingPong1", pingPong1);
        fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_Butterfly", butterfly);

        fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_PingPong0", pingPong0);
        fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_PingPong1", pingPong1);
        fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_Butterfly", butterfly);

        // resolution = (int)mapResolution;
        //Initialize Buffers
        // h0k_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.RGFloat);
        // h0k_inv_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.RGFloat);

        // htk_dx_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.RGFloat);
        // htk_dy_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.RGFloat);
        // htk_dz_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.RGFloat);
        // butterfly_buffer = CreateRenderTexture((int)Mathf.Log(resolution, 2), resolution, RenderTextureFormat.ARGBFloat);
        // bit_reversed_buffer = new ComputeBuffer(resolution, sizeof(int));
        // ping_pong0_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.ARGBFloat);
        // ping_pong1_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.ARGBFloat);
        // height_buffer = CreateRenderTexture(resolution, resolution, RenderTextureFormat.ARGBFloat);

        // //Generate bit reversed indices buffer
        // GenerateBitReverseIndex();

        // //Initialize Shader varialbes
        // initialFrequencyShader.SetFloat("resolution", resolution);
        // initialFrequencyShader.SetTexture(0, "h0k_buffer", h0k_buffer);
        // initialFrequencyShader.SetTexture(0, "h0k_inv_buffer", h0k_inv_buffer);

        // timeFrequencyShader.SetFloat("resolution", resolution);
        // timeFrequencyShader.SetTexture(0, "h0k_buffer", h0k_buffer);
        // timeFrequencyShader.SetTexture(0, "h0k_inv_buffer", h0k_inv_buffer);
        // timeFrequencyShader.SetTexture(0, "htk_dx_buffer", htk_dx_buffer);
        // timeFrequencyShader.SetTexture(0, "htk_dy_buffer", htk_dy_buffer);
        // timeFrequencyShader.SetTexture(0, "htk_dz_buffer", htk_dz_buffer);

        // butterflyShader.SetFloat("resolution", resolution);
        // butterflyShader.SetTexture(0, "butterfly_buffer", butterfly_buffer);
        // butterflyShader.SetBuffer(0, "bit_reversed_buffer", bit_reversed_buffer);

        // pingPongShader.SetFloat("resolution", resolution);
        // pingPongShader.SetTexture(0, "butterfly_buffer", butterfly_buffer);
        // pingPongShader.SetTexture(0, "pingpong0", ping_pong0_buffer);
        // pingPongShader.SetTexture(0, "pingpong1", ping_pong1_buffer);

        // inversionShader.SetFloat("resolution", resolution);
        // inversionShader.SetTexture(0, "pingpong0", ping_pong0_buffer);
        // inversionShader.SetTexture(0, "pingpong1", ping_pong1_buffer);
        // inversionShader.SetTexture(0, "height_buffer", height_buffer);

        // fillPingPongShader.SetFloat("resolution", resolution);
        // fillPingPongShader.SetTexture(0, "pingpong", ping_pong0_buffer);

        // //Generate Butterfly Buffer
        // GenerateButterflyValues();

        // //Generate Initial Fequencies
        // GenerateInitialFrequencyValues();

        // //Generate Time Frequencies
        // GenerateTimeFrequencyValues();

        // //Generate Height map
        // GeneratePingPongValues();

    }

    // Update is called once per frame
    void Update()
    {

        oceanagraphicSpectraShader.Dispatch(KERNEL_UPDATE_SPECTRUM, threadGroups, threadGroups, 1);
        // //Generate Initial Fequencies
        // GenerateInitialFrequencyValues();

        // //Generate Time Frequencies
        // GenerateTimeFrequencyValues();

        // //Generate Height map
        // GeneratePingPongValues();
    }

    void OnApplicationQuit()
    {
        initialSpectrum.Release();
        // h0k_buffer.Release();
        // h0k_inv_buffer.Release();
        // htk_dx_buffer.Release();
        // htk_dy_buffer.Release();
        // htk_dz_buffer.Release();
        // butterfly_buffer.Release();
        // bit_reversed_buffer.Release();
        // ping_pong0_buffer.Release();
        // ping_pong1_buffer.Release();
        // height_buffer.Release();
    }

}
