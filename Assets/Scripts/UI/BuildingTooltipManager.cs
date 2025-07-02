using UnityEngine;
using TMPro;

public class BuildingTooltipManager : MonoBehaviour
{
    public static BuildingTooltipManager Instance { get; private set; }

    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private RectTransform tooltipAnchor;
    [SerializeField] private RectTransform tooltipRootRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Hide();
    }

    public void Show(BuildingData data, Vector2 ignoredPointer)
    {
        tooltipRoot.gameObject.SetActive(true);

        // Place tooltip at anchor's anchored position
        // tooltipRootRect.anchorMin = tooltipAnchor.anchorMin;
        // tooltipRootRect.anchorMax = tooltipAnchor.anchorMax;
        // tooltipRootRect.pivot = tooltipAnchor.pivot;
        // tooltipRootRect.anchoredPosition = tooltipAnchor.anchoredPosition;

        nameText.text = data.displayName;
        costText.text = $"Water: {data.costWater}\nAlloy: {data.costAlloy}\nOil: {data.costOil}\nFood: {data.costFood}\nBrick: {data.costBrick}";
    }

    public void Hide()
    {
        tooltipRoot.SetActive(false);
    }
}
