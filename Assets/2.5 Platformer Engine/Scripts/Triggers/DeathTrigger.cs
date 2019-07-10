using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Makes a character die.
    /// </summary>
    public class DeathTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
        }
    }
}
