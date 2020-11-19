using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// URP 描述单个摄像机Render渲染
/// 2.2:画天空球
/// </summary>
public partial class CameraRenderer 
{
    /// <summary>
    /// 渲染上下文
    /// </summary>
    ScriptableRenderContext context;
    /// <summary>
    /// 摄像机
    /// </summary>
    Camera camera;
    /// <summary>
    /// Command Buffer Name
    /// </summary>
    const string bufferName = "Render Camera";
    /// <summary>
    /// Command Buufer 非托管对象 手动Dispose
    /// </summary>
    CommandBuffer buffer = new CommandBuffer { name=bufferName};
    #region 灯光配置
    Lighting lighting = new Lighting();
    #endregion
    #region Shader Tags
    /// <summary>
    /// Default Unlit Shader Pass
    /// </summary>
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        litShaderTagId=new ShaderTagId("CustomLit");
    #endregion
    public void Render(ScriptableRenderContext contex,Camera camera,bool useDynamicBatching,bool useGPUInstancing)
    {
        this.context = contex;
        this.camera = camera;
        //每个摄像机有自己的Buffer
        PrepareBuffer();
        //UI
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }

        Setup();
        //灯光配置
        lighting.Setup(contex,cullingResults);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        //Un support shader
        DrawUnsupportedShaders();
        //Gizmos 线
        DrawGizmos();
        Submit();
    }
    /// <summary>
    /// 剔除的结果
    /// </summary>
    CullingResults cullingResults;
    bool Cull()
    {
        //剔除参数结构
        if(camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 画可见的几何体
    /// </summary>
    void DrawVisibleGeometry(bool useDynamicBathching,bool useGPUInstancing)
    {
        //先画不透明物体 顺序是从前往后画 不透明 在半透明物体
        var sortingSettings = new SortingSettings(camera) { criteria=SortingCriteria.CommonOpaque};
        //开启动态合批 关闭GPU Instancing Draw unlitShader 和 litShaderTagId
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { enableDynamicBatching= useDynamicBathching, enableInstancing= useGPUInstancing };
        drawingSettings.SetShaderPassName(1,litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        //Draw Opaque Visible Renderer
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);

        //还没提交到GPU Queque队列里面 CPU维护了一个Command List队列
        context.DrawSkybox(camera);

        //画半透明物体 半透明物体不写入深度
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);
    }
    void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    void Setup()
    {
        //设置摄像机属性给全局shader，不设置摄像机旋转 天空球并不会旋转
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        //第三个参数背景清除 Color.clear (0,0,0,0) 完全透明 Profiler
        buffer.ClearRenderTarget(flags<=CameraClearFlags.Depth, flags==CameraClearFlags.Color,flags==CameraClearFlags.Color?camera.backgroundColor.linear: Color.clear);

        buffer.BeginSample(SampleName);
        ExcuteBuffer();
    }
    void Submit()
    {
        buffer.EndSample(SampleName);
        //Buffer进行操作执行
        ExcuteBuffer();
        context.Submit();
    }
}
