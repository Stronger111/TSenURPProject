using UnityEngine;
using UnityEngine.Rendering;
using static PostFXSettings;

public partial class PostFXStack 
{
    const string bufferName = "Post FX";

    CommandBuffer buffer = new CommandBuffer { name=bufferName};

    ScriptableRenderContext context;

    Camera camera;

    PostFXSettings settings;
    /// <summary>
    /// 是否是激活
    /// </summary>
    public bool IsActive => settings != null;

    enum Pass
    {
        BloomPrefilterFireflies,
        BloomPrefilter,
        BloomHorizontal,
        BloomVertical,
        BloomAdd, //叠加模式
        BloomScatter,//散射模式
        BloomScatterFinal,
        ColorGradingNone,
        ColorGradingACES,
        ColorGradingNeutral,
        ColorGradingReinhard,
        Copy,
        Final
    }
    /// <summary>
    /// 源RT
    /// </summary>
    int fxSourceId = Shader.PropertyToID("_PostFXSource"),
        fxSource2Id=Shader.PropertyToID("_PostFXSource2");
    int bloomBucibicUpsamplingId = Shader.PropertyToID("_BloomBicubicUpsampling");
    /// <summary>
    /// 预过滤 减少消耗
    /// </summary>
    int bloomPrefilterId = Shader.PropertyToID("_BloomPrefilter");
    int bloomThresholdId = Shader.PropertyToID("_BloomThreshold");//计算weight
    /// <summary>
    /// Bloom 强度
    /// </summary>
    int bloomIntensityId = Shader.PropertyToID("_BloomIntensity");
    int bloomResultId = Shader.PropertyToID("_BloomResult");
    #region Bloom
    const int maxBloomPyramidLevels = 16;
    /// <summary>
    /// ID标识符
    /// </summary>
    int bloomPyramidId;
    #endregion
    #region ColorGrading
    int colorAdjustmentsId = Shader.PropertyToID("_ColorAdjustments"),
        colorFilterId = Shader.PropertyToID("_ColorFilter"),
        whiteBalanceId=Shader.PropertyToID("_WhiteBalance"),
        splitToningShadowsId=Shader.PropertyToID("_SplitToningShadows"),
        splitToningHighlightsId=Shader.PropertyToID("_SplitToningHighlights"),
        channelMixerRedId=Shader.PropertyToID("_ChannelMixerRed"),
        channelMixerGreenId=Shader.PropertyToID("_ChannelMixerGreen"),
        channelMixerBlueId=Shader.PropertyToID("_ChannelMixerBlue"),
        smhShadowsId=Shader.PropertyToID("_SMHShadows"),
        smhMidtonesId=Shader.PropertyToID("_SMHMidtones"),
        smhHighlightsId=Shader.PropertyToID("_SMHHighlights"),
        smhRangeId=Shader.PropertyToID("_SMHRange"),
        colorGradingLUTId=Shader.PropertyToID("_ColorGradingLUT"),
        colorGradingLUTParametersId=Shader.PropertyToID("_ColorGradingLUTParameters"),
        colorGradingLUTInLogId=Shader.PropertyToID("_ColorGradingLUTInLogC");
    #endregion
    #region 配置
    bool useHDR;
    int colorLUTResolution;
    #endregion
    CameraSettings.FinalBlendMode finalBlendMode;
    int finalSrcBlendId = Shader.PropertyToID("_FinalSrcBlend"),
        finalDstBlendId=Shader.PropertyToID("_FinalDstBlend");
    public PostFXStack()
    {
        bloomPyramidId = Shader.PropertyToID("_BloomPyramid0");
        for(int i=1;i<maxBloomPyramidLevels*2;i++)
        {
            Shader.PropertyToID("_BloomPyramid"+i);
        }
    }
    #region 测试贴花UV
    int sp_SrcPieceSize = Shader.PropertyToID("_SrcPieceSize");
    int sp_RowNumAndIdx = Shader.PropertyToID("_RowNumAndIdx");
    int sp_DestPosAndSize = Shader.PropertyToID("_DestPosAndSize");
    int sp_RotAndScale = Shader.PropertyToID("_RotAndScale");
    int sp_NeedMirror = Shader.PropertyToID("_NeedMirror");
    int baseSkinRenderTextureId = Shader.PropertyToID("_BaseSkinTexture");
    DecalUVTest decalTest;
    #endregion
    public void Setup(ScriptableRenderContext context,Camera camera,PostFXSettings settings,
        bool useHDR,int colorLUTResolution,CameraSettings.FinalBlendMode finalBlendMode)
    {
        this.finalBlendMode = finalBlendMode;
        this.colorLUTResolution = colorLUTResolution;
        this.useHDR = useHDR;
        this.context = context;
        this.camera = camera;
        this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
        ApplySceneViewState();
    }

    public void Render(int sourceId)
    {
        //DrawSkinBlit(sourceId);
        //return;
        if (DoBloom(sourceId))
        {
            DoColorGradingToneMapping(bloomResultId);
            buffer.ReleaseTemporaryRT(bloomResultId);
        }
        else
        {
            DoColorGradingToneMapping(sourceId);
        }
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Draw(RenderTargetIdentifier from,RenderTargetIdentifier to,Pass pass)
    {
        buffer.SetGlobalTexture(fxSourceId,from);
        buffer.SetRenderTarget(to,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.DrawProcedural(Matrix4x4.identity,settings.Material,(int)pass, MeshTopology.Triangles,3);
    }

    void DrawFinal(RenderTargetIdentifier from)
    {
        buffer.SetGlobalFloat(finalSrcBlendId,(float)finalBlendMode.source);
        buffer.SetGlobalFloat(finalDstBlendId,(float)finalBlendMode.destination);
        buffer.SetGlobalTexture(fxSourceId, from);
        buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,finalBlendMode.destination==BlendMode.Zero ?
          RenderBufferLoadAction.DontCare:RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        buffer.SetViewport(camera.pixelRect);
        buffer.DrawProcedural(Matrix4x4.identity, settings.Material, (int)Pass.Final, MeshTopology.Triangles, 3);
    }
    /// <summary>
    /// 测试Blit
    /// </summary>
    /// <param name="from"></param>
    void DrawSkinBlit(RenderTargetIdentifier from)
    {
        buffer.Blit(settings.BaseSkinTexture, from);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        decalTest = GameObject.Find("Root").GetComponent<DecalUVTest>();
        settings.SkinBlitMaterial.SetVector(sp_RowNumAndIdx, decalTest.spRowNumAndIdx);
        settings.SkinBlitMaterial.SetVector(sp_DestPosAndSize, decalTest.spDestPosAndSize);
        settings.SkinBlitMaterial.SetVector(sp_SrcPieceSize, decalTest.spSrcPieceSize);
        settings.SkinBlitMaterial.SetVector(sp_RotAndScale, decalTest.spRotAndScale);
        //buffer.GetTemporaryRT(baseSkinRenderTextureId, settings.BaseSkinTexture.width,settings.BaseSkinTexture.height,0,FilterMode.Bilinear,RenderTextureFormat.Default);
        buffer.Blit(settings.SkinTexture, from, settings.SkinBlitMaterial);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        //buffer.SetGlobalTexture(fxSourceId, settings.SkinTexture);
        //buffer.SetViewport(camera.pixelRect);
        //buffer.DrawProcedural(Matrix4x4.identity,settings.SkinBlitMaterial,0,MeshTopology.Triangles, 3);
        //context.ExecuteCommandBuffer(buffer);
        //buffer.Clear();
        //DrawFinal(from);
        buffer.Blit(from, BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        //buffer.ReleaseTemporaryRT(baseSkinRenderTextureId);
    }
    bool DoBloom(int sourceId)
    {
        //buffer.BeginSample("Bloom");
        PostFXSettings.BloomSettings bloom = settings.Bloom;
        int width = camera.pixelWidth/2, height = camera.pixelHeight/2;

        if(bloom.maxIterations==0||bloom.intensity <=0f|| height<bloom.downscaleLimit || width<bloom.downscaleLimit)
        {
            //Draw(sourceId,BuiltinRenderTextureType.CameraTarget,Pass.Copy);
            //buffer.EndSample("Bloom");
            return false;
        }
        buffer.BeginSample("Bloom");
        //阀值超过阀值的像素部分才会Bloom
        Vector4 threshold;
        threshold.x = Mathf.GammaToLinearSpace(bloom.threshold);
        threshold.y = threshold.x * bloom.thresholdKnee;
        threshold.z = 2f * threshold.y;
        threshold.w = 0.25f / (threshold.y+0.00001f);
        threshold.y -= threshold.x;
        buffer.SetGlobalVector(bloomThresholdId, threshold);
        //看是否支持HDR格式纹理
        RenderTextureFormat format = useHDR?RenderTextureFormat.DefaultHDR:RenderTextureFormat.Default;
        //降采样
        buffer.GetTemporaryRT(bloomPrefilterId,width,height,0,FilterMode.Bilinear,format);
        Draw(sourceId,bloomPrefilterId ,bloom.fadeFireflies ? Pass.BloomPrefilterFireflies: Pass.BloomPrefilter);
        width /= 2;
        height /= 2;

        int fromId = bloomPrefilterId, toId = bloomPyramidId+1;

        int i;
        for(i=0;i< bloom.maxIterations; i++)
        {
            if (height < bloom.downscaleLimit*2 || width < bloom.downscaleLimit*2) //划分到最小像素 降分辨率
                break;
            int midId = toId - 1;
            buffer.GetTemporaryRT(midId,width,height,0,FilterMode.Bilinear,format);
            buffer.GetTemporaryRT(toId,width,height,0,FilterMode.Bilinear, format);
            Draw(fromId, midId, Pass.BloomHorizontal);
            Draw(midId,toId,Pass.BloomVertical);
            fromId = toId;
            toId += 2;
            width /= 2;
            height /= 2;
        }
        buffer.ReleaseTemporaryRT(bloomPrefilterId);

        buffer.SetGlobalFloat(bloomBucibicUpsamplingId,bloom.bicubicUpsampling?1f:0f);
        //Draw(fromId,BuiltinRenderTextureType.CameraTarget,Pass.Copy);
        Pass combinePass,finalPass;
        float finalIntensity;
        if(bloom.mode==PostFXSettings.BloomSettings.Mode.Additive)
        {
            combinePass = finalPass= Pass.BloomAdd;
            buffer.SetGlobalFloat(bloomIntensityId, 1f);
            finalIntensity = bloom.intensity;
        }
        else
        {
            combinePass = Pass.BloomScatter;
            finalPass = Pass.BloomScatterFinal;
            buffer.SetGlobalFloat(bloomIntensityId, bloom.scatter);
            finalIntensity = Mathf.Min(bloom.intensity,0.95f);
        }
        //升采样
        if(i>1) //迭代次数大于1次
        {
            buffer.ReleaseTemporaryRT(fromId - 1);
            toId -= 5;

            for (i -= 1; i > 0; i--)
            {
                buffer.SetGlobalTexture(fxSource2Id, toId + 1); //上去5次？
                Draw(fromId, toId, combinePass);

                buffer.ReleaseTemporaryRT(fromId);
                buffer.ReleaseTemporaryRT(toId + 1);
                fromId = toId;
                toId -= 2;
            }
        }else
        {
            buffer.ReleaseTemporaryRT(bloomPyramidId);
        }
        buffer.SetGlobalFloat(bloomIntensityId, finalIntensity);
        buffer.SetGlobalTexture(fxSource2Id,sourceId);
        //如果有Bloom 渲染到Result RT 上面
        buffer.GetTemporaryRT(bloomResultId,camera.pixelWidth,camera.pixelHeight,0,FilterMode.Bilinear,format);

        Draw(fromId, bloomResultId, finalPass);
        buffer.ReleaseTemporaryRT(fromId);
        buffer.EndSample("Bloom");
        return true;
    }

    void DoColorGradingToneMapping(int sourceId)
    {
        ConfigureColorAdjustments();
        ConfigWhiteBalance();
        ConfigureSplitToning();
        ConfigureChannelMixer();
        ConfigureShadowsMidtonesHighlights();
        //生成LUT 查找表
        int lutHeight = colorLUTResolution;
        int lutWidth = lutHeight * lutHeight;
        buffer.GetTemporaryRT(colorGradingLUTId,lutWidth,lutHeight,0,FilterMode.Bilinear,RenderTextureFormat.DefaultHDR);
        buffer.SetGlobalVector(colorGradingLUTParametersId,new Vector4(lutHeight,0.5f/lutWidth,0.5f/lutHeight,lutHeight/(lutHeight-1f)));
        //渲染ColorGrading和ToneMapping 到渲染纹理
        ToneMappingSettings.Mode mode = settings.ToneMapping.mode;
        Pass pass = Pass.ColorGradingNone + (int)mode;
        buffer.SetGlobalFloat(colorGradingLUTInLogId,useHDR && pass!=Pass.ColorGradingNone ? 1f:0f);
        Draw(sourceId,colorGradingLUTId, pass);

        //FinalPass
        buffer.SetGlobalVector(colorGradingLUTParametersId,new Vector4(1f/lutWidth,1f/lutHeight,lutHeight-1f));
        //Draw(sourceId,BuiltinRenderTextureType.CameraTarget,Pass.Final);
        DrawFinal(sourceId);
        buffer.ReleaseTemporaryRT(colorGradingLUTId);
    }

    void ConfigureColorAdjustments()
    {
        ColorAdjustmentsSetting colorAdjustments = settings.ColorAdjustments;
        buffer.SetGlobalVector(colorAdjustmentsId,new Vector4(Mathf.Pow(2f,colorAdjustments.postExposure),
            colorAdjustments.contrast*0.01f+1f,
            colorAdjustments.hueShift*(1f/360f),
            colorAdjustments.saturation*0.01f+1f)
            );

        buffer.SetGlobalColor(colorFilterId,colorAdjustments.colorFilter.linear);
    }

    void ConfigWhiteBalance()
    {
        WhiteBalanceSettings whiteBalance = settings.WhiteBalance;
        buffer.SetGlobalVector(whiteBalanceId,ColorUtils.ColorBalanceToLMSCoeffs(whiteBalance.temperature,whiteBalance.tint));
    }

    void ConfigureSplitToning()
    {
        SplitToningSettings splitToning = settings.SplitToning;
        Color splitColor = splitToning.shadows;
        splitColor.a = splitToning.balance * 0.01f;
        buffer.SetGlobalColor(splitToningShadowsId,splitColor);
        buffer.SetGlobalColor(splitToningHighlightsId, splitToning.highlights);
    }

    void ConfigureChannelMixer()
    {
        ChannelMixerSettings channelMixer = settings.ChannelMixer;
        buffer.SetGlobalVector(channelMixerRedId,channelMixer.red);
        buffer.SetGlobalVector(channelMixerGreenId,channelMixer.green);
        buffer.SetGlobalVector(channelMixerBlueId,channelMixer.blue);
    }

    void ConfigureShadowsMidtonesHighlights()
    {
        ShadowMidtonesHighlightsSettngs smh =settings.ShadowsMidtonesHighlights;
        buffer.SetGlobalColor(smhShadowsId,smh.shadows.linear);
        buffer.SetGlobalColor(smhMidtonesId,smh.midtones.linear);
        buffer.SetGlobalColor(smhHighlightsId,smh.highlights.linear);
        buffer.SetGlobalVector(smhRangeId,new Vector4(smh.shadowsStart,smh.shadowsEnd,smh.highlightsStart,smh.highLightsEnd));
    }
}
