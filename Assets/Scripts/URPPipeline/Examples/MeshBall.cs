﻿using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// Draw Mesh Instancing
/// </summary>
public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor"),
        metallicId=Shader.PropertyToID("_Metallic"),smoothnessId=Shader.PropertyToID("_Smoothness");

    [SerializeField]
    Mesh mesh = default;
    [SerializeField]
    Material material = default;
    [SerializeField]
    LightProbeProxyVolume lightProbeVolume = null;

    Matrix4x4[] matrices = new Matrix4x4[1023];
    Vector4[] baseColor = new Vector4[1023];
    float[] metallic = new float[1023],smoothness=new float[1023];
    
    MaterialPropertyBlock block;
    
    private void Awake()
    {
        for(int i=0;i< matrices.Length;i++)
        {
            matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere*10f,Quaternion.Euler(Random.value*360f, Random.value * 360f, Random.value * 360f),Vector3.one*Random.Range(0.5f,1.5f));
            baseColor[i] = new Vector4(Random.value,Random.value,Random.value, Random.Range(0.5f, 1f));

            metallic[i] = Random.value < 0.25f ? 1.0f : 0.0f;
            smoothness[i] = Random.Range(0.05f,0.95f);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame Instance 支持全局光照
    void Update()
    {
        if(block==null)
        {
            block = new MaterialPropertyBlock();
            block.SetVectorArray(baseColorId,baseColor);
            block.SetFloatArray(metallicId,metallic);
            block.SetFloatArray(smoothnessId,smoothness);
            if(!lightProbeVolume)
            {
                var positions = new Vector3[1023];
                for (int i = 0; i < matrices.Length; i++)
                {
                    positions[i] = matrices[i].GetColumn(3);
                }

                var lightProbes = new SphericalHarmonicsL2[1023];
                LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, lightProbes, null);

                block.CopySHCoefficientArraysFrom(lightProbes);
            }
           
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, 1023, block, ShadowCastingMode.On, true, 0, null, lightProbeVolume ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided,lightProbeVolume);
    }
}
