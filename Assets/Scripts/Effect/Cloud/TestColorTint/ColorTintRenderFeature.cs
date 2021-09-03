using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
///  
/// </summary>
public class ColorTintRenderFeature : ScriptableRendererFeature
{
	ColorTintRenderPass colorTintRenderPass;

	RenderTargetHandle m_RenderTargetHandle;
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		var dest = RenderTargetHandle.CameraTarget;
		renderer.EnqueuePass(colorTintRenderPass);
	}

	public override void Create()
	{
		colorTintRenderPass = new ColorTintRenderPass();
		colorTintRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		m_RenderTargetHandle.Init("_ScreenTexture");

	}
}
