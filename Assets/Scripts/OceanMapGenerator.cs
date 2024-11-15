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
    public float windSpeed = 1;
    public Vector2 windDirection;
    public int lengthScale = 1;
    public float amplitude = 1;
    public float depth = 1;
    public bool doIfft = false;
    public float lowPass = 0.0f;
    public float highPass = 1.0f;

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
    [NonSerialized]
    public RenderTexture slope;

    private int size;
    private int logSize;
    private int threadGroups;

    //Kernel IDs
    int KERNEL_INIT_SPECTRUM;
    int KERNEL_CONJUGATE_SPECTRUM;
    int KERNEL_UPDATE_SPECTRUM;
    int KERNEL_HORIZONTAL_IFFT;
    int KERNEL_VERTICAL_IFFT;
    int KERNEL_POST_PROCESS;
    int KERNEL_ASSEMBLE;

    [Serializable]
    public struct DisplaySpectrumSettings
    {
        [Range(0, 1)]
        public float scale;
        public float windSpeed;
        public float windDirection;
        public float fetch;
        [Range(0, 1)]
        public float spreadBlend;
        [Range(0, 1)]
        public float swell;
        public float peakEnhancement;
        public float shortWavesFade;
    }
    public struct SpectrumSettings
    {
        public float scale;
        public float angle;
        public float spreadBlend;
        public float swell;
        public float alpha;
        public float peakOmega;
        public float gama;
        public float shortWavesFade;
    }

    public DisplaySpectrumSettings display;
    private SpectrumSettings[] settings = new SpectrumSettings[1];
    private ComputeBuffer settingsBuffer;

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

    void IFFT(RenderTexture input)
    {
        fftShader.SetTexture(KERNEL_HORIZONTAL_IFFT, "_Target", input);
        fftShader.Dispatch(KERNEL_HORIZONTAL_IFFT, 1, size, 1);

        fftShader.SetTexture(KERNEL_VERTICAL_IFFT, "_Target", input);
        fftShader.Dispatch(KERNEL_VERTICAL_IFFT, 1, size, 1);

        fftShader.SetBool("_Scale", false);
        fftShader.SetBool("_Permute", true);
        fftShader.SetTexture(KERNEL_POST_PROCESS, "_Target", input);
        fftShader.Dispatch(KERNEL_POST_PROCESS, size, size, 1);
    }

    void SetKernelIDs()
    {
        KERNEL_INIT_SPECTRUM = oceanographicSpectraShader.FindKernel("CS_InitializeSpectrum");
        KERNEL_CONJUGATE_SPECTRUM = oceanographicSpectraShader.FindKernel("CS_ConjugateSpectrum");
        KERNEL_UPDATE_SPECTRUM = oceanographicSpectraShader.FindKernel("CS_UpdateSpectrum");
        KERNEL_HORIZONTAL_IFFT = fftShader.FindKernel("CS_HorzontalStepIFFT");
        KERNEL_VERTICAL_IFFT = fftShader.FindKernel("CS_VerticalStepIFFT");
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
        buffer = CreateRenderTexture(size, size, RenderTextureFormat.RGFloat);
        displacement = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
        slope = CreateRenderTexture(size, size, RenderTextureFormat.ARGBFloat);
    }

    float JonswapAlpha(float g, float fetch, float windSpeed)
    {
        return 0.076f * Mathf.Pow(g * fetch / windSpeed / windSpeed, -0.22f);
    }

    float JonswapPeakFrequency(float g, float fetch, float windSpeed)
    {
        return 22 * Mathf.Pow(windSpeed * fetch / g / g, -0.33f);
    }

    void SetSpectrumUniforms()
    {
        oceanographicSpectraShader.SetInt("_LengthScale", lengthScale);
        oceanographicSpectraShader.SetInt("_MapSize", size);
        oceanographicSpectraShader.SetFloat("_Amplitude", amplitude);
        oceanographicSpectraShader.SetFloat("_WindSpeed", windSpeed);
        oceanographicSpectraShader.SetVector("_WindDirection", windDirection);
        oceanographicSpectraShader.SetFloat("_Depth", depth);
        oceanographicSpectraShader.SetFloat("_Lambda", 1);
        oceanographicSpectraShader.SetFloat("_LowPass", lowPass);
        oceanographicSpectraShader.SetFloat("_HighPass", highPass);

        settings[0].scale = display.scale;
        settings[0].angle = display.windDirection / 180.0f * Mathf.PI;
        settings[0].spreadBlend = display.spreadBlend;
        settings[0].swell = Mathf.Clamp(display.swell, 0.01f, 1);
        //TODO: Implement user controlled gravity
        settings[0].alpha = JonswapAlpha(9.81f, display.fetch, display.windSpeed);
        settings[0].peakOmega = JonswapPeakFrequency(9.81f, display.fetch, display.windSpeed);
        settings[0].gama = display.peakEnhancement;
        settings[0].shortWavesFade = display.shortWavesFade;

        settingsBuffer.SetData(settings);
        oceanographicSpectraShader.SetBuffer(KERNEL_INIT_SPECTRUM, "_Spectrum", settingsBuffer);


        Shader.SetGlobalFloat("_LengthScale", lengthScale);

    }



    void Awake()
    {
        size = (int)mapResolution;
        logSize = (int)Mathf.Log(size, 2.0f);
        threadGroups = Mathf.CeilToInt(size / 8.0f);

        settingsBuffer = new ComputeBuffer(1, 8 * sizeof(float));

        SetKernelIDs();
        InitializeRenderTextures();
        SetSpectrumUniforms();

        //Generate Initial Spectrum
        oceanographicSpectraShader.SetTexture(KERNEL_INIT_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        oceanographicSpectraShader.SetTexture(KERNEL_INIT_SPECTRUM, "_WavesData", wavesData);
        oceanographicSpectraShader.Dispatch(KERNEL_INIT_SPECTRUM, threadGroups, threadGroups, 1);

        oceanographicSpectraShader.SetTexture(KERNEL_CONJUGATE_SPECTRUM, "_InitialSpectrum", initialSpectrum);
        oceanographicSpectraShader.Dispatch(KERNEL_CONJUGATE_SPECTRUM, threadGroups, threadGroups, 1);

        //Generate Time Dependent Spectrum
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDxDz", spectrumDxDz);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDyDxy", spectrumDyDxy);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDyxDyz", spectrumDyxDyz);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_SpectrumDxxDzz", spectrumDxxDzz);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_WavesData", wavesData);
        oceanographicSpectraShader.SetTexture(KERNEL_UPDATE_SPECTRUM, "_InitialSpectrum", initialSpectrum);

        // //Assemble
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDxDz", spectrumDxDz);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDyDxy", spectrumDyDxy);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDyxDyz", spectrumDyxDyz);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_SpectrumDxxDzz", spectrumDxxDzz);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_Displacement", displacement);
        oceanographicSpectraShader.SetTexture(KERNEL_ASSEMBLE, "_Slope", slope);
    }


    void Update()
    {
        SetSpectrumUniforms();
        oceanographicSpectraShader.Dispatch(KERNEL_INIT_SPECTRUM, threadGroups, threadGroups, 1);
        oceanographicSpectraShader.Dispatch(KERNEL_CONJUGATE_SPECTRUM, threadGroups, threadGroups, 1);
        oceanographicSpectraShader.Dispatch(KERNEL_UPDATE_SPECTRUM, threadGroups, threadGroups, 1);
        if (doIfft)
        {
            IFFT(spectrumDxDz);
            IFFT(spectrumDyDxy);
            IFFT(spectrumDyxDyz);
            IFFT(spectrumDxxDzz);
        }
        oceanographicSpectraShader.Dispatch(KERNEL_ASSEMBLE, threadGroups, threadGroups, 1);
    }

    void OnApplicationQuit()
    {
        initialSpectrum.Release();
        wavesData.Release();
        spectrumDxDz.Release();
        spectrumDyDxy.Release();
        spectrumDyxDyz.Release();
        spectrumDxxDzz.Release();
        buffer.Release();
        displacement.Release();
        slope.Release();


        settingsBuffer.Dispose();
    }

}
