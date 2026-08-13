using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleBike
{
    /// <summary>
    /// Invisible full-screen touch zone behind the HUD. Holding it makes the player
    /// sprint; any UI element on top naturally blocks it.
    /// </summary>
    public class SprintTouchZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData) => GameState.SprintHeld = true;
        public void OnPointerUp(PointerEventData eventData) => GameState.SprintHeld = false;
        void OnDisable() { GameState.SprintHeld = false; }
    }
}
