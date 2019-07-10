using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct WallSettings
    {
        /// <summary>
        /// Strength of jumping from a wall.
        /// </summary>
        [Tooltip("Strength of jumping from a wall.")]
        public float JumpStrength;

        /// <summary>
        /// Duration in seconds for user movement input to be ignored after a wall jump.
        /// </summary>
        [Tooltip("Duration in seconds for user movement input to be ignored after a wall jump.")]
        public float JumpTime;

        /// <summary>
        /// Custom gravity when sliding down a wall.
        /// </summary>
        [Tooltip("Custom gravity when sliding down a wall.")]
        public float Gravity;

        /// <summary>
        /// Distance to a wall for it to be registered.
        /// </summary>
        [Tooltip("Distance to a wall for it to be registered.")]
        public float Threshold;

        /// <summary>
        /// Minimum angle of a surface to be considered a wall.
        /// </summary>
        [Tooltip("Minimum angle of a surface to be considered a wall.")]
        public float MinAngle;

        public static WallSettings Default()
        {
            WallSettings settings;
            settings.JumpStrength = 5;
            settings.JumpTime = 0.5f;
            settings.Gravity = 5;
            settings.Threshold = 0.05f;
            settings.MinAngle = 80;

            return settings;
        }
    }
}
