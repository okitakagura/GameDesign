using UnityEngine;

namespace Platformer
{
    public class OnLeave : Executable
    {
        private void OnTriggerExit(Collider other)
        {
            Execute(other.gameObject);
        }
    }
}
