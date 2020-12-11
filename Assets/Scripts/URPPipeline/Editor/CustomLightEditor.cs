using UnityEditor;
using UnityEngine;
/// <summary>
/// 重写灯光面板
/// </summary>
[CanEditMultipleObjects]
[CustomEditorForRenderPipeline(typeof(Light),typeof(CustomRenderPipelineAsset))]
public class CustomLightEditor : LightEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //多选的物体是否有不同的值 相同的值并且是聚光灯类型
        if(!settings.lightType.hasMultipleDifferentValues && (LightType)settings.lightType.enumValueIndex==LightType.Spot)
        {
            settings.DrawInnerAndOuterSpotAngle();
            settings.ApplyModifiedProperties();
        }
    }
}
