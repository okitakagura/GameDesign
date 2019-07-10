using UnityEngine;

namespace Platformer
{
    public static class HitPauseManager
    {
        private static float _start;
        private static float _duration;

        public static void Pause(float duration, float delay)
        {
            _start = Time.realtimeSinceStartup + delay;
            _duration = duration;
        }

        public static void Update()
        {
            var t = Time.realtimeSinceStartup - _start;

            if (t < 0 || t > _duration || _duration <= float.Epsilon)
                Time.timeScale = 1.0f;
            else
            {
                t /= _duration;
                Time.timeScale = 0;
            }
        }
    }
}
