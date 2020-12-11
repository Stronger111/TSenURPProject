using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowSettings 
{
    /// <summary>
    /// 最大阴影到达的距离
    /// </summary>
    [Min(0.001f)]
    public float maxDistance = 100f;
    [Range(0.001f, 1f)]
    public float distanceFade = 0.1f;
    /// <summary>
    /// Shadow Map Size
    /// </summary>
    public enum TextureSize { _256=256,_512=512,_1024=1024,_2048=2048,_4096=4096,_8192=8192}
    /// <summary>
    /// 软阴影过滤方式
    /// </summary>
    public enum FilterMode {PCF2x2,PCF3x3,PCF5x5,PCF7x7 }
    public enum CascadeBlendMode { Hard,Soft,Dither}
    /// <summary>
    /// 方向光Shadow Map 大小
    /// </summary>
    public Directional directional = new Directional { atlasSize=TextureSize._1024,cascadeCount=4,cascadeRatio1=0.1f,cascadeRatio2=0.25f,cascadeRatio3=0.5f,cascadeFade=0.1f,
    cascadeBlend=CascadeBlendMode.Hard};
    /// <summary>
    /// 其他灯光设置
    /// </summary>
    public Other other = new Other { 
        atlasSize=TextureSize._1024,
        filter=FilterMode.PCF2x2
        };
 
    /// <summary>
    /// 单张纹理包含多个Shadow Map
    /// </summary>
    [System.Serializable]
    public struct Directional
    {
        /// <summary>
        /// Shadow Map 大小
        /// </summary>
        public TextureSize atlasSize;
        public FilterMode filter;
        /// <summary>
        /// Cascade 的数量
        /// </summary>
        [Range(1, 4)]
        public int cascadeCount;
        [Range(0f, 1f)]
        public float cascadeRatio1, cascadeRatio2, cascadeRatio3;
        /// <summary>
        /// Cascade Ratio
        /// </summary>
        public Vector3 CascadeRatios => new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
        /// <summary>
        /// 两个cascade之间的过渡
        /// </summary>
        [Range(0.001f,1f)]
        public float cascadeFade;
        /// <summary>
        /// 混合模式
        /// </summary>
        public CascadeBlendMode cascadeBlend;
    }
    [System.Serializable]
    public struct Other
    {
        public TextureSize atlasSize;
        public FilterMode filter;
    }
}


