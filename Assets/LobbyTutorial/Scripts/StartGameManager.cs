using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour
{



    private void Start()
    {
        LobbyManager.Instance.OnLobbyStartGame += LobbyManager_OnLobbyStartGame;
    }

    private void LobbyManager_OnLobbyStartGame(object sender, LobbyManager.LobbyEventArgs e)
    {
        Debug.Log("LobbyManager_OnLobbyStartGame called");
        // Start Game!
        if (LobbyManager.IsHost)
        {
            CreateRelay();
        }
        else
        {
            JoinRelay(LobbyManager.RelayJoinCode);
        }
        SceneManager.LoadScene(1);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }


    private async void CreateRelay()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(3);
        var joinCode   = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
        t.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        // Debug.Log($"Relay Join Code: {joinCode}");
        // Debug.Log("Starting Host with Relay...");
        NetworkManager.Singleton.StartHost();
        // Debug.Log("Host started successfully.");
        LobbyManager.Instance.SetRelayJoinCode(joinCode);
    }

    private async void JoinRelay(string joinCode)
    {
        var joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
        t.SetRelayServerData(
            joinAlloc.RelayServer.IpV4,
            (ushort)joinAlloc.RelayServer.Port,
            joinAlloc.AllocationIdBytes,
            joinAlloc.Key,
            joinAlloc.ConnectionData,
            joinAlloc.HostConnectionData
        );

        // Debug.Log("Joining Relay with Join Code: " + joinCode);
        // Debug.Log("Starting Client with Relay...");
        NetworkManager.Singleton.StartClient();
        // Debug.Log("Client started successfully.");
    }
}