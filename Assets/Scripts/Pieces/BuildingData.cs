using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingDefinition", menuName = "Game/Building Definition", order = 0)]
public class BuildingData : ScriptableObject
{
    [Header("Building Metadata")]
    public string displayName;

    public int costWater, costAlloy, costOil, costFood, costBrick;

    [Header("Visuals")]
    public GameObject prefab;
    public Sprite icon;
}