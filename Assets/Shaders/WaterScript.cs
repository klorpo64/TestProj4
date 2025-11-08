using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WaterScript : MonoBehaviour
{
    Material material;
    float time = 0f;
    float[] amplitudes, magnitudes, frequencies;

    //Unity only takes vector4 for some reason in setVectorArray
    Vector4[] directions;

    [SerializeField, Range(1, 100)]
    private int numWaves = 50;
    [SerializeField]
    Color waterColor = new(70f/255, 170/255f, 200/255f), sunColor = new(1, 1, 0.8f);
    [SerializeField]
    Vector4 sunDirection = new Vector4(0, 0.7f, -0.5f, 0).normalized;
    [SerializeField]
    float ambientStrength = 0.1f, specularStrength = 64, diffuseStrength = 0.6f,
        sunIntensity = 3f, fresnelStrength = 10f;

    void Start()
    {
        material = GetComponent<Renderer>().material;

        magnitudes = new float[numWaves];
        frequencies = new float[numWaves];

        //random wave directions
        directions = new Vector4[numWaves];
        float[] wavelengths = new float[numWaves];
        amplitudes = new float[numWaves];

        UnityEngine.Random.InitState(123434);
        for (int i = 0; i < numWaves; i++)
        {
            directions[i] = new Vector2(UnityEngine.Random.Range(-1f, 1f),
                                        UnityEngine.Random.Range(-1f, 1f)).normalized;
            wavelengths[i] = UnityEngine.Random.Range(1f, 20f) / (i + 1);

            float k = 2f * 3.14f / wavelengths[i];

            amplitudes[i] = UnityEngine.Random.Range(.05f, .9f) / k / (i + 1);
            float w = Mathf.Sqrt(9.81f * k);
            Debug.Log(w + " " + amplitudes[i]);
            magnitudes[i] = k;
            frequencies[i] = w;
        }

        material.SetFloatArray("_Amplitudes", amplitudes);
        material.SetFloatArray("_WaveMagnitudes", magnitudes);
        material.SetFloatArray("_Frequencies", frequencies);
        material.SetVectorArray("_WaveDirections", directions);
        material.SetFloat("_time", 0);
    }

    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_time", time);
        time += Time.deltaTime;

        material.SetColor("_WaterColor", waterColor);
        material.SetColor("_SunColor", sunColor);

        material.SetVector("_SunDirection", sunDirection.normalized);

        material.SetFloat("_AmbientStrength", ambientStrength);
        material.SetFloat("_DiffuseStrength", diffuseStrength);
        material.SetFloat("_SpecularStrength", specularStrength);
        material.SetFloat("_SunIntensity", sunIntensity);
        material.SetFloat("_FresnelStrength", fresnelStrength);
    }
}
