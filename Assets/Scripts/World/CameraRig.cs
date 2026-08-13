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

            // player at x=0; keep them left of center, road in the lower third
            float halfW = size * Cam.aspect;
            transform.position = new Vector3(halfW * v.camXOffsetFactor, size * v.camYOffsetFactor, -10f);
        }
    }
}
