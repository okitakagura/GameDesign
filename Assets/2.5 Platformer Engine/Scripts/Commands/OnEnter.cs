using UnityEngine;

namespace Platformer
{
    public class OnEnter : Executable
    {
        private void OnTriggerEnter(Collider other)
        {
            Execute(other.gameObject);
        }
    }
}
