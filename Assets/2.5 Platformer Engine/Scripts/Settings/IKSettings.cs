using System;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Settings for a chain of bones to be manipulated by IK.
    /// </summary>
    [Serializable]
    public struct IKChain
    {
        /// <summary>
        /// Defines quality of character IK.
        /// </summary>
        [Tooltip("Defines quality of character IK.")]
        [Range(1, 10)]
        public int Iterations;

        /// <summary>
        /// Chain of bones for the IK to modify.
        /// </summary>
        [Tooltip("Chains go in direction away from spine.")]
        public IKBone[] Bones;

        /// <summary>
        /// Check if there are any bones or the iteration count is positive.
        /// </summary>
        public bool IsEmpty
        {
            get { return Bones.Length == 0 || Iterations < 1; }
        }

        /// <summary>
        /// Returns an empty chain with default set delay values.
        /// </summary>
        public static IKChain Default()
        {
            var chain = new IKChain();
            chain.Iterations = 2;

            return chain;
        }
    }

    /// <summary>
    /// Character's IK settings.
    /// </summary>
    [Serializable]
    public struct IKSettings
    {
        public IKChain LeftArmChain;
        public IKChain RightArmChain;
        public IKChain UpperBodyChain;

        public Transform LeftHand;
        public Transform RightHand;
        public Transform Head;

        /// <summary>
        /// Default IK settings.
        /// </summary>
        public static IKSettings Default()
        {
            var settings = new IKSettings();
            settings.LeftArmChain = IKChain.Default();
            settings.RightArmChain = IKChain.Default();
            settings.UpperBodyChain = IKChain.Default();

            return settings;
        }
    }

    /// <summary>
    /// Settings of a bone to be manipulated by IK.
    /// </summary>
    [Serializable]
    public struct IKBone
    {
        /// <summary>
        /// Link to the bone.
        /// </summary>
        [Tooltip("Link to the bone.")]
        public Transform Transform;

        /// <summary>
        /// Defines bone's influence in a bone chain.
        /// </summary>
        [Tooltip("Defines bone's influence in a bone chain.")]
        [Range(0, 1)]
        public float Weight;

        [HideInInspector]
        internal IKTransform Link;

        public IKBone(Transform transform, float weight)
        {
            Transform = transform;
            Weight = weight;
            Link = null;
        }
    }
}