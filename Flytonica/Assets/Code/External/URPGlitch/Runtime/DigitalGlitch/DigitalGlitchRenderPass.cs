using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPGlitch.Runtime.DigitalGlitch
{
    sealed class DigitalGlitchRenderPass : ScriptableRenderPass, IDisposable
    {
        const string RenderPassName = "DigitalGlitch RenderPass";

        static readonly int MainTexID = Shader.PropertyToID("_MainTex");
        static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
        static readonly int TrashTexID = Shader.PropertyToID("_TrashTex");
        static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        readonly ProfilingSampler _profilingSampler;
        readonly System.Random _random;

        readonly Material _glitchMaterial;
        readonly Texture2D _noiseTexture;
        readonly DigitalGlitchVolume _volume;

        RTHandle _mainFrame;
        RTHandle _trashFrame1;
        RTHandle _trashFrame2;

        bool isActive =>
            _glitchMaterial != null &&
            _volume != null &&
            _volume.IsActive;

        public DigitalGlitchRenderPass(Shader shader)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _profilingSampler = new ProfilingSampler(RenderPassName);
            _random = new System.Random();
            _glitchMaterial = CoreUtils.CreateEngineMaterial(shader);

            _noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            var volumeStack = VolumeManager.instance.stack;
            _volume = volumeStack.GetComponent<DigitalGlitchVolume>();

            _mainFrame = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_UNorm);
            _trashFrame1 = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_UNorm);
            _trashFrame2 = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_UNorm);

            UpdateNoiseTexture();
        }

        public void Dispose()
        {
            CoreUtils.Destroy(_glitchMaterial);
            CoreUtils.Destroy(_noiseTexture);
            _mainFrame.Release();
            _trashFrame1.Release();
            _trashFrame2.Release();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsActiveForRendering(renderingData)) return;

            var cmd = CommandBufferPool.Get(RenderPassName);
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                cameraTargetDescriptor.depthBufferBits = 0;

                cmd.Blit(source, _mainFrame);

                var frameCount = Time.frameCount;
                if (frameCount % 13 == 0) cmd.Blit(source, _trashFrame1);
                if (frameCount % 73 == 0) cmd.Blit(source, _trashFrame2);

                var r = (float)_random.NextDouble();
                var blitTrashHandle = r > 0.5f ? _trashFrame1 : _trashFrame2;
                cmd.SetGlobalFloat(IntensityID, _volume.intensity.value);
                cmd.SetGlobalTexture(NoiseTexID, _noiseTexture);
                cmd.SetGlobalTexture(MainTexID, _mainFrame);
                cmd.SetGlobalTexture(TrashTexID, blitTrashHandle);

                cmd.Blit(_mainFrame, source, _glitchMaterial);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void UpdateNoiseTexture()
        {
            var color = randomColor;

            for (var y = 0; y < _noiseTexture.height; y++)
            {
                for (var x = 0; x < _noiseTexture.width; x++)
                {
                    var r = (float)_random.NextDouble();
                    if (r > 0.89f)
                    {
                        color = randomColor;
                    }

                    _noiseTexture.SetPixel(x, y, color);
                }
            }

            _noiseTexture.Apply();
        }

        Color randomColor
        {
            get
            {
                var r = (float)_random.NextDouble();
                var g = (float)_random.NextDouble();
                var b = (float)_random.NextDouble();
                var a = (float)_random.NextDouble();
                return new Color(r, g, b, a);
            }
        }

        private bool IsActiveForRendering(RenderingData renderingData)
        {
            var isPostProcessEnabled = renderingData.cameraData.postProcessEnabled;
            var isSceneViewCamera = renderingData.cameraData.isSceneViewCamera;
            return isActive && isPostProcessEnabled && !isSceneViewCamera;
        }
    }
}