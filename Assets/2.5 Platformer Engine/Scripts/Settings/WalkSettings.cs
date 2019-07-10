using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct WalkSettings
    {
        /// <summary>
        /// Speed of walking on an even ground.
        /// </summary>
        [Tooltip("Speed of walking on an even ground.")]
        public float Speed;

        /// <summary>
        /// Acceleration of walking.
        /// </summary>
        [Tooltip("Acceleration of walking.")]
        public float Acceleration;

        /// <summary>
        /// Decceleration of walking.
        /// </summary>
        [Tooltip("Decceleration of walking.")]
        public float Decceleration;

        /// <summary>
        /// Speed of changing directions.
        /// </summary>
        [Tooltip("Speed of changing directions.")]
        public float TurnSpeed;

        /// <summary>
        /// Distance to an obstacle for it to be registered.
        /// </summary>
        [Tooltip("Distance to an obstacle for it to be registered.")]
        public float ObstacleThreshold;

        /// <summary>
        /// Distance to a pushable object for it to be registered.
        /// </summary>
        [Tooltip("Distance to a pushable object for it to be registered.")]
        public float PushThreshold;

        public static WalkSettings Default()
        {
            WalkSettings settings;
            settings.Speed = 5;
            settings.Acceleration = 10;
            settings.Decceleration = 20;
            settings.TurnSpeed = 7;
            settings.ObstacleThreshold = 0.1f;
            settings.PushThreshold = 0.05f;

            return settings;
        }
    }
}
