using UnityEngine;

namespace Platformer
{
    public class OnDeath : Executable
    {
        public void Die()
        {
            Execute(null);
        }
    }
}
