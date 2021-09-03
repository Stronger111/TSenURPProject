using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ColorTint Pass
/// </summary>
public class ColorTintRenderPass : ScriptableRenderPass
{
	RenderTargetHandle source;
	RenderTargetHandle dest;
	/// <summary>
	/// Shader 和材质
	/// </summary>
	private Material colorTintMat;

	private ColorTint colorTint;
	const string m_ProfilerTag = "Render Color Tint";
	public ColorTintRenderPass()
	{
		var shader = Shader.Find("Hidden/PostProcessing/ColorTint");
		colorTintMat = CoreUtils.CreateEngineMaterial(shader);

	}

	public void Setup(RenderTargetHandle m_Source,RenderTargetHandle m_Dest)
	{
		source = m_Source;
		dest = m_Dest;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (colorTintMat==null)
		{
			UnityEngine.Debug.LogError("Material is null");
			return;
		}
		if (!renderingData.cameraData.postProcessEnabled) return;
		//判读后效模块是否开启
		var stack = VolumeManager.instance.stack;
		colorTint = stack.GetComponent<ColorTint>();
		if (colorTint == null) return;
		if (!colorTint.IsActive()) return;

		var cmd = CommandBufferPool.Get(m_ProfilerTag);
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		base.OnCameraCleanup(cmd);
	}
}
