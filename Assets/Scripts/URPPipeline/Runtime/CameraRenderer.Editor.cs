using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEditor;
/// <summary>
/// URP 描述单个摄像机Render渲染
/// 2.2:画天空球
/// </summary>
partial class CameraRenderer 
{
    partial void DrawGizmosBeforeFX();
    partial void DrawGizmosAfterFX();
    partial void DrawUnsupportedShaders();
    partial void PrepareForSceneWindow();
    /// <summary>
    /// per camera
    /// </summary>
    partial void PrepareBuffer();
#if UNITY_EDITOR
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
    /// <summary>
    /// 语法错误Shader
    /// </summary>
    static Material errorMaterial;
    string SampleName { get; set; }
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        //camera.name 会开辟内存
        buffer.name = SampleName=camera.name;
        Profiler.EndSample();
    }
    /// <summary>
    /// 点击摄像机的显示出来的虚线
    /// </summary>
    partial void DrawGizmosBeforeFX()
    {
        if(Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera,GizmoSubset.PreImageEffects);
        }
    }
    partial void DrawGizmosAfterFX()
    {
        if (Handles.ShouldRenderGizmos())
        {
            //ImageEffects之后
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        } 
    }
    /// <summary>
    /// Scene UI
    /// </summary>
    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);//传递到场景View里面
    }
    /// <summary>
    /// 不支持的Shader
    /// </summary>
    partial void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        //使用Hidden/InternalErrorShader 代替
        var drawingSettings = new DrawingSettings(LegacyShaderTagIds[0], new SortingSettings(camera)) { overrideMaterial=errorMaterial};
        //Multi Pass
        for(int i=1;i<LegacyShaderTagIds.Length;i++)
        {
            drawingSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);
        }
        var filterSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filterSettings);
    }
#else
    const string SampleName=bufferName;
#endif
}
