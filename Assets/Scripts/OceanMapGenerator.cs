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
    public float windSpeed = 1;
    public Vector2 windDirection;
    public int L = 1;
    public float A = 1;



    [NonSerialized]
    public ComputeBuffer h0k_buffer;
    [NonSerialized]
    public ComputeBuffer h0k_inv_buffer;
    [NonSerialized]
    public ComputeBuffer htk_dx_buffer;
    [NonSerialized]
    public ComputeBuffer htk_dy_buffer;
    [NonSerialized]
    public ComputeBuffer htk_dz_buffer;
    [NonSerialized]
    public ComputeBuffer butterfly_buffer;

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
        butterflyShader.Dispatch(0, 8, mapResolution, 1);
    }

    void Awake()
    {
        //Initialize Buffers
        h0k_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        h0k_inv_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        htk_dx_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        htk_dy_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        htk_dz_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        butterfly_buffer = new ComputeBuffer(8 * mapResolution, sizeof(float) * 4);

        //Initialize Shader varialbes
        initialFrequencyShader.SetFloat("resolution", mapResolution);
        initialFrequencyShader.SetBuffer(0, "h0k_buffer", h0k_buffer);
        initialFrequencyShader.SetBuffer(0, "h0k_inv_buffer", h0k_inv_buffer);

        timeFrequencyShader.SetFloat("resolution", mapResolution);
        timeFrequencyShader.SetBuffer(0, "h0k_buffer", h0k_buffer);
        timeFrequencyShader.SetBuffer(0, "h0k_inv_buffer", h0k_inv_buffer);
        timeFrequencyShader.SetBuffer(0, "htk_dx_buffer", htk_dx_buffer);
        timeFrequencyShader.SetBuffer(0, "htk_dy_buffer", htk_dy_buffer);
        timeFrequencyShader.SetBuffer(0, "htk_dz_buffer", htk_dz_buffer);

        butterflyShader.SetFloat("resolution", mapResolution);
        butterflyShader.SetBuffer(0, "butterfly_buffer", butterfly_buffer);

        //Generate Butterfly Buffer
        GenerateButterflyValues();

        //Generate Initial Fequencies
        GenerateInitialFrequencyValues();
    }

    // Update is called once per frame
    void Update()
    {
        //Generate Time Frequencies
        GenerateTimeFrequencyValues();
    }

    void OnApplicationQuit()
    {
        h0k_buffer.Release();
        h0k_inv_buffer.Release();
        htk_dx_buffer.Release();
        htk_dy_buffer.Release();
        htk_dz_buffer.Release();
    }

}
