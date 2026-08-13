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

        [Header("Mountains layer")]
        [Tooltip("Fraction of world scroll speed (0 = static, 1 = moves with the road)")]
        public float mountainParallax = 0.08f;
        public float mountainY = 0.4f;
        public float mountainScale = 2.2f;
        public int mountainTiles = 6;
        [Tooltip("Procedural fallback only")]
        public float mountainScaleMin = 1.5f;
        public float mountainScaleMax = 2.5f;

        [Header("Clouds layer")]
        public float cloudParallax = 0.05f;
        public int cloudCount = 7;
        public float cloudMinY = 5f;
        public float cloudMaxY = 11f;
        public float cloudMinScale = 1f;
        public float cloudMaxScale = 2.4f;

        [Header("Hills layer")]
        public float hillParallax = 0.25f;
        public float hillY = 0.15f;
        public float hillScale = 1.6f;
        public int hillTiles = 8;

        [Header("Trees layer (tilts with the road)")]
        public float treeParallax = 1f;
        public int treeCount = 14;
        public float treeY = 0.05f;
        public float treeMinScale = 0.7f;
        public float treeMaxScale = 1.1f;

        [Header("Road & riders")]
        public float roadScale = 1f;
        [Tooltip("Extra scale on all riders (player, NPCs, teammates)")]
        public float riderScale = 1f;

        [Header("Rider attachments (local units on the rider)")]
        [Tooltip("Helmet overlay fine-tune relative to the rider frame")]
        public Vector2 helmetOffset = Vector2.zero;
        [Tooltip("Trail effect anchor (behind the rear wheel)")]
        public Vector2 trailOffset = new Vector2(-0.55f, 0.02f);
        public float trailScale = 1f;
        [Tooltip("Emote speech bubble anchor above the head")]
        public Vector2 emoteBubbleOffset = new Vector2(0.35f, 2.05f);
        public float emoteBubbleScale = 1f;
        [Tooltip("Teammate name tag height above the rider")]
        public float nameTagHeight = 1.75f;

        [Header("UI icon sizes (canvas px)")]
        public float barIconSize = 84f;
        public float hudTopIconSize = 64f;
        public float emotePickerIconSize = 64f;

        [Header("Generated art (Resources/Art)")]
        [Tooltip("Rider sheet pixels per world unit (128px frame / 56 ≈ 2.3 m bike)")]
        public float riderArtPixelsPerUnit = 56f;
        [Tooltip("Environment atlas pixels per world unit (16px grid)")]
        public float envArtPixelsPerUnit = 16f;
    }
}
