using UnityEngine;

namespace Platformer
{
    public class OnAction : Executable
    {
        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();

            if (motor != null)
                motor.EnterAction(this);
        }

        private void OnTriggerExit(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();

            if (motor != null)
                motor.LeaveAction(this);
        }
    }
}
