using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleBike
{
    /// <summary>
    /// Invisible full-screen zone behind the HUD: dragging up/down steers the player's
    /// lane position on the road. Sprinting has its own button.
    /// </summary>
    public class SteerTouchZone : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            var cam = CameraRig.Main;
            if (cam == null || Screen.height <= 0) return;
            // screen px -> world units at the current zoom
            float worldPerPx = 2f * cam.orthographicSize / Screen.height;
            float delta = eventData.delta.y * worldPerPx * Tuning.Balance.laneDragSensitivity;
            GameState.PlayerLaneTarget = Lanes.Clamp(GameState.PlayerLaneTarget + delta);
        }
    }
}
