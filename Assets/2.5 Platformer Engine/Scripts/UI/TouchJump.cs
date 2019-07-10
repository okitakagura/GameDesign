using UnityEngine;
using UnityEngine.EventSystems;

namespace Platformer
{
    /// <summary>
    /// Button to make a character jump.
    /// </summary>
    public class TouchJump : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        /// Character for the button to manage jumping for.
        /// </summary>
        [Tooltip("Character for the button to manage jumping for.")]
        public CharacterMotor Motor;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Motor != null)
                Motor.InputJump();
        }
    }
}
