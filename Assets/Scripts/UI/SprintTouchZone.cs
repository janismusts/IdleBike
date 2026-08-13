using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleBike
{
    /// <summary>
    /// Invisible full-screen touch zone behind the HUD. Holding it makes the player
    /// sprint; any UI element on top naturally blocks it. Tracks pointers per id so
    /// multi-touch (a second finger tapping and lifting) doesn't cancel the hold.
    /// </summary>
    public class SprintTouchZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        readonly HashSet<int> _pointers = new HashSet<int>();

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointers.Add(eventData.pointerId);
            Sync();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pointers.Remove(eventData.pointerId);
            Sync();
        }

        void Sync() => GameState.SprintHeld = _pointers.Count > 0;

        void ReleaseAll()
        {
            _pointers.Clear();
            GameState.SprintHeld = false;
        }

        // Touches can be cancelled without a pointer-up (incoming call, app switch,
        // notification shade) — never leave the sprint stuck on.
        void OnDisable() => ReleaseAll();
        void OnApplicationPause(bool paused) { if (paused) ReleaseAll(); }
        void OnApplicationFocus(bool focused) { if (!focused) ReleaseAll(); }
    }
}
