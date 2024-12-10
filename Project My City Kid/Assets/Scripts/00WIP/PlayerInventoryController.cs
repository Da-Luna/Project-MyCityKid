using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// InventoryController is responsible for managing an inventory system. It can add, remove,
/// and check for items in the inventory, as well as handle persistence of the inventory data.
/// </summary>
public class PlayerInventoryController : MonoBehaviour
{
    [System.Serializable]
    public class InventoryEvent
    {
        [Tooltip("The key representing the inventory item.")]
        public string key;

        [Tooltip("Event triggered when the item is added to the inventory.")]
        public UnityEvent OnAdd;

        [Tooltip("Event triggered when the item is removed from the inventory.")]
        public UnityEvent OnRemove;
    }

    [System.Serializable]
    public class InventoryChecker
    {
        [Tooltip("List of items to check in the inventory.")]
        public string[] inventoryItems;

        [Tooltip("Event triggered when all specified items are found in the inventory.")]
        public UnityEvent OnHasItem;

        [Tooltip("Event triggered when any specified item is not found in the inventory.")]
        public UnityEvent OnDoesNotHaveItem;

        /// <summary>
        /// Checks if all items in inventoryItems exist in the inventory.
        /// </summary>
        /// <param name="inventory">The inventory to check.</param>
        /// <returns>True if all items are present, false otherwise.</returns>
        public bool CheckInventory(PlayerInventoryController inventory)
        {
            if (inventory != null)
            {
                for (var i = 0; i < inventoryItems.Length; i++)
                {
                    if (!inventory.HasItem(inventoryItems[i]))
                    {
                        OnDoesNotHaveItem.Invoke(); // Trigger event if item is not found
                        return false;
                    }
                }

                OnHasItem.Invoke(); // Trigger event if all items are found
                return true;
            }

            return false;
        }
    }

    [Tooltip("Array of events triggered by inventory actions.")]
    public InventoryEvent[] inventoryEvents;

    [Tooltip("Event triggered when the inventory is loaded.")]
    public event System.Action OnInventoryLoaded;

    protected Dictionary<string, int> m_InventoryItems = new(); // Dictionary to store inventory items

    /// <summary>
    /// Debug function useful in editor during play mode to print in console all objects in that InventoryController.
    /// </summary>
    [ContextMenu("Dump")]
    void Dump()
    {
        foreach (var item in m_InventoryItems)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }
    }

    /// <summary>
    /// Adds an item to the inventory.
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    public void AddItem(string key)
    {
        if (!m_InventoryItems.ContainsKey(key))
        {
            m_InventoryItems[key] = 0;
        }

        m_InventoryItems[key]++;
        
        var ev = GetInventoryEvent(key);
        if (ev != null) ev.OnAdd.Invoke(); // Trigger OnAdd event if exists
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    public void RemoveItem(string key)
    {
        if (m_InventoryItems.ContainsKey(key))
        {
            var ev = GetInventoryEvent(key);
            if (ev != null) ev.OnRemove.Invoke(); // Trigger OnRemove event if exists

            m_InventoryItems[key]--;

            if (m_InventoryItems[key] <= 0)
            {
                m_InventoryItems.Remove(key);
            }
        }
    }

    /// <summary>
    /// Checks if an item exists in the inventory.
    /// </summary>
    /// <param name="key">The key of the item to check.</param>
    /// <returns>True if the item exists and its count is greater than 0, false otherwise.</returns>
    public bool HasItem(string key)
    {
        return m_InventoryItems.ContainsKey(key) && m_InventoryItems[key] > 0;
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void Clear()
    {
        m_InventoryItems.Clear();
    }

    /// <summary>
    /// Retrieves the InventoryEvent associated with a given key.
    /// </summary>
    /// <param name="key">The key of the inventory item.</param>
    /// <returns>The InventoryEvent if found, null otherwise.</returns>
    InventoryEvent GetInventoryEvent(string key)
    {
        foreach (var iv in inventoryEvents)
        {
            if (iv.key == key) return iv;
        }

        return null;
    }
}
