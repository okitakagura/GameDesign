using System;
using UnityEngine;

namespace Platformer
{
    [Serializable]
    public struct CameraBorders
    {
        /// <summary>
        /// Left screen offset as a fraction of screen width.
        /// </summary>
        [Tooltip("")]
        public float Left;

        /// <summary>
        /// Right screen offset as a fraction of screen width.
        /// </summary>
        [Tooltip("")]
        public float Right;

        /// <summary>
        /// Top screen offset as a fraction of screen height.
        /// </summary>
        [Tooltip("")]
        public float Top;

        /// <summary>
        /// Bottom screen offset as a fraction of screen height.
        /// </summary>
        [Tooltip("")]
        public float Bottom;

        public CameraBorders(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}
