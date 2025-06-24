using UnityEngine;
using System;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform hexRenderPrefab;

    public event EventHandler<OnClickedOnBuildEventArgs> OnClickedOnBuild;
    public class OnClickedOnBuildEventArgs : EventArgs
    {
        /* public string BuildType { get; private set; }
        public HexTile Tile { get; private set; } */

        public Transform BuildType;
        public HexTile Tile;

        /* public OnClickedOnBuild(string buildType, HexTile tile)
        {
            BuildType = buildType;
            Tile = tile;
        } */
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Initialize the game board or any other setup
        CreateBoard();
    }

    public void CreateBoard()
    {
        // Logic to create the game board
        Debug.Log("Creating game board...");

        Vector3 origin = new Vector3(0, 0, 0);
        Transform hexRender = Instantiate(hexRenderPrefab, origin, Quaternion.identity);

        // Only spawn network object if it has NetworkObject component
        NetworkObject networkObj = hexRender.GetComponent<NetworkObject>();
        if (networkObj != null)
        {
            networkObj.Spawn(true);
        }
    }

    public void ClickedOnTile(string tileType)
    {
        // Logic to handle tile click
        Debug.Log("Tile clicked: " + tileType);
        /* OnClickedOnBuild?.Invoke(this, new OnClickedOnBuildEventArgs
        {
            BuildType = "city",
            Tile = tileType
        }); */
    }

    public void ClickedOnBuild(HexTile tileType, Transform buildType)
    {
        // Logic to handle tile click
        Debug.Log("Tile clicked: " + tileType);
        OnClickedOnBuild?.Invoke(this, new OnClickedOnBuildEventArgs
        {
            BuildType = buildType,
            Tile = tileType
        });
    }


}