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
    /// 支持最大灯光数
    /// </summary>
    const int maxDirLightCount = 4;
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    //存取灯光颜色和方向
    static Vector4[] dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount];

    CullingResults cullingResults;
    public void Setup(ScriptableRenderContext context,CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        SetupLights();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for(int i=0;i<visibleLights.Length;i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if(visibleLight.lightType==LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++,ref visibleLight);
                //达到最大数量 检查
                if (dirLightCount >= maxDirLightCount)
                    break;
            }
        }
        //设置GPU Buffer
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId,dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId,dirLightDirections);
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
    }
}
