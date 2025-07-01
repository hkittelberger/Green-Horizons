using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// [System.Serializable]
// public class TileData
// {
//     public string tileName;
//     public BuildingType allowedBuilding; // optional
//     public bool isBuildable;
// }

public class HexTile : MonoBehaviour, IClickable
{
    public TileData tileData;

    private void Start()
    {
        // if (tileData != null)
        // {
        //     SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //     if (sr != null)
        //     {
        //         sr.color = tileData.tileColor;
        //     }
        // }
    }

    public void OnClick()
    {
        // For now, we hardcode the building type to City
        GameManager.Instance.ClickedOnBuild(this, BuildingSelectionManager.Instance.CurrentSelected);

        // Future expansion: use tileData to determine allowed building types
        // Example:
        // if (tileData.allowedBuilding == BuildingType.PowerPlant)
        //     GameManager.Instance.ClickedOnBuild(this, tileData.allowedBuilding);
    }
}
