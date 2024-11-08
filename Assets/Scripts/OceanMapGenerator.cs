using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    public ComputeShader oceanographicSpectraShader;
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
    public bool doIfft = false;


    [NonSerialized]
    public RenderTexture initialSpectrum;
    [NonSerialized]
    public RenderTexture spectrumDxDz;
    [NonSerialized]
    public RenderTexture spectrumDyDxy;
    [NonSerialized]
    public RenderTexture spectrumDyxDyz;
    [NonSerialized]
    public RenderTexture spectrumDxxDzz;
    [NonSerialized]
    public RenderTexture wavesData;
    [NonSerialized]
    public RenderTexture butterfly;
    [NonSerialized]
    public RenderTexture buffer;
    [NonSerialized]
    public RenderTexture displacement;
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
    int KERNEL_CONJUGATE_SPECTRUM;
    int KERNEL_UPDATE_SPECTRUM;
    int KERNEL_PRECOMPUTE_BUTTERFLY;
    int KERNEL_HORIZONTAL_IFFT;
    int KERNEL_VERTICAL_IFFT;
    int KERNEL_POST_PROCESS;
    int KERNEL_PERMUTE;
    int KERNEL_ASSEMBLE;




    RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat format, bool useMipMaps = false)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear);
        rt.useMipMap = useMipMaps;
        rt.autoGenerateMips = false;
        rt.anisoLevel = 6;
        rt.filterMode = FilterMode.Trilinear;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    RenderTexture PrecomputeButterfly()
    {
        RenderTexture rt = new RenderTexture(logSize, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rt.filterMode = FilterMode.Point;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.Create();

        fftShader.SetInt("_MapSize", size);
        fftShader.SetTexture(KERNEL_PRECOMPUTE_BUTTERFLY, "_PrecomputeBuffer", rt);
        fftShader.Dispatch(KERNEL_PRECOMPUTE_BUTTERFLY, logSize, size / 2, 1);
        return rt;
    }
    void IFFT(RenderTexture input, bool outputToInput = false)
    {
        // fftShader.SetInt("_MapSize", size);

        // fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_PingPong0", input);
        // fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_PingPong1", buffer);
        // fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_Butterfly", butterfly);

        // bool pingPongFlag = false;

        // for (int i = 0; i < logSize; i++)
        // {
        //     pingPongFlag = !pingPongFlag;
        //     fftShader.SetInt("_Step", i);
        //     fftShader.SetBool("_PingPongFlag", pingPongFlag);
        //     fftShader.Dispatch(KERNEL_HORIZONTAL_IFFT, threadGroups, threadGroups, 1);
        // }

        // fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_PingPong0", input);
        // fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_PingPong1", buffer);
        // fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_Butterfly", butterfly);

        // for (int i = 0; i < logSize; i++)
        // {
        //     pingPongFlag = !pingPongFlag;
        //     fftShader.SetInt("_Step", i);
        //     fftShader.SetBool("_PingPongFlag", pingPongFlag);
        //     fftShader.Dispatch(KERNEL_VERTICAL_IFFT, threadGroups, threadGroups, 1);
        // }

        // if (pingPongFlag && outputToInput)
        // {
        //     Graphics.Blit(buffer, input);
        // }

        // if (!pingPongFlag && !outputToInput)
        // {
        //     Graphics.Blit(input, buffer);
        // }

        // //Permute
        // fftShader.SetTexture(KERNEL_PERMUTE, "_PingPong0", input);
        // fftShader.Dispatch(KERNEL_PERMUTE, threadGroups, threadGroups, 1);

        //---

        fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_Target", input);
        fftShader.Dispatch(KERNEL_HORIZONTAL_IFFT, 1, size, 1);

        fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_Target", input);
        fftShader.Dispatch(KERNEL_VERTICAL_IFFT, 1, size, 1);

        fftShader.SetBool("_Scale", true);
        fftShader.SetBool("_Permute", true);
        fftShader.SetTexture(KERNEL_POST_PROCESS, "_Target", input);
        fftShader.Dispatch(KERNEL_POST_PROCESS, size, size, 1);

    }

    void SetKernelIDs()
    {
        KERNEL_INIT_SPECTRUM = oceanographicSpectraShader.FindKernel("CS_InitializeSpectrum");
        KERNEL_CONJUGATE_SPECTRUM = oceanographicSpectraShader.FindKernel("CS_ConjugateSpectrum");
        KERNEL_UPDATE_SPECTRUM = oceanographicSpectraShader.FindKernel("CS_UpdateSpectrum");
        KERNEL_PRECOMPUTE_BUTTERFLY = fftShader.FindKernel("CS_PrecomputeButtefly");
        KERNEL_HORIZONTAL_IFFT = fftShader.FindKernel("CS_HorzontalStepIFFT");
        KERNEL_VERTICAL_IFFT = fftShader.FindKernel("CS_VerticalStepIFFT");
        KERNEL_PERMUTE = fftShader.FindKernel("CS_Permute");
        KERNEL_POST_PROCESS = fftShader.FindKernel("CS_PostProcess");
        KERNEL_ASSEMBLE = oceanographicSpectraShader.FindKernel("CS_Assemble");
    }
    void InitializeRenderTextures()
    {
        //TODO: Experiment with ARGBHalf
        initialSpectrum = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
        wavesData = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
        spectrumDxDz = CreateRenderTexture(size, size, RenderTextureFormat.RGFloat);
        spectrumDyDxy = CreateRenderTexture(size, size, RenderTextureFormat.RGFloat);
        spectrumDyxDyz = CreateRenderTexture(size, size, RenderTextureFormat.RGFloat);
        spectrumDxxDzz = CreateRenderTexture(size, size, RenderTextureFormat.RGFloat);
        butterfly = PrecomputeButterfly();
        buffer = CreateRenderTexture(size, size, RenderTextureFormat.RGFloat);
        displacement = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
    }
    void SetSpectrumUniforms()
    {
        oceanographicSpectraShader.SetInt("_L", L);
        oceanographicSpectraShader.SetInt("_MapSize", size);
        oceanographicSpectraShader.SetFloat("_A", A);
        oceanographicSpectraShader.SetFloat("_WindSpeed", windSpeed);
        oceanographicSpectraShader.SetVector("_WindDirection", windDirection);
        oceanographicSpectraShader.SetFloat("_Depth", depth);
        oceanographicSpectraShader.SetFloat("_Lambda", 1);
    }


    void Awake()
    {
        size = (int)mapResolution;
        logSize = (int)Mathf.Log(size, 2.0f);
        threadGroups = Mathf.CeilToInt(size / 8.0f);

        SetKernelIDs();
        InitializeRenderTextures();
        SetSpectrumUniforms();

        //Generate Initial Spectrum
        oceanographicSpectraShader.SetTexture(KERNEL_INIT_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        oceanographicSpectraShader.SetTexture(KERNEL_INIT_SPECTRUM, "_WavesData", wavesData);
        oceanographicSpectraShader.Dispatch(KERNEL_INIT_SPECTRUM, 512, 512, 1);

        oceanographicSpectraShader.SetTexture(KERNEL_CONJUGATE_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        oceanographicSpectraShader.Dispatch(KERNEL_CONJUGATE_SPECTRUM, 512, 512, 1);

        //Generate Time Dependent Spectrum
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDxDz", spectrumDxDz);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDyDxy", spectrumDyDxy);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDyxDyz", spectrumDyxDyz);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDxxDzz", spectrumDxxDzz);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_WavesData", wavesData);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        // oceanographicSpectraShader.Dispatch(KERNEL_UPDATE_SPECTRUM, threadGroups, threadGroups, 1);

        // IFFT(spectrumDxDz, true);
        // IFFT(spectrumDyDxy);
        // IFFT(spectrumDyxDyz);
        // IFFT(spectrumDxxDzz);

        // //Assemble
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDxDz", spectrumDxDz);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDyDxy", spectrumDyDxy);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDyxDyz", spectrumDyxDyz);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDxxDzz", spectrumDxxDzz);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_Displacement", displacement);

    }


    void Update()
    {
        SetSpectrumUniforms();
        oceanographicSpectraShader.Dispatch(KERNEL_INIT_SPECTRUM, 512, 512, 1);
        oceanographicSpectraShader.Dispatch(KERNEL_CONJUGATE_SPECTRUM, 512, 512, 1);
        // oceanographicSpectraShader.Dispatch(KERNEL_UPDATE_SPECTRUM, 512, 512, 1);
        if (doIfft)
        {
            IFFT(initialSpectrum);
            // IFFT(spectrumDxDz);
            // IFFT(spectrumDyDxy);
            // IFFT(spectrumDyxDyz);
            // IFFT(spectrumDxxDzz);
        }
    }

    void OnApplicationQuit()
    {
        initialSpectrum.Release();
        wavesData.Release();
        spectrumDxDz.Release();
        spectrumDyDxy.Release();
        spectrumDyxDyz.Release();
        spectrumDxxDzz.Release();
        butterfly.Release();
        buffer.Release();
        displacement.Release();
    }

}
