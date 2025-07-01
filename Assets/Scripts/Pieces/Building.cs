using UnityEngine;
using Unity.Netcode;

public class Building : NetworkBehaviour
{
    [Header("Definition")]
    public BuildingData definition;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private Color ownerColor;
    private ulong ownerClientId;

    public void Initialize(ulong clientId, Color color)
    {
        ownerClientId = clientId;
        ownerColor = color;
        ApplyColor();
        ApplySprite();
    }

    private void ApplyColor()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = ownerColor;
        else
            Debug.LogWarning("No SpriteRenderer found on building.");
    }

    private void ApplySprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (definition != null && definition.icon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = definition.icon;
        }
        else
        {
            Debug.LogWarning($"[Building] Could not set sprite. Definition or icon is missing on {gameObject.name}");
        }
    }

    [ClientRpc]
    public void SetColorClientRpc(float r, float g, float b, float a)
    {
        ownerColor = new Color(r, g, b, a);
        ApplyColor();
        ApplySprite();
    }

    public ulong GetOwnerId() => ownerClientId;
    public Color GetOwnerColor() => ownerColor;
    public string GetDisplayName() => definition?.displayName ?? "Unnamed";
}
