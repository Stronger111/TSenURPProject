using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// URP 描述单个摄像机Render渲染
/// 2.2:画天空球
/// </summary>
public class CameraRenderer 
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
    #region Shader
    /// <summary>
    /// Default Unlit Shader Pass
    /// </summary>
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    /// <summary>
    /// TSen RP 不支持的Shader类型
    /// </summary>
    static ShaderTagId[] LegacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    #endregion
    public void Render(ScriptableRenderContext contex,Camera camera)
    {
        this.context = contex;
        this.camera = camera;

        if(!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry();
        //Un support shader
        DrawUnsupportedShaders();
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
    void DrawVisibleGeometry()
    {
        //先画不透明物体 顺序是从前往后画 不透明 在半透明物体
        var sortingSettings = new SortingSettings(camera) { criteria=SortingCriteria.CommonOpaque};
        var drawingSettings = new DrawingSettings(unlitShaderTagId,sortingSettings);
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
        //第三个参数背景清除 Color.clear (0,0,0,0) 完全透明 Profiler
        buffer.ClearRenderTarget(true, true, Color.clear);

        buffer.BeginSample(bufferName);
        ExcuteBuffer();
    }
    void Submit()
    {
        buffer.EndSample(bufferName);
        //Buffer进行操作执行
        ExcuteBuffer();
        context.Submit();
    }
    /// <summary>
    /// 不支持的Shader
    /// </summary>
    void DrawUnsupportedShaders()
    {
        var drawingSettings = new DrawingSettings(LegacyShaderTagIds[0],new SortingSettings(camera));
        //Multi Pass
        for(int i=1;i<LegacyShaderTagIds.Length;i++)
        {
            drawingSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);
        }
        var filterSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filterSettings);
    }
}
