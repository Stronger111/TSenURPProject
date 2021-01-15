using UnityEngine;
using System;

[CreateAssetMenu(menuName ="Rendering/Custom Post FX Settings")]
public class PostFXSettings : ScriptableObject
{
    [SerializeField]
    Shader shader = default;
    #region 测试Shader功能
    [SerializeField]
    Shader skinBlitShader = default;
    [SerializeField]
    Texture skinTexture = null;
    [SerializeField]
    Texture baseSkinTexture = null;
    #endregion
    [System.NonSerialized]
    Material material;
    [System.NonSerialized]
    Material skinBlitMaterial;
    public Material Material
    {
        get
        {
            if (material == null && shader != null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }
    public Material SkinBlitMaterial
    {
        get
        {
            if(skinBlitMaterial == null && skinBlitShader!=null)
            {
                skinBlitMaterial = new Material(skinBlitShader);
                skinBlitMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return skinBlitMaterial;
        }
    }
    public Texture BaseSkinTexture
    {
        get
        {
            if (baseSkinTexture != null)
                return baseSkinTexture;
            return null;
        }
    }
    public Texture SkinTexture
    {
        get
        {
            if(skinTexture != null)
            {
                return skinTexture;
            }
            return null;
        }
    }
    [System.Serializable]
    public struct BloomSettings
    {
        [Range(0f,16f)]
        public int maxIterations;
        [Min(1f)]
        public int downscaleLimit;
        /// <summary>
        /// 三线性过滤
        /// </summary>
        public bool bicubicUpsampling;
        /// <summary>
        /// 阈值
        /// </summary>
        [Min(0f)]
        public float threshold;
        [Range(0f,1f)]
        public float thresholdKnee;
        [Min(0f)]
        public float intensity;
        /// <summary>
        /// 解决摄像机移动闪闪发光
        /// </summary>
        public bool fadeFireflies;
        public enum Mode { Additive, Scattering }
        public Mode mode;
        [Range(0.05f,0.95f)]
        public float scatter;
    }

    [Serializable]
    public struct ColorAdjustmentsSetting
    {
        /// <summary>
        /// 曝光值
        /// </summary>
        public float postExposure;
        [Range(-100f,100f)]
        public float contrast;
        [ColorUsage(false,true)]
        public Color colorFilter;
        [Range(-180f,180f)]
        public float hueShift;
        [Range(-100f,100f)]
        public float saturation;
    }
    [Serializable]
    public struct WhiteBalanceSettings
    {
        /// <summary>
        /// 温度和色彩
        /// </summary>
        public float temperature, tint;
    }
    [Serializable]
    public struct SplitToningSettings
    {
        [ColorUsage(false)]
        public Color shadows, highlights;
        [Range(-100f,100f)]
        public float balance;
    }
    [Serializable]
    public struct ChannelMixerSettings
    {
        public Vector3 red, green, blue;
    }
    [Serializable]
    public struct ShadowMidtonesHighlightsSettngs
    {
        [ColorUsage(false,true)]
        public Color shadows, midtones, highlights;
        [Range(0f, 2f)]
        public float shadowsStart, shadowsEnd, highlightsStart, highLightsEnd;
    }
    [Serializable]
    public struct ToneMappingSettings
    {
        public enum Mode { None,ACES,Neutral,Reinhard }
        public Mode mode;
    }

    [SerializeField]
    BloomSettings bloom = new BloomSettings { scatter=0.7f};
    public BloomSettings Bloom => bloom;
    [SerializeField]
    ColorAdjustmentsSetting colorAdjustments = new ColorAdjustmentsSetting { colorFilter=Color.white};
    [SerializeField]
    WhiteBalanceSettings whiteBalance = default;
    [SerializeField]
    SplitToningSettings splitToning = new SplitToningSettings { shadows = Color.gray, highlights = Color.gray };
    [SerializeField]
    ChannelMixerSettings channelMixer = new ChannelMixerSettings
    {
        red=Vector3.right,
        green=Vector3.up,
        blue=Vector3.forward
    };
    [SerializeField]
    ShadowMidtonesHighlightsSettngs shadowsMidtonesHighlights = new ShadowMidtonesHighlightsSettngs
    {
        shadows=Color.white,
        midtones=Color.white,
        highlights=Color.white,
        shadowsEnd=0.3f,
        highlightsStart=0.55f,
        highLightsEnd=1f
    };
    public WhiteBalanceSettings WhiteBalance => whiteBalance;
    public SplitToningSettings SplitToning => splitToning;
    public ChannelMixerSettings ChannelMixer => channelMixer;
    public ShadowMidtonesHighlightsSettngs ShadowsMidtonesHighlights => shadowsMidtonesHighlights;
    [SerializeField]
    ToneMappingSettings toneMapping = default;
    public ColorAdjustmentsSetting ColorAdjustments => colorAdjustments;
    public ToneMappingSettings ToneMapping => toneMapping;
}
