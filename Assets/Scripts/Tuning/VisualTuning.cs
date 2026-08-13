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

        [Header("Player placement on screen")]
        [Tooltip("Where the player sits horizontally, as a screen fraction from the left (0..1)")]
        [Range(0.05f, 0.95f)] public float playerScreenX = 0.35f;
        [Tooltip("Where the road surface sits vertically, as a screen fraction from the bottom (0..1)")]
        [Range(0.05f, 0.95f)] public float roadScreenY = 0.34f;

        [Header("World")]
        public Color skyColor = new Color(0.52f, 0.78f, 0.92f);
        public float mountainScaleMin = 1.5f;
        public float mountainScaleMax = 2.5f;

        [Header("Generated art (Resources/Art)")]
        [Tooltip("Rider sheet pixels per world unit (128px frame / 56 ≈ 2.3 m bike)")]
        public float riderArtPixelsPerUnit = 56f;
        [Tooltip("Environment atlas pixels per world unit (16px grid)")]
        public float envArtPixelsPerUnit = 16f;
    }
}
