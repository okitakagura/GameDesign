using UnityEngine;
using UnityEngine.SceneManagement;
namespace Platformer
{
    /// <summary>
    /// Triggers a character to change their orientation. Angle is taken from the trigger's orientation around the Y axis.
    /// </summary>
    public class Turn : MonoBehaviour
    {
        public Vector3 EnterDirection
        {
            get { return Quaternion.AngleAxis(EnterAngle, Vector3.up) * Vector3.forward; }
        }

        public Vector3 ExitDirection
        {
            get { return Quaternion.AngleAxis(ExitAngle, Vector3.up) * Vector3.forward; }
        }

        public float EnterAngle
        {
            get { return Enter + transform.eulerAngles.y; }
        }

        public float ExitAngle
        {
            get { return Exit + transform.eulerAngles.y; }
        }

        public float Enter = -90;
        public float Exit = 180;

        private void OnTriggerEnter(Collider other)
        {
            SceneManager.LoadScene("win");
        }
        //private void OnTriggerExit(Collider other)
        //{
        //    var motor = other.GetComponent<CharacterMotor>();
        //    if (motor == null)
        //        return;

        //    motor.CancelNextAngle();
        //}

        //private void OnTriggerStay(Collider other)
        //{
        //    var motor = other.GetComponent<CharacterMotor>();
        //    if (motor == null)
        //        return;

        //    var vector = (motor.transform.position - transform.position).normalized;
        //    var walk = Quaternion.AngleAxis(motor.WalkAngle, Vector3.up) * Vector3.forward;

        //    if (Vector3.Dot(vector, walk) > 0.01)
        //    {
        //        if (Vector3.Dot(walk, -EnterDirection) > 0.01)
        //        {
        //            if (motor.Direction == CharacterDirection.Right)
        //                motor.InputTurn(this, ExitAngle);
        //            else
        //                motor.InputTurn(this, ExitAngle + 180);
        //        }
        //        else if (Vector3.Dot(walk, -ExitDirection) > 0.01)
        //        {
        //            if (motor.Direction == CharacterDirection.Right)
        //                motor.InputTurn(this, EnterAngle);
        //            else
        //                motor.InputTurn(this, EnterAngle + 180);
        //        }
        //    }
        //    else
        //    {
        //        if (Vector3.Dot(walk, -EnterDirection) > 0.01)
        //        {
        //            if (motor.Direction == CharacterDirection.Right)
        //                motor.InputNextAngle(ExitAngle);
        //            else
        //                motor.InputNextAngle(ExitAngle + 180);
        //        }
        //        else if (Vector3.Dot(walk, -ExitDirection) > 0.01)
        //        {
        //            if (motor.Direction == CharacterDirection.Right)
        //                motor.InputNextAngle(EnterAngle);
        //            else
        //                motor.InputNextAngle(EnterAngle + 180);
        //        }
        //    }
        //}
    }
}
