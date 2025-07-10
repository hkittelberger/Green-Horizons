using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.Services.Authentication;

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
        // 1) wait for netcode client + LobbyManager to finish
        while (!NetworkManager.Singleton.IsConnectedClient
               || PlayerManager.Instance == null
               || LobbyManager.Instance == null
               || LobbyManager.Instance.GetJoinedLobby() == null)
        {
            yield return null;
        }

        // 2) grab your local clientId
        var localClientId = NetworkManager.Singleton.LocalClientId;

        // 3) wait until PlayerManager has your data
        PlayerData playerData = null;
        while ((playerData = PlayerManager.Instance.GetPlayerData(localClientId)) == null)
        {
            yield return null;
        }

        // 4) now it's safe to update everything
        ApplyPlayerData(localClientId, playerData);
    }

    private void ApplyPlayerData(ulong clientId, PlayerData playerData)
    {
        // color
        if (playerColorIndicator.TryGetComponent<Image>(out var img))
        {
            Debug.Log($"[PlayerUI] Setting color for client {clientId} to {playerData.playerColor}");
            img.color = playerData.playerColor;
        }
        else
        {
            Debug.LogWarning("[PlayerUI] No Image on playerColorIndicator!");
        }

        // name & resources
        playerNameText.text = $"Player {clientId}";
        alloyText.text =    playerData.alloy.ToString();
        brickText.text =    playerData.brick.ToString();
        foodText.text =     playerData.food.ToString();
        oilText.text =      playerData.oil.ToString();
        waterText.text =    playerData.water.ToString();
    }
}
