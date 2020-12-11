using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName ="Rendering/TSen Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool allowHDR = true;
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
    #endregion
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(allowHDR,useDynamicBatching, useGPUInstancing, 
            useSRPBatcher, useLightsPerObject,shadows,postFXSettings,(int)colorLUTResolution);
    }
}
