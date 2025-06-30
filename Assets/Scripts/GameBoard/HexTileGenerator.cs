using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HexTileGenerator : NetworkBehaviour
{
    [System.Serializable]
    public class TileType
    {
        public string name;
        public GameObject TilePrefab;
        public int count;
    }

    [Header("Hex Settings")]
    public int radius = 4;
    public float hexWidth = 3f;      // Flat-top width
    public float hexHeight = 2.598f; // height = width * sqrt(3)/2

    [Header("Tile Prefabs")]
    public GameObject waterTile; // Center tile
    public List<TileType> tileTypes;

    private bool hasGenerated = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer && !hasGenerated)
        {
            GenerateBoard();
            hasGenerated = true;
        }
    }

    private void GenerateBoard()
    {
        List<Vector3Int> hexPositions = GetHexCoordinates(radius);
        List<GameObject> tilePool = BuildShuffledTilePool();

        foreach (Vector3Int cube in hexPositions)
        {
            Vector3 worldPos = CubeToWorld(cube);
            GameObject tileToSpawn;

            if (cube == Vector3Int.zero && waterTile != null)
            {
                tileToSpawn = waterTile;
            }
            else
            {
                if (tilePool.Count == 0)
                {
                    Debug.LogWarning("Tile pool ran out of prefabs.");
                    continue;
                }

                tileToSpawn = tilePool[0];
                tilePool.RemoveAt(0);
            }

            GameObject tileGO = Instantiate(tileToSpawn, worldPos, Quaternion.identity);

            if (tileGO.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn(true);
            }
            else
            {
                Debug.LogError($"{tileGO.name} is missing a NetworkObject component.");
            }

            string cleanedName = tileToSpawn.name.Replace("(Clone)", "").Trim();
            tileGO.name = $"{cleanedName}_{cube.x}_{cube.y}_{cube.z}";
            tileGO.transform.SetParent(this.transform);
        }
    }

    private List<Vector3Int> GetHexCoordinates(int radius)
    {
        List<Vector3Int> hexes = new List<Vector3Int>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = Mathf.Max(-radius, -x - radius); y <= Mathf.Min(radius, -x + radius); y++)
            {
                int z = -x - y;
                hexes.Add(new Vector3Int(x, y, z));
            }
        }

        return hexes;
    }

    private List<GameObject> BuildShuffledTilePool()
    {
        List<GameObject> pool = new();

        foreach (var type in tileTypes)
        {
            if (type.TilePrefab == null)
            {
                Debug.LogWarning($"Missing prefab for tile type: {type.name}");
                continue;
            }

            for (int i = 0; i < type.count; i++)
            {
                pool.Add(type.TilePrefab);
            }
        }

        Shuffle(pool);
        return pool;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private Vector3 CubeToWorld(Vector3Int cube)
    {
        float x = hexWidth * 0.75f * cube.x;
        float y = hexHeight * (cube.z + 0.5f * cube.x);
        return new Vector3(x, y, 0f);
    }
}
