using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPULog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Tyson:" + "SystemInfo.deviceModel: " + SystemInfo.deviceModel);
        Debug.Log("Tyson:" + "SystemInfo.deviceName: " + SystemInfo.deviceName);
        Debug.Log("Tyson:" + "SystemInfo.deviceType: " + SystemInfo.deviceType);
        Debug.Log("Tyson:" + "SystemInfo.deviceUniqueIdentifier: " + SystemInfo.deviceUniqueIdentifier);
        Debug.Log("Tyson:" + "SystemInfo.systemMemorySize: " + SystemInfo.systemMemorySize);
        Debug.Log("Tyson:" + "SystemInfo.operatingSystem: " + SystemInfo.operatingSystem);
        Debug.Log("Tyson:" + "SystemInfo.graphicsDeviceID: " + SystemInfo.graphicsDeviceID);
        Debug.Log("Tyson:" + "SystemInfo.graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
        Debug.Log("Tyson:" + "SystemInfo.graphicsDeviceType: " + SystemInfo.graphicsDeviceType);
        Debug.Log("Tyson:" + "SystemInfo.graphicsDeviceVendorID: " + SystemInfo.graphicsDeviceVendorID);
        Debug.Log("Tyson:" + "SystemInfo.graphicsDeviceVersion: " + SystemInfo.graphicsDeviceVersion);
        Debug.Log("Tyson:" + "SystemInfo.graphicsMemorySize: " + SystemInfo.graphicsMemorySize);
        Debug.Log("Tyson:" + "SystemInfo.graphicsMultiThreaded: " + SystemInfo.graphicsMultiThreaded);
        Debug.Log("Tyson:" + "SystemInfo.supportedRenderTargetCount: " + SystemInfo.supportedRenderTargetCount);
        Debug.Log("Tyson:" + "SystemInfo.graphicsShaderLevel: " + SystemInfo.graphicsShaderLevel);
        Debug.Log("Tyson:" + "SystemInfo.maxTextureSize: " + SystemInfo.maxTextureSize);
        Debug.Log("Tyson:" + "SystemInfo.npotSupport: " + SystemInfo.npotSupport);
        Debug.Log("Tyson:" + "SystemInfo.processorCount: " + SystemInfo.processorCount);
        Debug.Log("Tyson:" + "SystemInfo.processorFrequency: " + SystemInfo.processorFrequency);
        Debug.Log("Tyson:" + "SystemInfo.processorType: " + SystemInfo.processorType);
        Debug.Log("Tyson:" + "SystemInfo.supports2DArrayTextures: " + SystemInfo.supports2DArrayTextures);
        Debug.Log("Tyson:" + "SystemInfo.supports3DRenderTextures: " + SystemInfo.supports3DRenderTextures);
        Debug.Log("Tyson:" + "SystemInfo.supportsAccelerometer: " + SystemInfo.supportsAccelerometer);
        Debug.Log("Tyson:" + "SystemInfo.supportsAudio: " + SystemInfo.supportsAudio);
        Debug.Log("Tyson:" + "SystemInfo.supportsComputeShaders: " + SystemInfo.supportsComputeShaders);
        //Debug.Log("Tyson:" + "SystemInfo.supportsImageEffects: " + SystemInfo.supportsImageEffects);
        Debug.Log("Tyson:" + "SystemInfo.supportsInstancing: " + SystemInfo.supportsInstancing);
        Debug.Log("Tyson:" + "SystemInfo.supportsLocationService: " + SystemInfo.supportsLocationService);
        Debug.Log("Tyson:" + "SystemInfo.supportsRawShadowDepthSampling: " + SystemInfo.supportsRawShadowDepthSampling);
        //Debug.Log("Tyson:" + "SystemInfo.supportsRenderToCubemap: " + SystemInfo.supportsRenderToCubemap);
        Debug.Log("Tyson:" + "SystemInfo.supportsShadows: " + SystemInfo.supportsShadows);
        Debug.Log("Tyson:" + "SystemInfo.supportsSparseTextures: " + SystemInfo.supportsSparseTextures);
        Debug.Log("Tyson:" + "SystemInfo.supportsVibration: " + SystemInfo.supportsVibration);
        //Debug.Log("Tyson:" + "SystemInfo.supportsRenderTextures: " + SystemInfo.supportsRenderTextures);
        Debug.Log("Tyson:" + "SystemInfo.supportsMotionVectors: " + SystemInfo.supportsMotionVectors);
        Debug.Log("Tyson:" + "SystemInfo.supports3DTextures: " + SystemInfo.supports3DTextures);
        Debug.Log("Tyson:" + "SystemInfo.supportsCubemapArrayTextures: " + SystemInfo.supportsCubemapArrayTextures);
        Debug.Log("Tyson:" + "SystemInfo.copyTextureSupport: " + SystemInfo.copyTextureSupport);
        Debug.Log("Tyson:" + "SystemInfo.supportsHardwareQuadTopology: " + SystemInfo.supportsHardwareQuadTopology);
        Debug.Log("Tyson:" + "SystemInfo.supports32bitsIndexBuffer: " + SystemInfo.supports32bitsIndexBuffer);
        Debug.Log("Tyson:" + "SystemInfo.supportsSeparatedRenderTargetsBlend: " + SystemInfo.supportsSeparatedRenderTargetsBlend);
        Debug.Log("Tyson:" + "SystemInfo.supportsMultisampledTextures: " + SystemInfo.supportsMultisampledTextures);
        Debug.Log("Tyson:" + "SystemInfo.supportsMultisampleAutoResolve: " + SystemInfo.supportsMultisampleAutoResolve);
        Debug.Log("Tyson:" + "SystemInfo.supportsTextureWrapMirrorOnce: " + SystemInfo.supportsTextureWrapMirrorOnce);
        Debug.Log("Tyson:" + "SystemInfo.usesReversedZBuffer: " + SystemInfo.usesReversedZBuffer);
        //Debug.Log("Tyson:" + "SystemInfo.supportsStencil: " + SystemInfo.supportsStencil);
        Debug.Log("Tyson:" + "SystemInfo.maxCubemapSize: " + SystemInfo.maxCubemapSize);
        Debug.Log("Tyson:" + "SystemInfo.supportsAsyncCompute: " + SystemInfo.supportsAsyncCompute);
        //Debug.Log("Tyson:" + "SystemInfo.supportsGPUFence: " + SystemInfo.supportsGPUFence);
        Debug.Log("Tyson:" + "SystemInfo.supportsAsyncGPUReadback: " + SystemInfo.supportsAsyncGPUReadback);
        Debug.Log("Tyson:" + "SystemInfo.supportsMipStreaming: " + SystemInfo.supportsMipStreaming);
        Debug.Log("Tyson:" + "SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders: " + SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders);
        Debug.Log("Tyson:" + "SystemInfo.hasHiddenSurfaceRemovalOnGPU: " + SystemInfo.hasHiddenSurfaceRemovalOnGPU);
        Debug.Log("Tyson:" + "SystemInfo.batteryLevel: " + SystemInfo.batteryLevel);
        Debug.Log("Tyson:" + "SystemInfo.batteryStatus: " + SystemInfo.batteryStatus);
        Debug.Log("Tyson:" + "SystemInfo.operatingSystemFamily: " + SystemInfo.operatingSystemFamily);
        //Debug.Log("Tyson:" + "SystemInfo.graphicsPixelFillrate: " + SystemInfo.graphicsPixelFillrate);
        //Debug.Log("Tyson:" + "SystemInfo.supportsGyroscope: " + SystemInfo.supportsGyroscope);
        Debug.Log("Tyson:" + "SystemInfo.graphicsUVStartsAtTop: " + SystemInfo.graphicsUVStartsAtTop);
        Debug.Log("Tyson:" + "SystemInfo.graphicsDeviceVendor: " + SystemInfo.graphicsDeviceVendor);
        //Debug.Log("Tyson:" + "SystemInfo.supportsVertexPrograms: " + SystemInfo.supportsVertexPrograms);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
