using System;

namespace UnityEngine.Rendering.Universal
{
	[Serializable,VolumeComponentMenu("Custom/ColorTint")]
	public class ColorTint : VolumeComponent, IPostProcessComponent
	{
		[Tooltip("ColorTint")]
		public ColorParameter color = new ColorParameter(Color.white,false,false,true);

		[Range(0f, 1f), Tooltip("ColorTint intensity")]
		public ClampedFloatParameter blend = new ClampedFloatParameter(0.5f,0f,1f);

		public bool IsActive() => blend.value > 0f;
		

		public bool IsTileCompatible() => false;
		
	}
}
