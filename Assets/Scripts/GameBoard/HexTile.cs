using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class HexTile : MonoBehaviour , IClickable
{
    public TileData tileData;

    void Start()
    {
        if (tileData != null)
        {
            // Optional: Change color or visuals based on data
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = tileData.tileColor;
        }
    }

    public void OnClick()
    {
        // Debug.Log("Tile clicked: " + tileData.tileType);
        // GameManager.Instance.ClickedOnTile(tileData.tileType);
        // GameManager.Instance.ClickedOnBuild(tileData.tileType, this);
        GameManager.Instance.ClickedOnBuild(this, null);
        // HexPopupManager.Instance.ShowPopup(tileData.tileType, transform.position);
    }
}
