﻿using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using LightType = UnityEngine.LightType;

public partial class CustomRenderPipeline 
{
    partial void InitializeForEditor();
    partial void DisposeForEditor();
#if UNITY_EDITOR

    partial void InitializeForEditor()
    {
        Lightmapping.SetDelegate(lightsDelegate);
    }

    partial void DisposeForEditor()
    {
        //base.Dispose(disposing);
        Lightmapping.ResetDelegate();
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DisposeForEditor();
        renderer.Dispose();
    }
    static Lightmapping.RequestLightsDelegate lightsDelegate = (Light[] lights,NativeArray<LightDataGI> output)=> 
    {
        var lightData = new LightDataGI();
        for(int i=0;i<lights.Length;i++)
        {
            Light light = lights[i];
            switch(light.type)
            {
                case LightType.Directional:
                    var directionalLight = new DirectionalLight();
                    LightmapperUtils.Extract(light,ref directionalLight);
                    lightData.Init(ref directionalLight);
                    break;
                case LightType.Point:
                    var pointLight = new PointLight();
                    LightmapperUtils.Extract(light, ref pointLight);
                    lightData.Init(ref pointLight);
                    break;
                case LightType.Spot:
                    var spotLight = new SpotLight();
                    LightmapperUtils.Extract(light, ref spotLight);
                    //设置聚光灯内角
                    spotLight.innerConeAngle = light.innerSpotAngle * Mathf.Deg2Rad;
                    spotLight.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
                    lightData.Init(ref spotLight);
                    break;
                case LightType.Area:
                    var rectangleLight = new RectangleLight();
                    LightmapperUtils.Extract(light, ref rectangleLight);
                    rectangleLight.mode = LightMode.Baked;
                    lightData.Init(ref rectangleLight);
                    break;
                default:
                    lightData.InitNoBake(light.GetInstanceID());
                    break;
            }
            //设置衰减类型
            lightData.falloff = FalloffType.InverseSquared;
            output[i] = lightData;
        }
    }; 
    #endif
}
