#ifndef CUSTOM_BRDF_INCLUDE
#define CUSTOM_BRDF_INCLUDE

//最小金属度 也会有反射现象平均 0.04
#define MIN_REFLECTIVITY 0.04
//0-0.96 防止得不到高光
float OneMinusReflectivity(float metallic)
{
   float range=1.0-MIN_REFLECTIVITY;
   return range-range*metallic;
}

struct BRDF
{
   //漫反射
   float3 diffuse;
   //高光
   float3 specular;
   //粗糙度
   float roughness;
   //根据粗糙度 采取正确的MipMap
   float perceptualRoughness;
   //菲涅尔反射
   float fresnel;
};
//Indirect BRDF IBL
float3 IndirectBRDF(Surface surface,BRDF brdf,float3 diffuse,float3 specular)
{
   //菲涅尔 //
   float fresnelStrength=surface.fresnelStrength*Pow4(1.0-saturate(dot(surface.normal,surface.viewDirection)));
    //反射
    float3 reflection=specular*lerp(brdf.specular,brdf.fresnel,fresnelStrength) ;
    //越粗糙反射越弱
    reflection /=brdf.roughness*brdf.roughness+1.0;
    return (diffuse*brdf.diffuse+reflection)*surface.occlusion;
}

BRDF GetBRDF(Surface surface,bool applyAlphaToDiffuse=false)
{
   BRDF brdf;
   //能量守恒
   float oneMinusReflectivity=OneMinusReflectivity(surface.metallic);
   brdf.diffuse=surface.color*oneMinusReflectivity;
   if(applyAlphaToDiffuse){
     //预乘Alpha漫反射
     brdf.diffuse*=surface.alpha;
   }
   //高光
   brdf.specular=lerp(MIN_REFLECTIVITY,surface.color,surface.metallic);
   brdf.perceptualRoughness=PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
   //粗糙度 Unity
   brdf.roughness=PerceptualRoughnessToRoughness(brdf.perceptualRoughness);
   //菲涅尔
   brdf.fresnel=saturate(surface.smoothness+1.0-oneMinusReflectivity);
   return brdf;
}
//
//高光强度
float SpecularStrength(Surface surface,BRDF brdf,Light light)
{
   //半角向量 灯光方向和视野方向 GGX?
   float3 h=SafeNormalize(light.direction+surface.viewDirection);
   //法线点乘 半角向量
   float nh2=Square(saturate(dot(surface.normal,h)));
   //灯光方向 点乘 半角向量
   float lh2=Square(saturate(dot(light.direction,h)));
   //粗糙度的平方
   float r2=Square(brdf.roughness);
   float d2=Square(nh2*(r2-1.0)+ 1.00001);
   float normalization=brdf.roughness*4.0+2.0;
   return r2/(d2*max(0.1,lh2)*normalization);
}
//高光+漫反射
float3 DirectBRDF(Surface surface,BRDF brdf,Light light)
{
   return SpecularStrength(surface,brdf,light)*brdf.specular+brdf.diffuse;
}
#endif