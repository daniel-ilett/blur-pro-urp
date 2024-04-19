namespace BlurShadersPro.URP
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class RadialBlur : ScriptableRendererFeature
    {
        class RadialBlurRenderPass : ScriptableRenderPass
        {
            private Material material;
            private RadialBlurSettings settings;

            private RenderTextureDescriptor blurTexDescriptor;
            private RTHandle blurTexHandle;
            private string profilerTag;

            public RadialBlurRenderPass(Material material)
            {
                this.material = material;

                profilerTag = "RadialBlur";
                settings = VolumeManager.instance.stack.GetComponent<RadialBlurSettings>();
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }

            public void EnqueuePass(ScriptableRenderer renderer)
            {
                if (settings != null && settings.IsActive())
                {
                    renderer.EnqueuePass(this);
                }
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (settings == null)
                {
                    return;
                }

                blurTexDescriptor = cameraTextureDescriptor;
                blurTexDescriptor.depthBufferBits = 0;

                RenderingUtils.ReAllocateIfNeeded(ref blurTexHandle, blurTexDescriptor);

                base.Configure(cmd, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

                // Set Radial Blur effect properties.
                material.SetInt("_KernelSize", settings.strength.value);
                material.SetFloat("_Spread", settings.strength.value / 7.5f);
                material.SetFloat("_StepSize", settings.stepSize.value / 1000.0f);

                RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // Perform the Blit operations for the Blur effect.
                using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
                {
                    Blit(cmd, cameraTargetHandle, blurTexHandle);
                    Blit(cmd, blurTexHandle, cameraTargetHandle, material, 0);
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    Destroy(material);
                }
                else
                {
                    DestroyImmediate(material);
                }
#else
                Destroy(material);
#endif

                blurTexHandle?.Release();
            }
        }

        RadialBlurRenderPass pass;

        public override void Create()
        {
            var shader = Shader.Find("BlurShadersProURP/RadialBlur");

            if (shader == null)
            {
                Debug.LogError("Cannot find shader: \"BlurShadersProURP/RadialBlur\".");
                return;
            }

            var material = new Material(shader);

            pass = new RadialBlurRenderPass(material);
            name = "Radial Blur";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            pass.EnqueuePass(renderer);
        }

        protected override void Dispose(bool disposing)
        {
            pass.Dispose();
            base.Dispose(disposing);
        }
    }
}
