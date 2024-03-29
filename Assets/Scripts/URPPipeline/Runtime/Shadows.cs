﻿using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    struct ShadowDirectionalLight
    {
        /// <summary>
        /// 哪一盏光投射阴影
        /// </summary>
        public int visibleLightIndex;
        /// <summary>
        /// Bias 配置
        /// </summary>
        public float slopeScaleBias;

        public float nearPlaneOffset;
    }
    /// <summary>
    /// 其他灯光阴影
    /// </summary>
    struct ShadowOtherLight
    {
        public int visibleLightIndex;
        public float slopeScaleBias;
        public float normalBias;
        //是否是点光源
        public bool isPoint;
    }
    /// <summary>
    /// 产生阴影的灯光集合
    /// </summary>
    ShadowDirectionalLight[] ShadowDirectionalLights = new ShadowDirectionalLight[maxShadowedDirLightCount];
    /// <summary>
    /// 最大其他灯光阴影
    /// </summary>
    ShadowOtherLight[] shadowedOtherLights = new ShadowOtherLight[maxShadowedOtherLightCount];
    const string bufferName = "Shadows";

    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    ScriptableRenderContext context;

    CullingResults cullingResults;

    ShadowSettings settings;
    /// <summary>
    /// 支持方向光阴影的个数
    /// </summary>
    const int maxShadowedDirLightCount = 4, maxShadowedOtherLightCount = 16;
    const int maxCascades=4;
    /// <summary>
    /// 方向光阴影灯光数量,其他光源阴影数量
    /// </summary>
    int shadowedDirLightCount,shadowedOtherLightCount;
    //剔除球
    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        //其他灯光Atlas
        otherShadowAtlasId=Shader.PropertyToID("_OtherShadowAtlas"),
        otherShadowMatricesId=Shader.PropertyToID("_OtherShadowMatrices"),
        otherShadowTilesId=Shader.PropertyToID("_OtherShadowTiles"),

        cascadeCountId=Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSpheresId=Shader.PropertyToID("_CascadeCullingSpheres"),
        cascadeDataId=Shader.PropertyToID("_CascadeData"),
        shadowAtlastSizeId=Shader.PropertyToID("_ShadowAtlasSize"),
        shadowDistanceFadeId=Shader.PropertyToID("_ShadowDistanceFade"),
        shadowPancakingId=Shader.PropertyToID("_ShadowPancaking")
        ;
    /// <summary>
    /// 方向光阴影矩阵 从哪一个Tile里面进行采样 例如:每一个灯光都有4级的Cascade
    /// </summary>
    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirLightCount* maxCascades],
                       otherShadowMatrices=new Matrix4x4[maxShadowedOtherLightCount];
    /// <summary>
    /// 剔除球
    /// </summary>
    static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades],
        cascadeData=new Vector4[maxCascades],
        otherShadowTiles=new Vector4[maxShadowedOtherLightCount]
        ;
    /// <summary>
    /// Shader变体 阴影
    /// </summary>
    static string[] directionalFilterKeywords = {"_DIRECTIONAL_PCF3", "_DIRECTIONAL_PCF5", "_DIRECTIONAL_PCF7" };
    /// <summary>
    /// 其他灯光过滤方式
    /// </summary>
    static string[] otherFilterKeywords = {"_OTHER_PCF3", "_OTHER_PCF5", "_OTHER_PCF7" };
    /// <summary>
    /// 阴影模式
    /// </summary>
    static string[] cascadeBlendKeywords = { "_CASCADE_BLEND_SOFT","_CASCADE_BLEND_DITHER"};
    /// <summary>
    /// 阴影DistanceShadowMask 和 ShadowMask  
    /// </summary>
    static string[] shadowMaskKeywords = { "_SHADOW_MASK_ALWAYS " , "_SHADOW_MASK_DISTANCE" };
    /// <summary>
    /// 是否使用ShadowMask
    /// </summary>
    bool useShadowMask;
    /// <summary>
    /// 图集大小  x 图集大小 y 每个纹素大小 z w
    /// </summary>
    Vector4 atlasSizes;
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        shadowedDirLightCount =shadowedOtherLightCount= 0;
        useShadowMask = false;
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void Render()
    {
        if (shadowedDirLightCount > 0)
        {
            //画方向光的阴影
            RenderDirectionalShadows();
        }
        else
        {
            //声明默认的ShadowMap格式防止 GPU采样错误的ShadowMap格式 1X1大小
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }

        if(shadowedOtherLightCount > 0)
        {
            RenderOtherShadows();
        }else
        {
            buffer.SetGlobalTexture(otherShadowAtlasId,dirShadowAtlasId);
        }

        //ShadowMask
        buffer.BeginSample(bufferName);
        SetKeywords(shadowMaskKeywords,useShadowMask? QualitySettings.shadowmaskMode==ShadowmaskMode.Shadowmask ? 0:1:-1);
        //Cascade Count to GPU 
        buffer.SetGlobalInt(cascadeCountId,shadowedDirLightCount >0 ? settings.directional.cascadeCount : 0);
        //两个Cascade 的衰减
        float f = 1f - settings.directional.cascadeFade;

        //m是最大阴影距离 f是过渡范围
        buffer.SetGlobalVector(shadowDistanceFadeId, new Vector4(
            1f / settings.maxDistance,
            1 / settings.distanceFade,
            1f / (1f - f * f)));
        //设置图集大小
        buffer.SetGlobalVector(shadowAtlastSizeId,atlasSizes);
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        atlasSizes.x = atlasSize;
        atlasSizes.y = 1f / atlasSize;
        //开辟RT ShadowMap格式
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //设置渲染目标
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        //设置Pancaking
        buffer.SetGlobalFloat(shadowPancakingId,1f);
        buffer.ClearRenderTarget(true, false, Color.clear);//Color.clear(0,0,0,0)

        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        int tiles = shadowedDirLightCount * settings.directional.cascadeCount;
        //支持四盏光 分到4个Tile里面
        int split = tiles <= 1?1 :tiles<=4?2:4;
        //1024 tileSize =512
        int tileSize = atlasSize / split;
        for (int i = 0; i < shadowedDirLightCount; i++)
        {
            //渲染一盏光为图集大小
            RenderDirectionalShadows(i, split, tileSize);
        }
        
        //设置变体
        SetKeywords(directionalFilterKeywords,(int)settings.directional.filter-1);
        //抖动过滤
        SetKeywords(cascadeBlendKeywords,(int)settings.directional.cascadeBlend-1);
        //buffer.SetGlobalVector(shadowAtlastSizeId,new Vector4(atlasSize,1f/atlasSize));//一个纹素大小

        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }
    /// <summary>
    /// 其他灯光阴影
    /// </summary>
    void RenderOtherShadows()
    {
        int atlasSize = (int)settings.other.atlasSize;
        atlasSizes.z = atlasSize;
        atlasSizes.w = 1f / atlasSize;
        //开辟RT ShadowMap格式
        buffer.GetTemporaryRT(otherShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //设置渲染目标
        buffer.SetRenderTarget(otherShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);//Color.clear(0,0,0,0)
        buffer.SetGlobalFloat(shadowPancakingId,0f);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        int tiles = shadowedOtherLightCount;
        //支持四盏光 分到4个Tile里面
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        //1024 tileSize =512
        int tileSize = atlasSize / split;
        for (int i = 0; i < shadowedOtherLightCount;)
        {
            if(shadowedOtherLights[i].isPoint)
            {
                RenderPointShadows(i,split,tileSize);
                //渲染点光源
                i += 6;
            }
            else
            {
                //渲染一盏光为图集大小
                RenderSpotShadows(i, split, tileSize);
                i += 1;
            }
          
        }
        //其他灯光矩阵
        buffer.SetGlobalMatrixArray(otherShadowMatricesId, otherShadowMatrices);
        buffer.SetGlobalVectorArray(otherShadowTilesId,otherShadowTiles);
        //设置变体
        SetKeywords(otherFilterKeywords, (int)settings.other.filter - 1);

        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void SetOtherTileData(int index,Vector2 offset,float scale, float bias)
    {
        //边界是一个纹素的一半
        float border = atlasSizes.w * 0.5f;
        Vector4 data = Vector4.zero;
        data.x = offset.x * scale + border;
        data.y = offset.y * scale + border;
        data.z = scale - border - border;
        data.w = bias;
        otherShadowTiles[index] = data;
    }
    /// <summary>
    /// 设置关键字
    /// </summary>
    void SetKeywords(string[] keywords,int enabledIndex)
    {
        //int enabledIndex = (int)settings.directional.filter - 1;
        for(int i=0;i< keywords.Length;i++)
        {
            if (i == enabledIndex)
            {
                buffer.EnableShaderKeyword(keywords[i]);
            }
            else
                buffer.DisableShaderKeyword(keywords[i]);
        }
    }
    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowDirectionalLight light = ShadowDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex) { useRenderingLayerMaskTest=true};
        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = settings.directional.CascadeRatios;
        //裁剪参数
        float cullingFactor = Mathf.Max(0f,0.8f - settings.directional.cascadeFade);
        float tileScale = 1f / split;
        //遍历Cascade的数量
        for(int i=0;i<cascadeCount;i++)
        {
            //Debug.Log(light.nearPlaneOffset);
            //Unity
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, i, cascadeCount, ratios, tileSize, light.nearPlaneOffset, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData);
            //??????？？？？？
            splitData.shadowCascadeBlendCullingFactor = cullingFactor;
            shadowSettings.splitData = splitData;
            if (index == 0)
            {
                //裁剪球 判断片段在哪一级的Cascade里面 w为半径
                Vector4 cullingSphere = splitData.cullingSphere;
                SetCascadeData(i,splitData.cullingSphere,tileSize);
            }
               
            int tileIndex = tileOffset + i;
            //设置ViewPort大小
            //SetTileViewport(index, split, tileSize);
            //从世界空间到灯光阴影投影空间
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(tileIndex, split, tileSize), tileScale);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
         
            //球覆盖
            buffer.SetGlobalVectorArray(cascadeCullingSpheresId,cascadeCullingSpheres);
            //Cascade数据
            buffer.SetGlobalVectorArray(cascadeDataId,cascadeData);
            //转换物体到灯光空间
            buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
            buffer.SetGlobalDepthBias(0f,light.slopeScaleBias);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
            buffer.SetGlobalDepthBias(0f,0f);
        }
    }
    /// <summary>
    /// 渲染聚光灯阴影
    /// </summary>
    /// <param name="index"></param>
    /// <param name="split"></param>
    /// <param name="tileSize"></param>
    void RenderSpotShadows(int index,int split , int tileSize)
    {
        ShadowOtherLight light = shadowedOtherLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults,light.visibleLightIndex);
        cullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(light.visibleLightIndex,out Matrix4x4 viewMatrix,
                      out Matrix4x4 projectionMatrix,out ShadowSplitData splitData);
        shadowSettings.splitData = splitData;
        //Normal Bias
        float texelSize = 2f / (tileSize * projectionMatrix.m00);
        float filterSize = texelSize * ((float)settings.other.filter+1f);
        float bias=light.normalBias*filterSize* 1.4142136f;
        Vector2 offset = SetTileViewport(index,split,tileSize);
        float tileScale = 1 / split;
        SetOtherTileData(index, offset, tileScale, bias);
        otherShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix*viewMatrix, offset, tileScale);
        buffer.SetViewProjectionMatrices(viewMatrix,projectionMatrix);
        buffer.SetGlobalDepthBias(0f,light.slopeScaleBias);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
        buffer.SetGlobalDepthBias(0f,0f);
    }
    /// <summary>
    /// 渲染点光源阴影
    /// </summary>
    /// <param name="index"></param>
    /// <param name="split"></param>
    /// <param name="tileSize"></param>
    void RenderPointShadows(int index, int split, int tileSize)
    {
        ShadowOtherLight light = shadowedOtherLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        //Normal Bias
        float texelSize = 2f / tileSize;
        float filterSize = texelSize * ((float)settings.other.filter + 1f);
        float bias = light.normalBias * filterSize * 1.4142136f;
        float tileScale = 1 / split;
        //解决阴影不连续问题
        float fovBias = Mathf.Atan(1f+bias+filterSize)*Mathf.Rad2Deg*2f-90f;
        for (int i=0;i<6;i++)
        {
            cullingResults.ComputePointShadowMatricesAndCullingPrimitives(light.visibleLightIndex,(CubemapFace)i, fovBias, out Matrix4x4 viewMatrix,
                          out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
            //解决阴影跑到其他物体上
            viewMatrix.m11 = -viewMatrix.m11;
            viewMatrix.m12 = -viewMatrix.m12;
            viewMatrix.m13 = -viewMatrix.m13;
            shadowSettings.splitData = splitData;
            int tileIndex = index + i;
            Vector2 offset = SetTileViewport(tileIndex, split, tileSize);
            SetOtherTileData(tileIndex, offset, tileScale, bias);
            otherShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, tileScale);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
            buffer.SetGlobalDepthBias(0f, 0f);
        }
    }
    void SetCascadeData(int index,Vector4 cullingSphere,float tileSize)
    {
        //球体的直径/TileSize 一个纹素占据的距离大小
        float texelSize = 2f * cullingSphere.w/tileSize;
        //解决PCF出现阴影锯齿
        float filterSize = texelSize * ((float)settings.directional.filter+1f);
        //1/半径 是一个Step
        //cascadeData[index].x = 1f / cullingSphere.w; 沿着对角线进行偏移
        cascadeData[index] = new Vector4(1f / cullingSphere.w, filterSize* 1.4142136f);
        cullingSphere.w -= filterSize;
        //半径进行平方
        cullingSphere.w *= cullingSphere.w;
        cascadeCullingSpheres[index] = cullingSphere;
    }
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m,Vector2 offset,float scale)
    {
        //usesReversedZBuffer 1 是最近深度 0为最远  pro投影矩阵*view矩阵[x,y,z]
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
            //split= scale=0.5f
            //float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
        }
        return m;
    }

    Vector2 SetTileViewport(int index,int split,float tileSize)
    {
        Vector2 offset = new Vector2(index%split,index/split);
        //左下角为0,0
        buffer.SetViewport(new Rect(offset.x* tileSize,offset.y*tileSize,tileSize,tileSize));
        return offset;
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        if(shadowedOtherLightCount > 0)
        {
            buffer.ReleaseTemporaryRT(otherShadowAtlasId);
        }
        ExecuteBuffer();
    }

    /// <summary>
    /// 储备方向光阴影 Vector2存取强度和光线的索引 支持四盏光的ShadowMask
    /// </summary>
    /// <param name="light"></param>
    /// <param name="visibleLightIndex"></param>
    public Vector4 ReserveDirectionalShadows(Light light,int visibleLightIndex)
    {
        //检测阴影强度 None &&cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)
        if (shadowedDirLightCount<maxShadowedDirLightCount
            &&light.shadows!=LightShadows.None &&light.shadowStrength>0f)
        {
            float maskChannel = -1;
            //shadowMask
            LightBakingOutput lightBaking = light.bakingOutput;
            if(lightBaking.lightmapBakeType==LightmapBakeType.Mixed && lightBaking.mixedLightingMode==MixedLightingMode.Shadowmask)
            {
                useShadowMask = true;
                maskChannel = lightBaking.occlusionMaskChannel;
            }
            //直接Return 出去 不需要Cacade数据 阴影跑出CullSphere里面
            if(!cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                //负数防止Shader 采样ShadowMap
                return new Vector4(-light.shadowStrength,0f,0f, maskChannel);
            }

            ShadowDirectionalLights[shadowedDirLightCount] = new ShadowDirectionalLight { visibleLightIndex=visibleLightIndex,slopeScaleBias=light.shadowBias,nearPlaneOffset=light.shadowNearPlane};
            //* 级联ShadowMap的数量
            return new Vector4(light.shadowStrength,settings.directional.cascadeCount*shadowedDirLightCount++,light.shadowNormalBias, maskChannel);
        }
        return new Vector4(0f,0f,0f,-1f);
    }
    /// <summary>
    /// 用于其他灯光阴影
    /// </summary>
    /// <param name="light"></param>
    /// <param name="visibleLightIndex"></param>
    /// <returns></returns>
    public Vector4 ReserveOtherShadows(Light light,int visibleLightIndex)
    {
        //判断无阴影 或者强度是0 立即返回
        if(light.shadows==LightShadows.None || light.shadowStrength <=0f)
        {
            return new Vector4(0f,0f,0f,-1f);
        }
        float maskChannel = -1f;
       
        LightBakingOutput lightBaking = light.bakingOutput;
        if(lightBaking.lightmapBakeType == LightmapBakeType.Mixed && lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask)
        {
            useShadowMask = true;
            maskChannel = lightBaking.occlusionMaskChannel;
        }
        //是否是点光源 疑问:为什么点光源 需要6个Tile
        bool isPoint = light.type == LightType.Point;
        int newLightCount = shadowedOtherLightCount + (isPoint ? 6 : 1);
        if(newLightCount >= maxShadowedOtherLightCount || !cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds b))
        {
            return new Vector4(-light.shadowStrength,0f,0f,maskChannel);
        }
        shadowedOtherLights[shadowedOtherLightCount] = new ShadowOtherLight
        { 
            visibleLightIndex=visibleLightIndex,
            slopeScaleBias=light.shadowBias,
            normalBias=light.shadowNormalBias,
            isPoint=isPoint
        };
        Vector4 data = new Vector4(light.shadowStrength, shadowedOtherLightCount++, isPoint ? 1f: 0f, lightBaking.occlusionMaskChannel);
        shadowedOtherLightCount = newLightCount;
        //返回Mix灯光 ShadowMask
        return data;
    }
}
