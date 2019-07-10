using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Describes the name and count of an inventory name.
    /// </summary>
    [Serializable]
    public struct InventoryItem
    {
        /// <summary>
        /// Name of the inventory item.
        /// </summary>
        [Tooltip("Name of the inventory item.")]
        public string Name;

        /// <summary>
        /// Amount of the item.
        /// </summary>
        [Tooltip("Amount of the item.")]
        public int Count;
    }

    public class CharacterInventory : MonoBehaviour
    {
        /// <summary>
        /// Items belonging in the inventory.
        /// </summary>
        [Tooltip("Items belonging in the inventory.")]
        public List<InventoryItem> Items = new List<InventoryItem>();

        /// <summary>
        /// Returns the number of items with the given name.
        /// </summary>
        public int Count(string name)
        {
            foreach (var stored in Items)
                if (stored.Name == name)
                    return stored.Count;

            return 0;
        }

        /// <summary>
        /// Returns true if the inventory contains at least the given number of items.
        /// </summary>
        public bool Contains(InventoryItem item)
        {
            foreach (var stored in Items)
                if (stored.Name == item.Name && stored.Count >= item.Count)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds items to the inventory. If it already exists, adds the count. Negative counts can be used in which case the item will be removed if it reaches zero.
        /// </summary>
        public void Add(InventoryItem item)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Name == item.Name)
                {
                    item.Count += Items[i].Count;

                    if (item.Count > 0)
                        Items[i] = item;
                    else
                        Items.RemoveAt(i);

                    return;
                }

            if (item.Count > 0)
                Items.Add(item);
        }

        /// <summary>
        /// Removes items. Will not add new entry if the given item count is negative, but it will increase stored count if an entry already exists.
        /// </summary>
        public void Remove(InventoryItem item)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Name == item.Name)
                {
                    item.Count = Items[i].Count - item.Count;

                    if (item.Count > 0)
                        Items[i] = item;
                    else
                        Items.RemoveAt(i);

                    return;
                }
        }
    }
}
