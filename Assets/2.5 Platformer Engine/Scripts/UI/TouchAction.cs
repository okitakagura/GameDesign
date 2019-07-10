using UnityEngine;
using UnityEngine.EventSystems;

namespace Platformer
{
    /// <summary>
    /// Button to make a character perform an action.
    /// </summary>
    public class TouchAction : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        /// Character for the button to make perform actions.
        /// </summary>
        [Tooltip("Character for the button to make perform actions.")]
        public CharacterMotor Motor;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Motor != null && Motor.Action != null)
                Motor.Action.Execute(Motor.gameObject);
        }
    }
}
