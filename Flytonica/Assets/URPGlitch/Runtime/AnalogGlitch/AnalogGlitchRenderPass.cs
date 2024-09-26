using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPGlitch.Runtime.AnalogGlitch
{
    sealed class AnalogGlitchRenderPass : ScriptableRenderPass, IDisposable
    {
        const string RenderPassName = "AnalogGlitch RenderPass";

        // Material Properties
        static readonly int MainTexID = Shader.PropertyToID("_MainTex");
        static readonly int ScanLineJitterID = Shader.PropertyToID("_ScanLineJitter");
        static readonly int VerticalJumpID = Shader.PropertyToID("_VerticalJump");
        static readonly int HorizontalShakeID = Shader.PropertyToID("_HorizontalShake");
        static readonly int ColorDriftID = Shader.PropertyToID("_ColorDrift");

        readonly ProfilingSampler _profilingSampler;
        readonly Material _glitchMaterial;
        readonly AnalogGlitchVolume _volume;

        RTHandle _mainFrame;
        float _verticalJumpTime;

        bool isActive =>
            _glitchMaterial != null &&
            _volume != null &&
            _volume.IsActive;

        public AnalogGlitchRenderPass(Shader shader)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _profilingSampler = new ProfilingSampler(RenderPassName);
            _glitchMaterial = CoreUtils.CreateEngineMaterial(shader);

            var volumeStack = VolumeManager.instance.stack;
            _volume = volumeStack.GetComponent<AnalogGlitchVolume>();

            _mainFrame = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_UNorm); // создание RTHandle
        }

        public void Dispose()
        {
            CoreUtils.Destroy(_glitchMaterial);
            _mainFrame.Release();  // освобождение ресурсов RTHandle
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var isPostProcessEnabled = renderingData.cameraData.postProcessEnabled;
            var isSceneViewCamera = renderingData.cameraData.isSceneViewCamera;

            if (!isActive || !isPostProcessEnabled || isSceneViewCamera)
            {
                return;
            }

            var cmd = CommandBufferPool.Get(RenderPassName);
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                cameraTargetDescriptor.depthBufferBits = 0;
                
                cmd.Blit(source, _mainFrame.nameID);

                // Применение глитч-эффекта
                var scanLineJitter = _volume.scanLineJitter.value;
                var verticalJump = _volume.verticalJump.value;
                var horizontalShake = _volume.horizontalShake.value;
                var colorDrift = _volume.colorDrift.value;

                _verticalJumpTime += Time.deltaTime * verticalJump * 11.3f;

                var slThresh = Mathf.Clamp01(1.0f - scanLineJitter * 1.2f);
                var slDisp = 0.002f + Mathf.Pow(scanLineJitter, 3) * 0.05f;
                _glitchMaterial.SetVector(ScanLineJitterID, new Vector2(slDisp, slThresh));

                var vj = new Vector2(verticalJump, _verticalJumpTime);
                _glitchMaterial.SetVector(VerticalJumpID, vj);
                _glitchMaterial.SetFloat(HorizontalShakeID, horizontalShake * 0.2f);

                var cd = new Vector2(colorDrift * 0.04f, Time.time * 606.11f);
                _glitchMaterial.SetVector(ColorDriftID, cd);

                cmd.SetGlobalTexture(MainTexID, _mainFrame.nameID);
                cmd.Blit(_mainFrame.nameID, source, _glitchMaterial);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
