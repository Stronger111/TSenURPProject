using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

[Serializable]
public class CameraSettings
{
    [Serializable]
   public struct FinalBlendMode
   {
        public BlendMode source, destination;
   }
    public bool copyColor=true,copyDepth = true;
    //是否覆盖全局后期设置
    [RenderingLayerMaskFieldAttribute]
    public int renderingLayerMask = -1;
    public bool maskLights = false;
    public bool overridePostFX = false;
    public PostFXSettings postFXSettings = default;
    public FinalBlendMode finalBlendMode = new FinalBlendMode {source=BlendMode.One,destination=BlendMode.Zero };
}
