using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.Rendering.Universal
{
	/// <summary>
	/// ʵ���Զ���RayMarchingCloud
	/// ��ַ���� https://zhuanlan.zhihu.com/p/248406797 �����
	/// </summary>
	[Serializable, VolumeComponentMenu("Custom/RayMarchingCloud")]
	public sealed class RayMarchingCloud : VolumeComponent, IPostProcessComponent
	{
		
		/// <summary>
		/// �жϲ����Ƿ���
		/// </summary>
		/// <returns></returns>
		public bool IsActive()
		{
			throw new NotImplementedException();
		}

		public bool IsTileCompatible() => true;
		
	}
}
