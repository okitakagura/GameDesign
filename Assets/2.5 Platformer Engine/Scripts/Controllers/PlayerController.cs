using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Manages a character. Allows keyboard input.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Object to activate and deactivate depending if there is an action to perform by the motor.
        /// </summary>
        [Tooltip("Object to activate and deactivate depending if there is an action to perform by the motor.")]
        public GameObject ActionUI;

        internal bool HasMovement;
        internal Vector2 Movement;

        private void Update()
        {
            Characters.MainPlayer = Characters.Get(gameObject);

            var motor = GetComponent<CharacterMotor>();
            if (motor == null)
                return;

            if (ActionUI != null)
                if (ActionUI.activeSelf != (motor.Action != null))
                    ActionUI.SetActive(motor.Action != null);

            if (motor.IsFalling)
                motor.StandUp();
            else
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && !motor.IsHangingOnEdge)
                    motor.InputAttack();

                if (Input.GetKey(KeyCode.UpArrow) || (HasMovement && Movement.y > 0.05f))
                    motor.InputClimb(1);

                if (Input.GetKey(KeyCode.DownArrow) || (HasMovement && Movement.y < -0.05f))
                    motor.InputClimb(-1);

                if (Input.GetKey(KeyCode.LeftArrow) || (HasMovement && Movement.x < -0.05f))
                {
                    motor.Direction = CharacterDirection.Left;
                    motor.InputMovement(-1);
                    if (!motor.IsHangingOnRope)
                        if (motor.IsOnWalkableSurface || !motor.IsGrounded)
                            motor.Direction = CharacterDirection.Left;
                }

                if (Input.GetKey(KeyCode.RightArrow) || (HasMovement && Movement.x > 0.05f))
                {
                    motor.Direction = CharacterDirection.Right;
                    motor.InputMovement(1);

                    if (!motor.IsHangingOnRope)
                        if (motor.IsOnWalkableSurface || !motor.IsGrounded)
                            motor.Direction = CharacterDirection.Right;
                }
                if (Input.GetKey(KeyCode.DownArrow) || (HasMovement && Movement.x > 0.05f))
                {
                    motor.Direction = CharacterDirection.Inside;
                    motor.InputMovement(2);

                    //if (!motor.IsHangingOnRope)
                        //if (motor.IsOnWalkableSurface || !motor.IsGrounded)
                            //motor.Direction = CharacterDirection.Left;
                }
                if (Input.GetKey(KeyCode.UpArrow) || (HasMovement && Movement.x < -0.05f))
                {
                    motor.Direction = CharacterDirection.Outside;
                    motor.InputMovement(-2);

                    //if (!motor.IsHangingOnRope)
                        //if (motor.IsOnWalkableSurface || !motor.IsGrounded)
                            //motor.Direction = CharacterDirection.Right;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                    motor.InputJump();
            }
        }
    }
}
