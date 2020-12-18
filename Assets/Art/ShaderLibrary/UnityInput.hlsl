#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED
//per draw 物体到世界矩阵
CBUFFER_START(UnityPerDraw)
  float4x4 unity_ObjectToWorld;
  float4x4 unity_WorldToObject;
  float4 unity_LODFade;
  real4 unity_WorldTransformParams;
  //渲染层
  float4 unity_RenderingLayer;
  //其他灯光数据
  real4 unity_LightData;
  real4 unity_LightIndices[2];
  //动态物体 阴影 Bake 到 LightProbe
  float4 unity_ProbesOcclusion;
  //反射探针
  float4 unity_SpecCube0_HDR;
  //LightMap 数据
  float4 unity_LightmapST;
  //不会打断Srp Batch
  float4 unity_DynamicLightmapST;
  //Light Probe 球谐光
  float4 unity_SHAr;
  float4 unity_SHAg;
  float4 unity_SHAb;

  float4 unity_SHBr;
  float4 unity_SHBg;
  float4 unity_SHBb;
  float4 unity_SHC;
  //LPPVs 对一些大的动物体接收场景间接光部分 3D float texture
  float4 unity_ProbeVolumeParams;
  float4x4 unity_ProbeVolumeWorldToObject;
  float4 unity_ProbeVolumeSizeInv;
  float4 unity_ProbeVolumeMin;
CBUFFER_END

//世界空间转换裁剪矩阵
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;
//世界空间摄像机位置
float3 _WorldSpaceCameraPos;
//????
float4 unity_OrthoParams;
//?????
float4 _ProjectionParams;
//
float4 _ScreenParams;
float4 _ZBufferParams;
#endif