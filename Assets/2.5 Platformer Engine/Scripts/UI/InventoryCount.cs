using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    /// <summary>
    /// Updates a Text component to the count of items stored in an inventory.
    /// </summary>
    [RequireComponent(typeof(Text))]
    [ExecuteInEditMode]
    public class InventoryCount : MonoBehaviour
    {
        /// <summary>
        /// Inventory which is searched for the item.
        /// </summary>
        [Tooltip("Inventory which is searched for the item.")]
        public CharacterInventory Inventory;

        /// <summary>
        /// Item name to look for in the inventory.
        /// </summary>
        [Tooltip("Item name to look for in the inventory.")]
        public string Item;

        /// <summary>
        /// Text to add before the number.
        /// </summary>
        [Tooltip("Text to add before the number.")]
        public string Prefix;

        /// <summary>
        /// Text to add after the number.
        /// </summary>
        [Tooltip("Text to add after the number.")]
        public string Suffix;

        private Text _text;
        private string _storedPrefix;
        private string _storedSuffix;
        private int _stored = int.MinValue;

        private void Update()
        {
            if (_text == null)
                _text = GetComponent<Text>();

            if (_text == null || Inventory == null)
                return;

            int count = Inventory.Count(Item);

            if (count != _stored || Prefix != _storedPrefix || Suffix != _storedSuffix)
            {
                _text.text = Prefix + count.ToString() + Suffix;
                _stored = count;
                _storedPrefix = Prefix;
                _storedSuffix = Suffix;
            }
        }
    }
}
