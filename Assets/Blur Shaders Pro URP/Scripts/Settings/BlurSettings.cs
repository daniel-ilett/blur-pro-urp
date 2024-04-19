namespace BlurShadersPro.URP
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [System.Serializable, VolumeComponentMenu("Blur Shaders Pro/Blur")]
    public sealed class BlurSettings : VolumeComponent, IPostProcessComponent
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Tooltip("Blur Strength")]
        public ClampedIntParameter strength = new ClampedIntParameter(5, 1, 101);

        public bool IsActive()
        {
            return strength.value > 1 && active;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}
