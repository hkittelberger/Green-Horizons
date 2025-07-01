using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI alloyText;
    [SerializeField] private TextMeshProUGUI brickText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI oilText;
    [SerializeField] private TextMeshProUGUI waterText;
    [SerializeField] private GameObject playerColorIndicator;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void Start()
    {
        StartCoroutine(InitUIWhenReady());
    }

    private IEnumerator InitUIWhenReady()
    {
        // Wait for network to connect and player data to be initialized
        while (!NetworkManager.Singleton.IsConnectedClient || PlayerManager.Instance == null)
            yield return null;

        var localClientId = NetworkManager.Singleton.LocalClientId;
        var playerData = PlayerManager.Instance.GetPlayerData(localClientId);

        while (playerData == null)
        {
            yield return null;
            playerData = PlayerManager.Instance.GetPlayerData(localClientId);
        }

        Debug.Log($"[PlayerUI] Initializing UI for client {localClientId} with color {playerData.playerColor}");
        UpdatePlayerUI(localClientId, playerData);
    }

    private void UpdatePlayerUI(ulong clientId, PlayerData playerData)
    {
        if (playerColorIndicator.TryGetComponent(out Image img))
            img.color = playerData.playerColor;
        else
            Debug.LogWarning("[PlayerUI] playerColorIndicator missing Image component.");

        playerNameText.text = $"Player {clientId}";
        alloyText.text = playerData.alloy.ToString();
        brickText.text = playerData.brick.ToString();
        foodText.text = playerData.food.ToString();
        oilText.text = playerData.oil.ToString();
        waterText.text = playerData.water.ToString();
    }
}
