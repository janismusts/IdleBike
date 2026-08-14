using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleBike
{
    /// <summary>
    /// Dedicated hold-to-sprint button. Tracks pointers per id so a second finger
    /// lifting elsewhere doesn't cancel the hold; never leaves the sprint stuck on.
    /// </summary>
    public class SprintHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

        void OnDisable() => ReleaseAll();
        void OnApplicationPause(bool paused) { if (paused) ReleaseAll(); }
        void OnApplicationFocus(bool focused) { if (!focused) ReleaseAll(); }
    }
}
