using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 渲染管线
/// </summary>
public partial class CustomRenderPipeline : RenderPipeline
{
    CameraBufferSettings cameraBufferSettings;
    //bool allowHDR;
    #region 配置信息
    bool useDynamicBathcing, useGPUInstancing,useLightsPerObject;
    /// <summary>
    /// 阴影设置
    /// </summary>
    ShadowSettings shadowSettings;
    /// <summary>
    /// 后期处理
    /// </summary>
    PostFXSettings postFXSettings;
    int colorLUTResolution;
    #endregion
    public CustomRenderPipeline(CameraBufferSettings cameraBufferSettings,bool useDynamicBathcing,bool useGPUInstancing,bool useLightsPerObject,
        bool useSRPBatcher,ShadowSettings shadowSettings,PostFXSettings postFXSettings,int colorLUTResolution,Shader cameraRendererShader)
    {
        this.colorLUTResolution = colorLUTResolution;
        this.cameraBufferSettings = cameraBufferSettings;
        this.postFXSettings = postFXSettings;
        this.shadowSettings = shadowSettings;
        this.useDynamicBathcing = useDynamicBathcing;
        this.useGPUInstancing = useGPUInstancing;
        this.useLightsPerObject = useLightsPerObject;
        //开启SRP Batch 会优先使用
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        //是用Gamma颜色还是线性颜色
        GraphicsSettings.lightsUseLinearIntensity = true;
        //
        InitializeForEditor();
        renderer = new CameraRenderer(cameraRendererShader);
    }
    /// <summary>
    /// 引用单个摄像机Render
    /// </summary>
    CameraRenderer renderer;
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //遍历场景所有摄像机进行渲染,前向渲染 缺陷每个摄像机的渲染方式相同,可以让每个摄像机使用不同的渲染方式
        foreach(Camera camera in cameras)
        {
            renderer.Render(context,camera, cameraBufferSettings, useDynamicBathcing, useGPUInstancing, 
                useLightsPerObject,shadowSettings,postFXSettings, colorLUTResolution);
        }
    }
}
