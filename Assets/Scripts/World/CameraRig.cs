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
            Cam = gameObject.AddComponent<Camera>();
            Cam.orthographic = true;
            Cam.orthographicSize = GameConfig.CamMinSize;
            Cam.backgroundColor = new Color(0.52f, 0.78f, 0.92f);
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.nearClipPlane = -10f;
            Cam.farClipPlane = 100f;
            gameObject.AddComponent<AudioListener>();
        }

        void LateUpdate()
        {
            if (Cam == null || GameState.Data == null) return;
            float targetSize = Mathf.Clamp(
                GameConfig.CamMinSize + GameState.CurrentSpeed * GameConfig.CamSizePerSpeed,
                GameConfig.CamMinSize, GameConfig.CamMaxSize);
            float size = Mathf.SmoothDamp(Cam.orthographicSize, targetSize, ref _sizeVelocity, GameConfig.CamSmoothTime);
            Cam.orthographicSize = size;

            // player at x=0; keep them ~35% from the left edge, road in lower third
            float halfW = size * Cam.aspect;
            transform.position = new Vector3(halfW * 0.3f, size * 0.32f, -10f);
        }
    }
}
