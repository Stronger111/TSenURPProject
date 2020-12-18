#ifndef FRAMENT_INCLUDE
#define FRAMENT_INCLUDE

TEXTURE2D(_CameraColorTexture);
SAMPLER(sampler_CameraColorTexture);
TEXTURE2D(_CameraDepthTexture);

struct Fragment
{
   float2 positionSS;
   float2 screenUV;
   //深度
   float depth;
   //缓冲区深度
   float bufferDepth;
};

Fragment GetFragment(float4 positionSS)
{
   Fragment f;
   f.positionSS=positionSS.xy;
   f.screenUV=f.positionSS/_ScreenParams.xy;
   //屏幕空间位置的W分量当中
   f.depth=IsOrthographicCamera()? OrthographicDepthBufferToLinear(positionSS.z): positionSS.w;
   f.bufferDepth=SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_point_clamp,f.screenUV);
   //f.bufferDepth = LOAD_TEXTURE2D(_CameraDepthTexture, f.positionSS).r;
   f.bufferDepth=IsOrthographicCamera()? OrthographicDepthBufferToLinear(f.bufferDepth) : LinearEyeDepth(f.bufferDepth,_ZBufferParams);
   return f;
}
//获取颜色
float4 GetBufferColor(Fragment fragment,float2 uvOffset=float2(0.0,0.0))
{
    float2 uv=fragment.screenUV+uvOffset;
    return SAMPLE_TEXTURE2D(_CameraColorTexture,sampler_CameraColorTexture,uv);
}
#endif