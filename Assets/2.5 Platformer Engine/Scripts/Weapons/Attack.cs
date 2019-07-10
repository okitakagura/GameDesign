    using System;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Describes a single attack.
    /// </summary>
    [Serializable]
    public struct Attack
    {
        /// <summary>
        /// Expected animation state for the character using the attack.
        /// </summary>
        [Tooltip("Expected animation state for the character using the attack.")]
        public string AnimationState;

        /// <summary>
        /// Alternate animation state for the character using the attack.
        /// </summary>
        [Tooltip("Alternate animation state for the character using the attack.")]
        public string AlternateAnimationState;

        /// <summary>
        /// Fraction of the attack animation for when to turn on the attack.
        /// </summary>
        [Tooltip("Fraction of the attack animation for when to turn on the attack.")]
        public float Start;

        /// <summary>
        /// Delay as a fraction of animation duration to activate the hit detection mode after the start of an attack.
        /// </summary>
        [Tooltip("Delay as a fraction of animation duration to activate the hit detection mode after the start of an attack.")]
        public float HitDelay;

        /// <summary>
        /// Duration of an attack as a fraction of animation duration.
        /// </summary>
        [Tooltip("Duration of an attack as a fraction of animation duration.")]
        public float Duration;

        /// <summary>
        /// Damage to deal to a target.
        /// </summary>
        [Tooltip("Damage to deal to a target.")]
        public float Damage;

        /// <summary>
        /// Should the attack ignore collisions with characters behind the attacker's back.
        /// </summary>
        [Tooltip("Should the attack ignore collisions with characters behind the attacker's back.")]
        public bool IgnoreBehind;

        /// <summary>
        /// Distance for the owner to dash forward. Value is ignored if close to zero.
        /// </summary>
        [Tooltip("Distance for the owner to dash forward. Value is ignored if close to zero.")]
        public float Dash;

        /// <summary>
        /// Does the dash causes enemies to fall.
        /// </summary>
        [Tooltip("Does the dash causes enemies to fall.")]
        public bool IsStrongDash;

        /// <summary>
        /// Duration in seconds of the pause after a successful attack
        /// </summary>
        [Tooltip("Duration in seconds of the pause after a successful attack")]
        public float HitPause;

        /// <summary>
        /// Time in seconds to wait before applying the pause.
        /// </summary>
        [Tooltip("Time in seconds to wait before applying the pause.")]
        public float HitPauseDelay;

        /// <summary>
        /// Material to use for this attack. If set to none the default trail material will be used.
        /// </summary>
        [Tooltip("Material to use for this attack. If set to none the default trail material will be used.")]
        public Material TrailMaterialOverride;

        /// <summary>
        /// Creates a default attack description.
        /// </summary>
        public static Attack Default()
        {
            Attack attack;
            attack.AnimationState = "Attack";
            attack.AlternateAnimationState = null;
            attack.Start = 0.35f;
            attack.HitDelay = 0.0f;
            attack.Duration = 0.6f;
            attack.Damage = 50;
            attack.IgnoreBehind = true;
            attack.IsStrongDash = false;
            attack.HitPause = 0.1f;
            attack.HitPauseDelay = 0.05f;
            attack.TrailMaterialOverride = null;
            attack.Dash = 0;

            return attack;
        }
    }
}
