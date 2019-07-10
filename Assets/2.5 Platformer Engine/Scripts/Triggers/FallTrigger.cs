using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Makes a character to enter the fallen mode.
    /// </summary>
    public class FallTrigger : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            makeFall(other.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            makeFall(other.gameObject);
        }

        private void makeFall(GameObject other)
        {
            var parentMotor = GetComponent<CharacterMotor>();
            if (parentMotor == null && transform.parent != null)
                parentMotor = transform.parent.GetComponent<CharacterMotor>();

            if (parentMotor != null && parentMotor.IsFalling)
                return;

            if (other == gameObject ||
                (transform.parent != null && transform.parent.gameObject == other))
                return;

            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null || !motor.CanBeMadeToFall)
                return;

            motor.KnockOut();
        }
    }
}
