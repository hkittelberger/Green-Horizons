using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    private int index = 0;

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("NetworkManager.Singleton is null in PlayerSpawner!");
            return;
        }

        Debug.Log("PlayerSpawner registered to client connect event.");
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {

        // Debug.Log($"HandleClientConnected called on {clientId}. IsServer: {NetworkManager.Singleton.IsServer}");

        // if (!NetworkManager.Singleton.IsServer)
        //     return;

        // Debug.Log("Spawning player for client: " + clientId);

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete && NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"Scene loaded: {sceneEvent.SceneName}. Spawning player for client {sceneEvent.ClientId}");

            Transform spawnPoint = spawnPoints[index % spawnPoints.Length];
            index++;

            GameObject playerInstance = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab, spawnPoint.position, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(sceneEvent.ClientId);
        }
    }
}
