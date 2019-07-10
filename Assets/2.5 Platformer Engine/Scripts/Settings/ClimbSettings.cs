using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct ClimbingSettings
    {
        /// <summary>
        /// How high the character's head is above the edge when hanging.
        /// </summary>
        [Tooltip("How high the character's head is above the edge when hanging.")]
        public float EdgeHeight;

        /// <summary>
        /// Intensity of rope swinging controls.
        /// </summary>
        [Tooltip("Intensity of rope swinging controls.")]
        public float SwingSpeed;

        /// <summary>
        /// Speed of climbing up a rope.
        /// </summary>
        [Tooltip("Speed of climbing up a rope.")]
        public float RopeClimbSpeed;

        /// <summary>
        /// Strength for jumping away from a rope.
        /// </summary>
        [Tooltip("Strength for jumping away from a rope.")]
        public float RopeJumpStrength;

        /// <summary>
        /// Time in seconds after jumping away from rope to ignore rope triggers.
        /// </summary>
        [Tooltip("Time in seconds after jumping away from rope to ignore rope triggers.")]
        public float RopeIgnoreTime;

        public static ClimbingSettings Default()
        {
            ClimbingSettings settings;
            settings.EdgeHeight = 0;
            settings.SwingSpeed = 40;
            settings.RopeClimbSpeed = 2;
            settings.RopeJumpStrength = 10;
            settings.RopeIgnoreTime = 1;

            return settings;
        }
    }
}
