using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.Rendering.Universal
{
	/// <summary>
	/// 实现自定义RayMarchingCloud
	/// 地址连接 https://zhuanlan.zhihu.com/p/248406797 体积云
	/// </summary>
	[Serializable, VolumeComponentMenu("Custom/RayMarchingCloud")]
	public sealed class RayMarchingCloud : VolumeComponent, IPostProcessComponent
	{
		
		/// <summary>
		/// 判断参数是否开启
		/// </summary>
		/// <returns></returns>
		public bool IsActive()
		{
			throw new NotImplementedException();
		}

		public bool IsTileCompatible() => true;
		
	}
}
