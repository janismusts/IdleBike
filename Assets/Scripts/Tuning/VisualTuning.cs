using UnityEngine;

namespace IdleBike
{
    [CreateAssetMenu(fileName = "VisualTuning", menuName = "IdleBike/Visual Tuning")]
    public class VisualTuning : ScriptableObject
    {
        [Header("Camera (portrait: visible width = 2 * size * aspect)")]
        public float camMinSize = 7.5f;
        public float camMaxSize = 26f;
        [Tooltip("Ortho size added per m/s of current speed")]
        public float camSizePerSpeed = 0.35f;
        public float camSmoothTime = 0.6f;
        [Tooltip("Camera x = halfWidth * this (keeps player left of center)")]
        public float camXOffsetFactor = 0.3f;
        [Tooltip("Camera y = size * this (keeps road in lower third)")]
        public float camYOffsetFactor = 0.32f;

        [Header("World")]
        public Color skyColor = new Color(0.52f, 0.78f, 0.92f);
        public float mountainScaleMin = 1.5f;
        public float mountainScaleMax = 2.5f;
    }
}
