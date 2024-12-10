using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InventoryItem is responsible for handling items that can be collected and added to an inventory. 
/// It uses a CircleCollider2D to detect when an item is collected.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class InventoryItem : MonoBehaviour
{
    [Tooltip("The key representing this inventory item.")]
    public string inventoryKey = "";

    [Tooltip("Layers that can interact with this item.")]
    public LayerMask layers;

    [Tooltip("Disable the item upon collection.")]
    public bool disableOnEnter = false;

    [HideInInspector]
    new public CircleCollider2D collider; // The CircleCollider2D component

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void OnEnable()
    {
        collider = GetComponent<CircleCollider2D>();
    }

    /// <summary>
    /// Resets the item to its default state.
    /// </summary>
    void Reset()
    {
        layers = LayerMask.NameToLayer("Everything");
        collider = GetComponent<CircleCollider2D>();
        collider.radius = 5;
        collider.isTrigger = true;
    }

    /// <summary>
    /// Called when another object enters the trigger collider attached to this object.
    /// </summary>
    /// <param name="other">The Collider2D object that entered the trigger.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & layers) != 0)
        {
            var ic = other.GetComponent<PlayerInventoryController>();
            if (ic != null)
            {
                ic.AddItem(inventoryKey); // Add the item to the inventory
                if (disableOnEnter)
                {
                    gameObject.SetActive(false); // Disable the item if required
                }
            }
        }
    }

    /// <summary>
    /// Draws a gizmo in the editor for visual debugging.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "InventoryItem", false);
    }
}
