using UnityEngine;

namespace Platformer
{
    public class OnArrive : Executable
    {
        public void OnFinishMove(GameObject target)
        {
            Execute(target);
        }
    }
}
