﻿using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;

    //设置的属性
    bool Clipping { set => SetProperty("_Clipping", "_CLIPPING",value); }
    bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach(Material m in materials)
            {
                m.renderQueue = (int)value;
            }
        }
    }
    bool HasProperty(string name) =>
        FindProperty(name, properties, false) != null;

    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");

    bool showPresets;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        editor = materialEditor;
        materials = editor.targets;
        this.properties = properties;
        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets,"Presets",true);
        if(showPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            if (HasPremultiplyAlpha && PresetButton("Transparent"))
            {
                TransparentPreset();
            }
        }
    }

    bool SetProperty(string name,float value)
    {
        MaterialProperty property = FindProperty(name,properties,false);
        if(property!=null)
        {
            property.floatValue = value;
            return true;
        }
        return false;
    }

    void SetProperty(string name, string keyword,bool value)
    {
        if (SetProperty(name, value ? 1f : 0.0f))
        {
            SetKeyworld(keyword, value);
        }
    }
    void SetKeyworld(string keyword,bool enabled)
    {
        if(enabled)
        {
            foreach(Material m in materials)
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach(Material m in materials)
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    bool PresetButton(string name)
    {
        if(GUILayout.Button(name))
        {
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 不透明材质
    /// </summary>
    void OpaquePreset()
    {
        if(PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }
    /// <summary>
    /// Alpha Test
    /// </summary>
    void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }
    /// <summary>
    /// Transpart
    /// </summary>
    void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
    /// <summary>
    /// 预乘Alpha
    /// </summary>
    void TransparentPreset()
    {
        if (PresetButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

}
