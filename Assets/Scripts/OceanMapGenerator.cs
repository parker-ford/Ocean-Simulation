using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanMapGenerator : MonoBehaviour
{

    public int mapResolution = 256;
    public ComputeShader initialFrequencyShader;
    public float windSpeed = 1;
    public Vector2 windDirection;
    public int L = 1;
    public float A = 1;



    [NonSerialized]
    public ComputeBuffer h0k_buffer;
    [NonSerialized]
    public ComputeBuffer h0k_inv_buffer;

    void SetInitialFrequencyValues()
    {
        initialFrequencyShader.SetInt("_L", L);
        initialFrequencyShader.SetFloat("_A", A);
        initialFrequencyShader.SetFloat("_WindSpeed", windSpeed);
        initialFrequencyShader.SetVector("_WindDirection", windDirection);
    }

    void Awake()
    {
        h0k_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        h0k_inv_buffer = new ComputeBuffer(mapResolution * mapResolution, sizeof(float) * 2);
        initialFrequencyShader.SetBuffer(0, "h0k_buffer", h0k_buffer);
        initialFrequencyShader.SetBuffer(0, "h0k_inv_buffer", h0k_inv_buffer);
        initialFrequencyShader.SetFloat("resolution", mapResolution);
        SetInitialFrequencyValues();
    }

    // Update is called once per frame
    void Update()
    {
        SetInitialFrequencyValues();
        initialFrequencyShader.Dispatch(0, mapResolution, mapResolution, 1);
    }

    void OnApplicationQuit()
    {
        h0k_buffer.Release();
        h0k_inv_buffer.Release();
    }

}
