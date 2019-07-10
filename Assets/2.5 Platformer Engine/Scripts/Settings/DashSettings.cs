using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct DashSettings
    {
        /// <summary>
        /// Speed in meters per second when dashing.
        /// </summary>
        [Tooltip("Speed in meters per second when dashing.")]
        public float Speed;

        /// <summary>
        /// Force applied to objects that are touched when dashing.
        /// </summary>
        [Tooltip("Force applied to objects that are touched when dashing.")]
        public Vector2 Push;

        /// <summary>
        /// Time in seconds to wait after a dash to turn the gravity back on.
        /// </summary>
        [Tooltip("Time in seconds to wait after a dash to turn the gravity back on.")]
        public float NoGravityTime;

        public static DashSettings Default()
        {
            DashSettings settings;
            settings.Speed = 15;
            settings.Push = new Vector2(200, 200);
            settings.NoGravityTime = 0.35f;

            return settings;
        }
    }
}
