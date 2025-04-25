using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager I { get; private set; }

    [Header("Tilemap")]
    [Tooltip("Must be the same Tilemap used by DungeonGenerator")]
    [SerializeField] private Tilemap floorTilemap;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public List<GameObject> enemyPrefabs;
    public List<GameObject> itemPrefabs;
    public List<GameObject> propPrefabs;

    [Header("Counts (will clamp to available cells)")]
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private int itemCount = 10;
    [SerializeField] private int propCount = 8;

    private List<Vector3Int> _freeCells;

    void Awake()
    {
        if (I != null && I != this) Destroy(gameObject);
        else I = this;
    }

    /// <summary>
    /// Call this once the dungeon is generated and floorTilemap is painted.
    /// </summary>
    public void SpawnAll()
    {
        CacheFreeCells();

        if (_freeCells.Count == 0)
        {
            Debug.LogError("[SpawnerManager] No floor cells found to spawn on!");
            return;
        }

        SpawnPlayer();
        SpawnGroup(enemyPrefabs, enemyCount, "Enemies");
        SpawnGroup(itemPrefabs, itemCount, "Items");
        SpawnGroup(propPrefabs, propCount, "Props");
    }

    private void CacheFreeCells()
    {
        _freeCells = new List<Vector3Int>();
        var bounds = floorTilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(pos))
                _freeCells.Add(pos);
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        var idx = Random.Range(0, _freeCells.Count);
        var cell = _freeCells[idx];
        var pos = floorTilemap.GetCellCenterWorld(cell);

        var playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
        _freeCells.RemoveAt(idx);

        var playerCamera = Camera.main.GetComponent<PlayerCamera>();
        if (playerCamera != null)
            playerCamera.SetTarget(playerInstance.transform);
    }

    private void SpawnGroup(List<GameObject> prefabs, int count, string parentName)
    {
        if (prefabs == null || prefabs.Count == 0 || count <= 0) return;

        var parent = new GameObject(parentName).transform;
        count = Mathf.Min(count, _freeCells.Count);

        for (int i = 0; i < count; i++)
        {
            var prefab = prefabs[Random.Range(0, prefabs.Count)];
            if (prefab == null) continue;

            var idx = Random.Range(0, _freeCells.Count);
            var cell = _freeCells[idx];
            var pos = floorTilemap.GetCellCenterWorld(cell);

            Instantiate(prefab, pos, Quaternion.identity, parent);
            _freeCells.RemoveAt(idx);
        }
    }
}
