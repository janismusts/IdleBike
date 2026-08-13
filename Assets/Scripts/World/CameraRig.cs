using UnityEngine;

namespace IdleBike
{
    /// <summary>Orthographic camera that zooms out as speed grows, keeping the player readable.</summary>
    public class CameraRig : MonoBehaviour
    {
        public Camera Cam { get; private set; }
        float _sizeVelocity;

        public void Build()
        {
            var v = Tuning.Visual;
            Cam = gameObject.AddComponent<Camera>();
            Cam.orthographic = true;
            Cam.orthographicSize = v.camMinSize;
            Cam.backgroundColor = v.skyColor;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.nearClipPlane = -10f;
            Cam.farClipPlane = 100f;
            gameObject.AddComponent<AudioListener>();
        }

        void LateUpdate()
        {
            if (Cam == null || GameState.Data == null) return;
            var v = Tuning.Visual;
            float targetSize = Mathf.Clamp(
                v.camMinSize + GameState.CurrentSpeed * v.camSizePerSpeed,
                v.camMinSize, v.camMaxSize);
            float size = Mathf.SmoothDamp(Cam.orthographicSize, targetSize, ref _sizeVelocity, v.camSmoothTime);
            Cam.orthographicSize = size;

            // Solve camera position so the player (x=0) lands at playerScreenX of the
            // viewport and the road surface (y=0) at roadScreenY — both designer-tunable.
            float halfW = size * Cam.aspect;
            float camX = halfW * (1f - 2f * v.playerScreenX);
            float camY = size * (1f - 2f * v.roadScreenY);
            transform.position = new Vector3(camX, camY, -10f);
        }
    }
}
