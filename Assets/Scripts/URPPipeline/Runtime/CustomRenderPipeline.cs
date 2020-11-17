using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    public CustomRenderPipeline()
    {
        //开启SRP Batch
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
    /// <summary>
    /// 引用单个摄像机Render
    /// </summary>
    CameraRenderer renderer = new CameraRenderer();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //遍历场景所有摄像机进行渲染,前向渲染 缺陷每个摄像机的渲染方式相同,可以让每个摄像机使用不同的渲染方式
        foreach(Camera camera in cameras)
        {
            renderer.Render(context,camera);
        }
    }
}
