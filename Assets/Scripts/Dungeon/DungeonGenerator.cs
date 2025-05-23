using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    public Tilemap FloorTilemap => floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap wallsBackTilemap;
    [SerializeField] private Tilemap wallsFrontTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase[] floorTiles;
    [SerializeField] private RuleTile wallRuleTile;
    [SerializeField] private RuleTile wallBaseRuleTile;
    [SerializeField] private RuleTile wallTopRuleTile;

    [Header("Generation Settings")]
    [SerializeField] private int _mapWidth = 50;
    [SerializeField] private int _mapHeight = 50;
    [SerializeField] private int _minRoomSize = 6;
    [SerializeField] private int _maxRoomSize = 12;
    [SerializeField] private int _maxRooms = 8;
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private int _roomSpacing = 2; // minimum tiles between rooms

    private bool[,] _floorGrid;
    private List<Room> _rooms = new List<Room>();
    private List<Vector3Int> wallPositions = new List<Vector3Int>();

    private class Room
    {
        public RectInt Bounds;
        public Vector2Int Center => new Vector2Int(Bounds.x + Bounds.width / 2, Bounds.y + Bounds.height / 2);
        public Room(RectInt bounds) { Bounds = bounds; }
    }

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        ClearMaps();
        InitializeGrid();
        GenerateRooms();
        ConnectRooms();
        PaintFloors();
        GatherWallPositions();
        PaintWalls();

        if (SpawnerManager.I != null)
            SpawnerManager.I.SpawnAll();
    }

    public void ClearMaps()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    private void InitializeGrid()
    {
        _floorGrid = new bool[_mapWidth, _mapHeight];
        _rooms.Clear();
    }

    private void GenerateRooms()
    {
        int attempts = 0;
        int spacing = _roomSpacing;
        while (_rooms.Count < _maxRooms && attempts < _maxRooms * 5)
        {
            int w = Random.Range(_minRoomSize, _maxRoomSize + 1);
            int h = Random.Range(_minRoomSize, _maxRoomSize + 1);
            int maxX = Mathf.Max(1, _mapWidth - w - 1);
            int maxY = Mathf.Max(1, _mapHeight - h - 1);
            int x = Random.Range(1, maxX);
            int y = Random.Range(1, maxY);
            RectInt newRoom = new RectInt(x, y, w, h);
            bool overlaps = false;
            foreach (var r in _rooms)
            {
                // expand existing room by spacing to enforce minimum gap between rooms
                RectInt expanded = new RectInt(
                    r.Bounds.x - spacing,
                    r.Bounds.y - spacing,
                    r.Bounds.width + spacing * 2,
                    r.Bounds.height + spacing * 2
                );
                if (expanded.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }
            if (!overlaps)
            {
                _rooms.Add(new Room(newRoom));
                for (int i = x; i < x + w && i < _mapWidth; i++)
                    for (int j = y; j < y + h && j < _mapHeight; j++)
                        _floorGrid[i, j] = true;
            }
            attempts++;
        }
    }

    private void ConnectRooms()
    {
        for (int i = 1; i < _rooms.Count; i++)
        {
            Vector2Int a = _rooms[i - 1].Center;
            Vector2Int b = _rooms[i].Center;
            if (Random.value < 0.5f)
            {
                CreateHorizontalTunnel(a.x, b.x, a.y);
                CreateVerticalTunnel(a.y, b.y, b.x);
            }
            else
            {
                CreateVerticalTunnel(a.y, b.y, a.x);
                CreateHorizontalTunnel(a.x, b.x, b.y);
            }
        }
    }

    /// <summary>
    /// Creates a horizontal corridor at least 3 tiles tall, centered on the given y coordinate.
    /// </summary>
    private void CreateHorizontalTunnel(int xStart, int xEnd, int y)
    {
        int yMin = Mathf.Max(0, y - 1);
        int yMax = Mathf.Min(_mapHeight - 1, y + 1);
        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            if (x < 0 || x >= _mapWidth) continue;
            for (int ty = yMin; ty <= yMax; ty++)
            {
                _floorGrid[x, ty] = true;
            }
        }
    }

    /// <summary>
    /// Creates a vertical corridor at least 3 tiles wide, centered on the given x coordinate.
    /// </summary>
    private void CreateVerticalTunnel(int yStart, int yEnd, int x)
    {
        int xMin = Mathf.Max(0, x - 1);
        int xMax = Mathf.Min(_mapWidth - 1, x + 1);
        for (int y = Mathf.Min(yStart, yEnd); y <= Mathf.Max(yStart, yEnd); y++)
        {
            if (y < 0 || y >= _mapHeight) continue;
            for (int tx = xMin; tx <= xMax; tx++)
            {
                _floorGrid[tx, y] = true;
            }
        }
    }

    private void GatherWallPositions()
    {
        wallPositions.Clear();

        // cardinal offsets
        Vector2Int[] cardinal = {
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int( 0,-1),
        };

        // diagonal offsets
        Vector2Int[] diagonal = {
            new Vector2Int( 1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int( 1,-1),
            new Vector2Int(-1,-1),
        };

        for (int x = 0; x < _mapWidth; x++)
            for (int y = 0; y < _mapHeight; y++)
            {
                if (!_floorGrid[x, y])
                    continue;

                // 1) Cardinal neighbours
                foreach (var off in cardinal)
                {
                    int nx = x + off.x, ny = y + off.y;
                    if (nx < 0 || nx >= _mapWidth || ny < 0 || ny >= _mapHeight)
                        continue;
                    if (!_floorGrid[nx, ny])
                    {
                        var wallPos = new Vector3Int(nx, ny, 0);
                        if (!wallPositions.Contains(wallPos))
                            wallPositions.Add(wallPos);
                    }
                }

                // 2) Inside-corner (diagonal) neighbours
                foreach (var d in diagonal)
                {
                    int nx = x + d.x, ny = y + d.y;
                    if (nx < 0 || nx >= _mapWidth || ny < 0 || ny >= _mapHeight)
                        continue;

                    // the two orthogonally adjacent cells
                    bool adj1 = _floorGrid[x + d.x, y];
                    bool adj2 = _floorGrid[x, y + d.y];

                    // if both adjacents are floor, but diag is empty → corner
                    if (adj1 && adj2 && !_floorGrid[nx, ny])
                    {
                        var wallPos = new Vector3Int(nx, ny, 0);
                        if (!wallPositions.Contains(wallPos))
                            wallPositions.Add(wallPos);
                    }
                }
            }
    }

    /// <summary>
    /// Previously this built walls manually; now uses a RuleTile for automatic sprite & collider.
    /// </summary>
    private void PaintWalls()
    {
        // If your RuleTiles need floor context:
        if (wallBaseRuleTile is FloorAwareIsometricRuleTile fa) fa.floorTilemap = floorTilemap;
        if (wallTopRuleTile is FloorAwareIsometricRuleTile fa2) fa2.floorTilemap = floorTilemap;

        wallsBackTilemap.ClearAllTiles();
        wallsFrontTilemap.ClearAllTiles();

        foreach (Vector3Int cell in wallPositions)
        {
            // paint bottom-half on back map
            wallsBackTilemap.SetTile(cell, wallBaseRuleTile);

            // paint overhang on front map
            wallsFrontTilemap.SetTile(cell, wallTopRuleTile);
        }

        wallsBackTilemap.RefreshAllTiles();
        wallsFrontTilemap.RefreshAllTiles();
    }

    private void PaintFloors()
    {
        if (floorTiles == null || floorTiles.Length == 0)
            return;

        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                if (!_floorGrid[x, y])
                    continue;

                int idx = Random.Range(0, floorTiles.Length);
                var chosen = floorTiles[idx];
                floorTilemap.SetTile(new Vector3Int(x, y, 0), chosen);
            }
        }
    }
}