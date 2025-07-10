using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour
{

     [Header("Manager Prefabs (must have NetworkObject)")]
    [SerializeField] private GameObject[] managerPrefabs;

    [Header("Name of the scene to load via NetworkManager.SceneManager")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyStartGame += LobbyManager_OnLobbyStartGame;
    }

    private void LobbyManager_OnLobbyStartGame(object sender, LobbyManager.LobbyEventArgs e)
    {
        // Debug.Log("LobbyManager_OnLobbyStartGame called");
        if (LobbyManager.IsHost)
        {
            CreateRelayAndStart();
        }
        else
        {
            JoinRelay(LobbyManager.RelayJoinCode);
        }
    }

    // only once per load
    private void OnLoadComplete(ulong clientId, string scene, LoadSceneMode mode)
    {
        if (!NetworkManager.Singleton.IsServer || scene != gameSceneName) return;
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;

        // spawn your manager prefabs on the server
        foreach (var prefab in managerPrefabs)
        {
            if (prefab == null || GameObject.Find(prefab.name) != null) continue;
            var inst = Instantiate(prefab);
            var netObj = inst.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn(true);
            else
                Debug.LogError($"'{prefab.name}' missing NetworkObject");
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }


    private async void CreateRelayAndStart()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(3);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

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
        StartHost();
        // Debug.Log("Host started successfully.");
        LobbyManager.Instance.SetRelayJoinCode(joinCode);
        
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        NetworkManager.Singleton.SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
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
        StartClient();
        // Debug.Log("Client started successfully.");
    }
}