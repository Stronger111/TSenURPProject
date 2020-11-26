using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName ="Rendering/TSen Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    #region Batching
    [SerializeField]
    bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true;
    #endregion
    #region Shadow
    [SerializeField]
    ShadowSettings shadows = default;
    #endregion
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher,shadows);
    }
}
