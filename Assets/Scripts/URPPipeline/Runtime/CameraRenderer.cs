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
    #region 后期配置
    PostFXStack postFXStack = new PostFXStack();
    //static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");
    static int colorAttachmentId = Shader.PropertyToID("_CameraColorAttachment"),
        depthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment"),
        colorTextureId=Shader.PropertyToID("_CameraColorTexture"),
        depthTextureId=Shader.PropertyToID("_CameraDepthTexture"),
        sourceTextureId=Shader.PropertyToID("_SourceTexture");
    bool useHDR;
    bool useColorTexture, useDepthTexture,useIntermediateBuffer;
    /// <summary>
    /// 当拷贝纹理不支持
    /// </summary>
    static bool copyTextureSupported = SystemInfo.copyTextureSupport>CopyTextureSupport.None;
    Material material;
    #endregion
    #region 多摄像机
    static CameraSettings defaultCameraSettings = new CameraSettings();
    #endregion
    Texture2D missingTexture;
    public CameraRenderer(Shader shader)
    {
        material = CoreUtils.CreateEngineMaterial(shader);
        missingTexture = new Texture2D(1, 1) { hideFlags=HideFlags.HideAndDontSave,name="Missing"};
        missingTexture.SetPixel(0,0,Color.white*0.5f);
        missingTexture.Apply(true,true);
    }
    public void Render(ScriptableRenderContext contex,Camera camera,CameraBufferSettings bufferSettings, bool useDynamicBatching,bool useGPUInstancing,bool useLightsPerObject,
        ShadowSettings shadowSettings,PostFXSettings postFXSettings,int colorLUTResolution)
    {
        this.context = contex;
        this.camera = camera;

        var crpCamera = camera.GetComponent<CustomRenderPipelineCamera>();
        CameraSettings cameraSettings = crpCamera!=null? crpCamera.Settings : defaultCameraSettings;
        //useDepthTexture = true;
        if(camera.cameraType==CameraType.Reflection)
        {
            useColorTexture = bufferSettings.copyColorReflection;
            useDepthTexture = bufferSettings.copyDepthRelections;
        }else
        {
            useColorTexture = bufferSettings.copyColor && cameraSettings.copyColor;
            //两个条件成立
            useDepthTexture = bufferSettings.copyDepth && cameraSettings.copyDepth;
        }
        //判断是否覆盖摄像机设置
        if (cameraSettings.overridePostFX)
        {
            postFXSettings = cameraSettings.postFXSettings;
        }
        //每个摄像机有自己的Buffer
        PrepareBuffer();
        //UI
        PrepareForSceneWindow();
        //阴影Cull
        if (!Cull(shadowSettings.maxDistance))
        {
            return;
        }
        useHDR = bufferSettings.allowHDR && camera.allowHDR;
        buffer.BeginSample(SampleName);
        ExcuteBuffer();
        //灯光配置 添加阴影参数
        lighting.Setup(contex,cullingResults,shadowSettings,useLightsPerObject,cameraSettings.maskLights ? cameraSettings.renderingLayerMask :-1);
        //后期配置
        postFXStack.Setup(context,camera,postFXSettings,useHDR,colorLUTResolution,cameraSettings.finalBlendMode);
        buffer.EndSample(SampleName);
        //常规渲染
        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing, useLightsPerObject, cameraSettings.renderingLayerMask);
        //Un support shader
        DrawUnsupportedShaders();
        //Gizmos 线
        DrawGizmosBeforeFX();
        if(postFXStack.IsActive)
        {
            postFXStack.Render(colorAttachmentId);
        }else if(useIntermediateBuffer)
        {
            if (camera.targetTexture)
            {
                //Test UV

                buffer.CopyTexture(colorAttachmentId,camera.targetTexture);
            }else
            {
                //画到摄像机
                Draw(colorAttachmentId, BuiltinRenderTextureType.CameraTarget);
            }
            ExcuteBuffer();
        }
        DrawGizmosAfterFX();
        //释放纹理资源
        Cleanup();
        //lighting.Cleanup();
        Submit();
    }
    /// <summary>
    /// 剔除的结果
    /// </summary>
    CullingResults cullingResults;
    bool Cull(float maxShadowDistance)
    {
        //剔除参数结构
        if(camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            //Culling 参数最大阴影距离 阴影距离为摄像机平面和最大距离的最小值
            p.shadowDistance =Mathf.Min(maxShadowDistance,camera.farClipPlane) ;
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 画可见的几何体
    /// </summary>
    void DrawVisibleGeometry(bool useDynamicBathching,bool useGPUInstancing,bool useLightsPerObject,int renderingLayerMask)
    {
        PerObjectData lightsPerObjectFlags = useLightsPerObject ? PerObjectData.LightData | PerObjectData.LightIndices : PerObjectData.None;
        //先画不透明物体 顺序是从前往后画 不透明 在半透明物体
        var sortingSettings = new SortingSettings(camera) { criteria=SortingCriteria.CommonOpaque};
        //开启动态合批 关闭GPU Instancing Draw unlitShader 和 litShaderTagId LightMap LightProbe LightProbeVolume Reflection Probe
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { enableDynamicBatching= useDynamicBathching, 
            enableInstancing= useGPUInstancing,perObjectData=PerObjectData.ReflectionProbes | PerObjectData.Lightmaps | PerObjectData.ShadowMask | PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
            PerObjectData.LightProbeProxyVolume | PerObjectData.OcclusionProbeProxyVolume | lightsPerObjectFlags
        }; //CPU 发送数据给GPU  动态物体ShadowMask
        drawingSettings.SetShaderPassName(1,litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, renderingLayerMask : (uint)renderingLayerMask);
        //Draw Opaque Visible Renderer
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);

        //还没提交到GPU Queque队列里面 CPU维护了一个Command List队列
        context.DrawSkybox(camera);
        if(useColorTexture||useDepthTexture)
        {
            //拷贝深度图
            CopyAttachments();
        }
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
        //都需要深度
        useIntermediateBuffer =useColorTexture | useDepthTexture | postFXStack.IsActive;
        //后期处理
        if (useIntermediateBuffer)
        {
            if(flags > CameraClearFlags.Color)
            {
                flags = CameraClearFlags.Color;
            }
            //是否使用HDR 得到HDR渲染格式的RT
            buffer.GetTemporaryRT(colorAttachmentId,camera.pixelWidth,camera.pixelHeight,0,FilterMode.Bilinear,
                useHDR? RenderTextureFormat.DefaultHDR:RenderTextureFormat.Default);
            buffer.GetTemporaryRT(depthAttachmentId,camera.pixelWidth,camera.pixelHeight,32,FilterMode.Point,RenderTextureFormat.Depth);
            buffer.SetRenderTarget(colorAttachmentId, RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store,
                depthAttachmentId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        }
        //第三个参数背景清除 Color.clear (0,0,0,0) 完全透明 Profiler
        buffer.ClearRenderTarget(flags<=CameraClearFlags.Depth, flags==CameraClearFlags.Color,flags==CameraClearFlags.Color?camera.backgroundColor.linear: Color.clear);

        buffer.BeginSample(SampleName);
        //
        buffer.SetGlobalTexture(colorTextureId,missingTexture);
        //容错
        buffer.SetGlobalTexture(depthTextureId,missingTexture);
        ExcuteBuffer();
    }
    void Submit()
    {
        buffer.EndSample(SampleName);
        //Buffer进行操作执行
        ExcuteBuffer();
        context.Submit();
    }
    void CopyAttachments()
    {
        if(useColorTexture)
        {
            buffer.GetTemporaryRT(colorTextureId,camera.pixelWidth,camera.pixelHeight,0,FilterMode.Bilinear,useHDR?
                RenderTextureFormat.DefaultHDR:RenderTextureFormat.Default);
            if(copyTextureSupported)
            {
                buffer.CopyTexture(colorAttachmentId,0,0,colorTextureId,0,0);
            }else
            {
                Draw(colorAttachmentId,colorTextureId);
            }
        }
        //
        if(useDepthTexture)
        {
            buffer.GetTemporaryRT(depthTextureId,camera.pixelWidth,camera.pixelHeight,32,FilterMode.Point,RenderTextureFormat.Depth);
            if(copyTextureSupported)
            {
                buffer.CopyTexture(depthAttachmentId, depthTextureId);
            }else
            {
                Draw(depthAttachmentId,depthTextureId,true);
                //Load 回来
                //buffer.SetRenderTarget(colorAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store,
                //    depthAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store);
            }
            //ExcuteBuffer();
        }
        if(!copyTextureSupported)
        {
            buffer.SetRenderTarget(colorAttachmentId, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                   depthAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store);
        }
        ExcuteBuffer();
    }
    void Cleanup()
    {
        lighting.Cleanup();
        if (useIntermediateBuffer)
        {
            buffer.ReleaseTemporaryRT(colorAttachmentId);
            buffer.ReleaseTemporaryRT(depthAttachmentId);
            if(useColorTexture)
            {
                buffer.ReleaseTemporaryRT(colorTextureId);
            }
            if (useDepthTexture)
            {
                buffer.ReleaseTemporaryRT(depthTextureId);
            }
        }
    }
    void Draw(RenderTargetIdentifier from,RenderTargetIdentifier to,bool isDepth=false)
    {
        buffer.SetGlobalTexture(sourceTextureId,from);
        buffer.SetRenderTarget(to,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.DrawProcedural(Matrix4x4.identity,material, isDepth?1:0, MeshTopology.Triangles,3);
    }
    public void Dispose()
    {
        CoreUtils.Destroy(material);
        CoreUtils.Destroy(missingTexture);
    }
}
