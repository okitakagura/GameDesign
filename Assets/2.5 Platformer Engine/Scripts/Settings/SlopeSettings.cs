using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct SlopeSettings
    {
        /// <summary>
        /// Minimum angle to register surface as a slope.
        /// </summary>
        [Tooltip("Minimum angle to register surface as a slope.")]
        public float MinSlopeAngle;

        /// <summary>
        /// Maximum angle to register surface as a slope.
        /// </summary>
        [Tooltip("Maximum angle to register surface as a slope.")]
        public float MaxSlopeAngle;

        /// <summary>
        /// Maximum angle for a surface to be deemed walkable.
        /// </summary>
        [Tooltip("Maximum angle for a surface to be deemed walkable.")]
        public float MaxWalkAngle;

        /// <summary>
        /// Speed of sliding down a slope.
        /// </summary>
        [Tooltip("Speed of sliding down a slope.")]
        public float SlideSpeed;

        public static SlopeSettings Default()
        {
            SlopeSettings settings;
            settings.MinSlopeAngle = 25;
            settings.MaxSlopeAngle = 60;
            settings.MaxWalkAngle = 35;
            settings.SlideSpeed = 4;

            return settings;
        }
    }
}
