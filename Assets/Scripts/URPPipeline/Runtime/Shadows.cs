using UnityEngine;
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
    /// 产生阴影的灯光集合
    /// </summary>
    ShadowDirectionalLight[] ShadowDirectionalLights = new ShadowDirectionalLight[maxShadowedDirectionalLightCount];

    const string bufferName = "Shadows";

    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    ScriptableRenderContext context;

    CullingResults cullingResults;

    ShadowSettings settings;
    /// <summary>
    /// 支持方向光阴影的个数
    /// </summary>
    const int maxShadowedDirectionalLightCount = 4,maxCascades=4;
    int shadowedDirectionalLightCount;
    //剔除球
    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCountId=Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSpheresId=Shader.PropertyToID("_CascadeCullingSpheres"),
        cascadeDataId=Shader.PropertyToID("_CascadeData"),
        shadowAtlastSizeId=Shader.PropertyToID("_ShadowAtlasSize"),
        shadowDistanceFadeId=Shader.PropertyToID("_ShadowDistanceFade")
        ;
    /// <summary>
    /// 方向光阴影矩阵 从哪一个Tile里面进行采样 例如:每一个灯光都有4级的Cascade
    /// </summary>
    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount* maxCascades];
    /// <summary>
    /// 剔除球
    /// </summary>
    static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades],
        cascadeData=new Vector4[maxCascades];
    /// <summary>
    /// Shader变体 阴影
    /// </summary>
    static string[] directionalFilterKeywords = {"_DIRECTIONAL_PCF3", "_DIRECTIONAL_PCF5", "_DIRECTIONAL_PCF7" };
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
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        shadowedDirectionalLightCount = 0;
        useShadowMask = false;
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            //画方向光的阴影
            RenderDirectionalShadows();
        }
        else
        {
            //声明默认的ShadowMap格式防止 GPU采样错误的ShadowMap格式 1X1大小
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
        //ShadowMask
        buffer.BeginSample(bufferName);
        SetKeywords(shadowMaskKeywords,useShadowMask? QualitySettings.shadowmaskMode==ShadowmaskMode.Shadowmask ? 0:1:-1);
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        //开辟RT ShadowMap格式
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //设置渲染目标
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);//Color.clear(0,0,0,0)

        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        int tiles = shadowedDirectionalLightCount * settings.directional.cascadeCount;
        //支持四盏光 分到4个Tile里面
        int split = tiles <= 1?1 :tiles<=4?2:4;
        //1024 tileSize =512
        int tileSize = atlasSize / split;
        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            //渲染一盏光为图集大小
            RenderDirectionalShadows(i, split, tileSize);
        }
        //两个Cascade 的衰减
        float f = 1f - settings.directional.cascadeFade;

        //m是最大阴影距离 f是过渡范围
        buffer.SetGlobalVector(shadowDistanceFadeId,new Vector4(1f/settings.maxDistance,1/settings.distanceFade,1f/(1f-f*f)));
        //设置变体
        SetKeywords(directionalFilterKeywords,(int)settings.directional.filter-1);
        //抖动过滤
        SetKeywords(cascadeBlendKeywords,(int)settings.directional.cascadeBlend-1);
        buffer.SetGlobalVector(shadowAtlastSizeId,new Vector4(atlasSize,1f/atlasSize));//一个纹素大小

        buffer.EndSample(bufferName);
        ExecuteBuffer();
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
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = settings.directional.CascadeRatios;
        //裁剪参数
        float cullingFactor = Mathf.Max(0f,0.8f - settings.directional.cascadeFade);
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
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(tileIndex, split, tileSize), split);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            //Cascade Count to GPU 
            buffer.SetGlobalInt(cascadeCountId,settings.directional.cascadeCount);
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
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m,Vector2 offset,int split)
    {
        //usesReversedZBuffer 1 是最近深度 0为最远  pro投影矩阵*view矩阵[x,y,z]
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
            //split= scale=0.5f
            float scale = 1f / split;
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
        if (shadowedDirectionalLightCount<maxShadowedDirectionalLightCount
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

            ShadowDirectionalLights[shadowedDirectionalLightCount] = new ShadowDirectionalLight { visibleLightIndex=visibleLightIndex,slopeScaleBias=light.shadowBias,nearPlaneOffset=light.shadowNearPlane};
            //* 级联ShadowMap的数量
            return new Vector4(light.shadowStrength,settings.directional.cascadeCount*shadowedDirectionalLightCount++,light.shadowNormalBias, maskChannel);
        }
        return new Vector4(0f,0f,0f,-1f);
    }
}
