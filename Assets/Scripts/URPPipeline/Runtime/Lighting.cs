using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting : MonoBehaviour
{
    const string bufferName = "Lighting";

    CommandBuffer buffer = new CommandBuffer { name=bufferName};
    /// <summary>
    /// 灯光的颜色
    /// </summary>
    //static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
    /// <summary>
    /// 灯光的方向
    /// </summary>
    //static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    /// <summary>
    /// 支持最大方向光灯光数 支持最大其他灯光数
    /// </summary>
    const int maxDirLightCount = 4 , maxOtherLightCount=64;
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
        dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    //存取灯光颜色和方向
    static Vector4[] dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount],
        dirLightShadowData=new Vector4[maxDirLightCount];
    //其他灯光数据发送给GPU
    static int otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
               otherLightColorsId=Shader.PropertyToID("_OtherLightColors"),
               otherLightPositionsId=Shader.PropertyToID("_OtherLightPositions"),
               otherLightDirectionsId=Shader.PropertyToID("_OtherLightDirections"),
               otherLightSpotAnglesId=Shader.PropertyToID("_OtherLightSpotAngles");
    //其他灯光
    static Vector4[] otherLightColors = new Vector4[maxOtherLightCount],
                     otherLightPositions=new Vector4[maxOtherLightCount],
                     otherLightDirections=new Vector4[maxOtherLightCount],
                     otherLightSpotAngles=new Vector4[maxOtherLightCount];

    CullingResults cullingResults;
    /// <summary>
    /// 阴影渲染
    /// </summary>
    Shadows shadows = new Shadows();
    public void Setup(ScriptableRenderContext context,CullingResults cullingResults,ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        //阴影提前设置
        shadows.Setup(context,cullingResults,shadowSettings);
        //设置灯光
        SetupLights();
        //渲染阴影
        shadows.Render();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        //方向光 和其他灯光
        int dirLightCount = 0,otherLightCount=0;
        for(int i=0;i<visibleLights.Length;i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            //if(visibleLight.lightType==LightType.Directional)
            //{
            //    SetupDirectionalLight(dirLightCount++,ref visibleLight);
            //    //达到最大数量 检查
            //    if (dirLightCount >= maxDirLightCount)
            //        break;
            //}
            switch(visibleLight.lightType)
            {
                case LightType.Directional:
                    if(dirLightCount<maxDirLightCount)
                    {
                        SetupDirectionalLight(dirLightCount++, ref visibleLight);
                    }
                    break;
                case LightType.Point:
                    if(otherLightCount<maxOtherLightCount)
                    {
                        SetupPointLight(otherLightCount++,ref visibleLight);
                    }
                    break;
                case LightType.Spot:
                    if(otherLightCount<maxOtherLightCount)
                    {
                        SetupSpotLight(otherLightCount++,ref visibleLight);
                    }
                    break;
            }
        }
        //设置GPU Buffer
        buffer.SetGlobalInt(dirLightCountId, dirLightCount);
        if (dirLightCount>0)
        {
            buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
            //阴影Uniform Buffer
            buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }
        //其他灯光 灯光数据
        buffer.SetGlobalInt(otherLightCountId,otherLightCount);
        if(otherLightCount > 0)
        {
            buffer.SetGlobalVectorArray(otherLightColorsId,otherLightColors);
            buffer.SetGlobalVectorArray(otherLightPositionsId,otherLightPositions);
            //灯光方向
            buffer.SetGlobalVectorArray(otherLightDirectionsId,otherLightDirections);
            //聚光灯角度
            buffer.SetGlobalVectorArray(otherLightSpotAnglesId,otherLightSpotAngles);
        }
       
    }
    void SetupDirectionalLight(int index,ref VisibleLight visibleLight)
    {
        //Light light = RenderSettings.sun;
        //buffer.SetGlobalVector(dirLightColorId,light.color.linear*light.intensity);
        //buffer.SetGlobalVector(dirLightDirectionId,-light.transform.forward);
        //finalColor=Color*Intensity
        dirLightColors[index] = visibleLight.finalColor;
        //知乎解释visibleLight.localToWorldMatrix.GetColumn(2)
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        //阴影保存
        dirLightShadowData[index]=shadows.ReserveDirectionalShadows(visibleLight.light,index);
    }
    /// <summary>
    /// 设置点光源数据
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="visibleLight">可见光数据</param>
    void SetupPointLight(int index,ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w = 1f / Mathf.Max(visibleLight.range*visibleLight.range,0.00001f); //w分量添加灯光范围
        otherLightPositions[index] = position;
    }
    /// <summary>
    /// 设置聚光灯数据
    /// </summary>
    /// <param name="index"></param>
    /// <param name="visibleLight"></param>
    void SetupSpotLight(int index, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f); //w分量添加灯光范围
        otherLightPositions[index] = position;
        otherLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        //聚光灯角度
        Light light = visibleLight.light;
        float innerCos = Mathf.Cos(Mathf.Deg2Rad*0.5f*light.innerSpotAngle);
        float outerCos = Mathf.Cos(Mathf.Deg2Rad*0.5f*visibleLight.spotAngle);
        float angleRangeInv = 1f / Mathf.Max(innerCos-outerCos,0.001f);
        otherLightSpotAngles[index] = new Vector4(angleRangeInv,-outerCos*angleRangeInv);
    }
    public void Cleanup()
    {
        //清除纹理资源
        shadows.Cleanup();
    }
}
