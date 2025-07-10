using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class LobbyUI : MonoBehaviour {


    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button changeRedButton;
    [SerializeField] private Button changeBlueButton;
    [SerializeField] private Button changeGreenButton;
    [SerializeField] private Button changeYellowButton;
    [SerializeField] private Button leaveLobbyButton;


    private void Awake() {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        changeRedButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Red);
        });
        changeBlueButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Blue);
        });
        changeGreenButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Green);
        });
        changeYellowButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Yellow);
        });

        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });
    }

    private void Start() {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
        
        LobbyManager.Instance.OnJoinedLobby         -= UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate   -= UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby           -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby     -= LobbyManager_OnLeftLobby;
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        UpdateLobby();
    }

    private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby) {
        ClearLobby();

        foreach (Player player in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }


        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        RefreshColorButtons(lobby);

        Show();
    }
    
    private void RefreshColorButtons(Lobby lobby) {
        // 1) build a set of all chosen colors
        var taken = new HashSet<LobbyManager.PlayerCharacter>();
        foreach (var p in lobby.Players) {
            if (p.Data.TryGetValue(LobbyManager.KEY_PLAYER_CHARACTER, out var data)
             && System.Enum.TryParse<LobbyManager.PlayerCharacter>(data.Value, out var c))
            {
                taken.Add(c);
            }
        }

        // 2) find what *you* have right now (so you can still re‐pick your own)
        var me = lobby.Players
            .First(x => x.Id == AuthenticationService.Instance.PlayerId)
            .Data[LobbyManager.KEY_PLAYER_CHARACTER].Value;
        Enum.TryParse(me, out LobbyManager.PlayerCharacter myColor);

        // 3) disable any button whose color is taken by someone *else*
        changeRedButton.interactable    = !taken.Contains(LobbyManager.PlayerCharacter.Red)    || myColor == LobbyManager.PlayerCharacter.Red;
        changeBlueButton.interactable   = !taken.Contains(LobbyManager.PlayerCharacter.Blue)   || myColor == LobbyManager.PlayerCharacter.Blue;
        changeGreenButton.interactable  = !taken.Contains(LobbyManager.PlayerCharacter.Green)  || myColor == LobbyManager.PlayerCharacter.Green;
        changeYellowButton.interactable = !taken.Contains(LobbyManager.PlayerCharacter.Yellow) || myColor == LobbyManager.PlayerCharacter.Yellow;
    }

    private void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}