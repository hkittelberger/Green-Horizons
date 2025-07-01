using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingSelectionManager : MonoBehaviour
{
    public static BuildingSelectionManager Instance { get; private set; }

    [System.Serializable]
    public class BuildingSelectionUI
    {
        public BuildingType type;
        public Image background; // background image that turns red when selected
    }

    [Header("UI Selection Mapping")]
    [SerializeField] private List<BuildingSelectionUI> buildingOptions;

    [Header("Selection Color Settings")]
    [SerializeField] private Color selectedColor = Color.red;
    [SerializeField] private Color defaultColor = Color.gray;

    public BuildingType CurrentSelected { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Don't select anything by default
        CurrentSelected = BuildingType.None;
        DeselectAll();
    }

    public void SelectBuilding(BuildingType type)
    {
        // If already selected, deselect
        if (CurrentSelected == type)
        {
            DeselectAll();
            CurrentSelected = BuildingType.None;
            Debug.Log("[BuildingSelectionManager] Deselected all buildings");
            return;
        }

        CurrentSelected = type;

        Color playerColor = Color.red; // fallback
        if (PlayerManager.Instance != null)
        {
            var data = PlayerManager.Instance.GetPlayerData(Unity.Netcode.NetworkManager.Singleton.LocalClientId);
            if (data != null)
                playerColor = data.playerColor;
        }

        foreach (var entry in buildingOptions)
        {
            if (entry.background != null)
                entry.background.color = (entry.type == type) ? playerColor : defaultColor;
        }

        Debug.Log($"[BuildingSelectionManager] Selected building type: {type}");
    }

    public void DeselectAll()
    {
        foreach (var entry in buildingOptions)
        {
            if (entry.background != null)
                entry.background.color = defaultColor;
        }

        CurrentSelected = BuildingType.None;
    }

    // Called by UI buttons via UnityEvent in the Inspector
    public void SelectBuildingByName(string buildingTypeName)
    {
        if (System.Enum.TryParse(buildingTypeName, out BuildingType parsedType))
        {
            SelectBuilding(parsedType);
        }
        else
        {
            Debug.LogError($"[BuildingSelectionManager] Invalid building type: {buildingTypeName}");
        }
    }
}
