using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "HexTile/Tile Data", order = 1)]
public class TileData : ScriptableObject
{
    public string tileType;
    public Sprite icon;
    // Add more fields like movementCost, resourceAmount, etc. if needed
}
