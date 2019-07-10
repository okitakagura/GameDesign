using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct AirSettings
    {
        /// <summary>
        /// Horizontal speed of the character in air.
        /// </summary>
        [Tooltip("Horizontal speed of the character in air.")]
        public float Speed;

        /// <summary>
        /// Horizontal acceleration when in air.
        /// </summary>
        [Tooltip("Horizontal acceleration when in air.")]
        public float Acceleration;

        /// <summary>
        /// Horizontal decceleration when in air.
        /// </summary>
        [Tooltip("Horizontal decceleration when in air.")]
        public float Decceleration;

        /// <summary>
        /// Jump strength from the ground.
        /// </summary>
        [Tooltip("Jump strength from the ground.")]
        public float GroundJumpStrength;

        /// <summary>
        /// Jump strength when in air.
        /// </summary>
        [Tooltip("Jump strength when in air.")]
        public float AirJumpStrength;

        /// <summary>
        /// Custom gravity when in air.
        /// </summary>
        [Tooltip("Custom gravity when in air.")]
        public float Gravity;

        /// <summary>
        /// Threshold to register ground.
        /// </summary>
        [Tooltip("Threshold to register ground.")]
        public float GroundThreshold;

        public static AirSettings Default()
        {
            AirSettings settings;
            settings.Speed = 4;
            settings.Acceleration = 5;
            settings.Decceleration = 5;
            settings.GroundJumpStrength = 10;
            settings.AirJumpStrength = 10;
            settings.Gravity = 20;
            settings.GroundThreshold = 0.2f;

            return settings;
        }
    }
}
