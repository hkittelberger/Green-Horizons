using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingTooltipTarget : MonoBehaviour
{
    [SerializeField] private BuildingData data;

    private bool isHovered = false;

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (IsPointerOverThisUI(mousePos))
        {
            if (!isHovered)
            {
                isHovered = true;
                BuildingTooltipManager.Instance?.Show(data, mousePos);
            }
        }
        else
        {
            if (isHovered)
            {
                isHovered = false;
                BuildingTooltipManager.Instance?.Hide();
            }
        }
    }

    private bool IsPointerOverThisUI(Vector2 pointerPos)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = pointerPos
        };

        var results = new List<RaycastResult>();
        GraphicRaycaster raycaster = GetComponentInParent<Canvas>()?.GetComponent<GraphicRaycaster>();
        if (raycaster == null) return false;

        raycaster.Raycast(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }
}
