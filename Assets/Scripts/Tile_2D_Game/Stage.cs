using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject TilePrefab;
    public Sprite[] fogSprites;

    private GameObject[] _tileObjects;
    private GameObject[] _fogObjects;

    public int MapWidth = 20;
    public int MapHeight = 20;


    [Range(0f, 0.9f)] public float ErodePercent = 0.5f;
    [Min(2)] public int ErodeIterations = 2;
    [Range(0f, 0.1f)] public float LakePercent = 0.2f;
    [Range(0f, 0.3f)] public float TreePercent = 0.2f;
    [Range(0f, 0.2f)] public float HillPercent = 0.2f;
    [Range(0f, 0.1f)] public float MountainPercent = 0.2f;
    [Range(0f, 0.1f)] public float TownPercent = 0.2f;
    [Range(0f, 0.1f)] public float MonsterPercent = 0.2f;

    public Vector2 TileSize = new(16, 16);
    public Sprite[] IslandSprites;
    private int prevTileId;

    private Map _map;

    public Map Map => _map;

    public PlayerMovement playerPrefab;
    private PlayerMovement player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("space");
            ResetStage();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 screenPos = Input.mousePosition;
            Debug.Log(ScreenPosToTileId(screenPos));
            Debug.Log(GetTilePos(ScreenPosToTileId(screenPos)));
        }

        if (_tileObjects != null)
        {
            int currentTileId = ScreenPosToTileId(Input.mousePosition);
            if (prevTileId != currentTileId && currentTileId != -1)
            {
                _tileObjects[currentTileId].GetComponent<SpriteRenderer>().color = Color.green;
                if (prevTileId >= 0 && prevTileId < _tileObjects.Length)
                {
                    _tileObjects[prevTileId].GetComponent<SpriteRenderer>().color = Color.white;
                }
                prevTileId = currentTileId;
            }
        }
    }

    public void UpdateFog(int playerTileId, int visibleRange)
    {
        // 1. 플레이어 주변 범위 안의 타일을 방문 처리
        for (int i = 0; i < _map.Tiles.Length; i++)
        {
            if (GetTileDistance(playerTileId, i) <= visibleRange)
            {
                _map.Tiles[i].IsVisited = true;
            }
        }

        // 2. 방문한 타일은 안개 끄고, 방문 안 한 타일은 안개 켜기
        for (int i = 0; i < _fogObjects.Length; i++)
        {
            if (_map.Tiles[i].IsVisited)
            {
                _fogObjects[i].SetActive(false);
            }
            else
            {
                _fogObjects[i].SetActive(true);

                int fogSpriteId = GetFogSpriteId(i);
                _fogObjects[i].GetComponent<SpriteRenderer>().sprite = fogSprites[fogSpriteId];
            }
        }
    }
    private int GetFogSpriteId(int tileId)
    {
        var tile = _map.Tiles[tileId];
        int id = 0;

        // 위쪽도 안개면 Top 비트 추가
        if (tile.Adjacents[(int)Sides.Top] != null &&
            tile.Adjacents[(int)Sides.Top].IsVisited == false)
        {
            id |= 1 << (int)Sides.Top;
        }

        // 왼쪽도 안개면 Left 비트 추가
        if (tile.Adjacents[(int)Sides.Left] != null &&
            tile.Adjacents[(int)Sides.Left].IsVisited == false)
        {
            id |= 1 << (int)Sides.Left;
        }

        // 오른쪽도 안개면 Right 비트 추가
        if (tile.Adjacents[(int)Sides.Right] != null &&
            tile.Adjacents[(int)Sides.Right].IsVisited == false)
        {
            id |= 1 << (int)Sides.Right;
        }

        // 아래쪽도 안개면 Bottom 비트 추가
        if (tile.Adjacents[(int)Sides.Bottom] != null &&
            tile.Adjacents[(int)Sides.Bottom].IsVisited == false)
        {
            id |= 1 << (int)Sides.Bottom;
        }

        if (id == 0)
            return 15;

        return id;
    }


    private int GetTileDistance(int tileIdA, int tileIdB)
    {
        int rowA = tileIdA / MapWidth, colA = tileIdA % MapWidth;
        int rowB = tileIdB / MapWidth, colB = tileIdB % MapWidth;
        return Mathf.Max(Mathf.Abs(rowA - rowB), Mathf.Abs(colA - colB));
    }

    private void ResetStage()
    {
        Debug.Log("resetstage");
        _map = new();
        _map.Init(MapHeight, MapWidth);
        _map.CreateIsLand(
            ErodePercent,
            ErodeIterations,
            LakePercent,
            TreePercent,
            HillPercent,
            MountainPercent,
            TownPercent,
            MonsterPercent
        );
        CreateGrid();
        CreatePlayer();
    }

    private void DrawPath(List<Tile> path)
    {
        foreach (var tile in _tileObjects)
        {
            tile.GetComponent<SpriteRenderer>().color = Color.white;
        }

        for (int i = 0; i < path.Count; i++)
        {
            float t = i / (path.Count - 1);
            _tileObjects[path[i].Id].GetComponent<SpriteRenderer>().color =
                Color.Lerp(Color.green, Color.red, t);
        }
    }

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }
        player = Instantiate(playerPrefab);
        player.Warp(Map.startTileId.Id);
    }

    private void CreateGrid()
    {
        Debug.Log("creategrid");
        if (_tileObjects != null)
        {
            foreach (var tile in _tileObjects)
            {
                Destroy(tile.gameObject);
            }
        }

        _tileObjects = new GameObject[MapWidth * MapHeight];

        var position = Vector3.zero;
        position.x -= (MapWidth * TileSize.x) / 2 - TileSize.x / 2;
        position.y += (MapHeight * TileSize.y) / 2 - TileSize.y / 2;
        for (int i = 0; i < MapHeight; i++)
        {
            for (int j = 0; j < MapWidth; j++)
            {
                var tileId = i * MapWidth + j;
                var newGO = Instantiate(TilePrefab, transform);
                newGO.transform.position = position;
                position.x += TileSize.x;

                _tileObjects[tileId] = newGO;
                DecorateTile(tileId);
            }
            position.x = -((MapWidth * TileSize.x) / 2 - TileSize.x / 2);
            position.y -= TileSize.y;
        }
        /*
        _fogObjects = new GameObject[MapWidth * MapHeight];
        for (int i = 0; i < _tileObjects.Length; i++)
        {
            var fogGO = new GameObject($"Fog_{i}");
            fogGO.transform.SetParent(transform);
            fogGO.transform.position = _tileObjects[i].transform.position;

            var sr = fogGO.AddComponent<SpriteRenderer>();
            sr.sprite = fogSprites[0];
            sr.sortingOrder = 1;

            _fogObjects[i] = fogGO;
        }*/
    }

    public void DecorateTile(int tileId)
    {
        var tile = _map.Tiles[tileId];
        var tileGO = _tileObjects[tileId];
        var renderer = tileGO.GetComponent<SpriteRenderer>();
        if (tile.AutoTileId != (int)TileTypes.Empty)
        {
            renderer.sprite = IslandSprites[tile.AutoTileId];
        }
        else
        {
            renderer.sprite = null;
        }
    }

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return WorldPosToTileId(worldPos);
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        var x = worldPos.x + (MapWidth * TileSize.x) / 2;
        var y = worldPos.y - (MapHeight * TileSize.y) / 2;
        var col = Mathf.FloorToInt(x / TileSize.x);
        var row = Mathf.FloorToInt(-y / TileSize.y);
        if (col < 0 || col >= MapWidth || row < 0 || row >= MapHeight)
        {
            return -1;
        }
        return row * MapWidth + col;
    }

    public Vector3 GetTilePos(int y, int x)
    {
        var pos = Vector3.zero;
        pos.x = x * TileSize.x - (MapWidth * TileSize.x) / 2 + TileSize.x / 2;
        pos.y = -(y * TileSize.y - (MapHeight * TileSize.y) / 2 + TileSize.y / 2);
        return pos;
    }

    public Vector3 GetTilePos(int tileId)
    {
        var y = tileId / MapWidth;
        var x = tileId % MapWidth;
        return GetTilePos(y, x);
    }
}

   