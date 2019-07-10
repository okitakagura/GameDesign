using UnityEngine;
using UnityEngine.EventSystems;

namespace Platformer
{
    /// <summary>
    /// Touchscreen button that manages character's attacks.
    /// </summary>
    public class TouchAttack : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        /// Character the button will make attack.
        /// </summary>
        [Tooltip("Character the button will make attack.")]
        public CharacterMotor Motor;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Motor != null)
                Motor.InputAttack();
        }
    }
}
