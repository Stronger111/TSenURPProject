using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName ="Rendering/TSen Render Pipeline")]
public partial class CustomRenderPipelineAsset : RenderPipelineAsset
{
    //[SerializeField]
    //bool allowHDR = true;
    [SerializeField]
    CameraBufferSettings cameraBuffer = new CameraBufferSettings { allowHDR = true };
    #region Batching
    [SerializeField]
    bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true,useLightsPerObject=true;
    #endregion
    #region Shadow
    [SerializeField]
    ShadowSettings shadows = default;
    #endregion
    #region PPS
    [SerializeField]
    PostFXSettings postFXSettings = default;
    public enum ColorLUTResolution {_16=16,_32=32,_64=64 }
    [SerializeField]
    ColorLUTResolution colorLUTResolution = ColorLUTResolution._32;
    [SerializeField]
    Shader cameraRendererShader = default;
    #endregion
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(cameraBuffer, useDynamicBatching, useGPUInstancing, 
            useSRPBatcher, useLightsPerObject,shadows,postFXSettings,(int)colorLUTResolution,cameraRendererShader);
    }
}
