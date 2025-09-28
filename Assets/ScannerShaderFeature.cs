using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

public class ScannerShaderFeature : ScriptableRendererFeature
{
    public Shader overrideShader;
    class CustomRenderPass : ScriptableRenderPass
    {
        public Shader overrideShader;
        public Material overrideMaterial;
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // VolumeStack stack = VolumeManager.instance.stack;
            // CommandBuffer cmd = CommandBufferPool.Get();
            //
            // using (new ProfilingScope(cmd, new ProfilingSampler("Custom Render Pass")))
            // {
            //     Debug.Log("Blitting");
            //     Blitter.BlitCameraTexture(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, renderingData.cameraData.renderer.cameraColorTargetHandle, overrideMaterial, 0);
            // }
            //
            // context.ExecuteCommandBuffer(cmd);
            // cmd.Clear();
            // CommandBufferPool.Release(cmd);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Render Custom Pass";

            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // Use this scope to set the required inputs and outputs of the pass and to
                // setup the passData with the required properties needed at pass execution time.

                // Make use of frameData to access resources and camera data through the dedicated containers.
                // Eg:
                // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // Setup pass inputs and outputs through the builder interface.
                // Eg:
                // builder.UseTexture(sourceTexture);
                // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // VolumeStack stack = VolumeManager.instance.stack;
            // CommandBuffer cmd = CommandBufferPool.Get();
            //
            // using (new ProfilingScope(cmd, new ProfilingSampler("Custom Render Pass")))
            // {
            //     Debug.Log("Blitting");
            //     Blitter.BlitCameraTexture(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, renderingData.cameraData.renderer.cameraColorTargetHandle, overrideMaterial, 0);
            // }
            //
            // context.ExecuteCommandBuffer(cmd);
            // cmd.Clear();
            // CommandBufferPool.Release(cmd);
        }

        // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        // public override DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, ref RenderingData renderingData, SortingCriteria sortingCriteria)
        // {
        //     var settings = base.CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
        //     settings.overrideShader = overrideShader;
        //     return settings;
        // }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        Debug.Log("Create CustomShaderFeature");
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.overrideMaterial = CoreUtils.CreateEngineMaterial(overrideShader);
        m_ScriptablePass.overrideShader = overrideShader;

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Debug.Log("AddRenderPasses");
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // public override void Dispose()
    // {
    //     CoreUtils.Destroy(overrideMaterial);
    // }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color);
            // m_ScriptablePass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);

        }
    }
}
