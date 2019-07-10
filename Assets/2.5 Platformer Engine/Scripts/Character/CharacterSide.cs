using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Marks a character as having a side/team.
    /// </summary>
    public class CharacterSide : MonoBehaviour
    {
        /// <summary>
        /// ID of the side. AI will attack anyone with a different number.
        /// </summary>
        [Tooltip("ID of the side. AI will attack anyone with a different number.")]
        public int Side;
    }
}
