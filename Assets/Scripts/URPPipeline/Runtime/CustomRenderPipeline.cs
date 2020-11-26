using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    #region 配置信息
    bool useDynamicBathcing, useGPUInstancing;
    /// <summary>
    /// 阴影设置
    /// </summary>
    ShadowSettings shadowSettings;
    #endregion
    public CustomRenderPipeline(bool useDynamicBathcing,bool useGPUInstancing,bool useSRPBatcher,ShadowSettings shadowSettings)
    {
        this.shadowSettings = shadowSettings;
        this.useDynamicBathcing = useDynamicBathcing;
        this.useGPUInstancing = useGPUInstancing;
        //开启SRP Batch 会优先使用
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        //是用Gamma颜色还是线性颜色
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    /// <summary>
    /// 引用单个摄像机Render
    /// </summary>
    CameraRenderer renderer = new CameraRenderer();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //遍历场景所有摄像机进行渲染,前向渲染 缺陷每个摄像机的渲染方式相同,可以让每个摄像机使用不同的渲染方式
        foreach(Camera camera in cameras)
        {
            renderer.Render(context,camera, useDynamicBathcing, useGPUInstancing,shadowSettings);
        }
    }
}
