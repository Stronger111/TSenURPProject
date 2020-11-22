#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED
//per draw 物体到世界矩阵
CBUFFER_START(UnityPerDraw)
  float4x4 unity_ObjectToWorld;
  float4x4 unity_WorldToObject;
  float4 unity_LODFade;
  real4 unity_WorldTransformParams;
CBUFFER_END

//世界空间转换裁剪矩阵
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;
//世界空间摄像机位置
float3 _WorldSpaceCameraPos;

#endif