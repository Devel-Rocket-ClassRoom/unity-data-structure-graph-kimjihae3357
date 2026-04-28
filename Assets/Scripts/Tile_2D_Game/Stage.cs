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

        if(Input.GetKeyDown(KeyCode.Mouse0))
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
                _tileObjects[currentTileId].GetComponent<SpriteRenderer>().color= Color.green;
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
        for (int i = 0; i < _map.Tiles.Length; i++)
        {
            if (GetTileDistance(playerTileId, i) <= visibleRange)
                _map.Tiles[i].IsVisited = true;
        }

        for (int i = 0; i < _fogObjects.Length; i++)
        {
            if (_map.Tiles[i].IsVisited)
            {
                _fogObjects[i].SetActive(false);
            }
            else
            {
                bool isBorder = false;
                foreach (var adj in _map.Tiles[i].Adjacents)
                {
                    if (adj != null && adj.IsVisited)
                    {
                        isBorder = true;
                        break;
                    }
                }

                _fogObjects[i].SetActive(true);
                if (isBorder)
                {
                    int fogSpriteId = GetFogSpriteId(i);
                    _fogObjects[i].GetComponent<SpriteRenderer>().sprite = fogSprites[fogSpriteId];
                }
                else
                {
                    _fogObjects[i].GetComponent<SpriteRenderer>().sprite = fogSprites[0];
                }
            }
        }
    }
    private int GetFogSpriteId(int tileId)
    {
        var tile = _map.Tiles[tileId];
        int id = 0;

        // AutoTileId와 동일한 비트 구조
        // 0001 Top
        // 0010 Left  
        // 0100 Right
        // 1000 Bottom
        if (tile.Adjacents[(int)Sides.Top] != null && tile.Adjacents[(int)Sides.Top].IsVisited)
            id |= 1 << (int)Sides.Top;
        if (tile.Adjacents[(int)Sides.Left] != null && tile.Adjacents[(int)Sides.Left].IsVisited)
            id |= 1 << (int)Sides.Left;
        if (tile.Adjacents[(int)Sides.Right] != null && tile.Adjacents[(int)Sides.Right].IsVisited)
            id |= 1 << (int)Sides.Right;
        if (tile.Adjacents[(int)Sides.Bottom] != null && tile.Adjacents[(int)Sides.Bottom].IsVisited)
            id |= 1 << (int)Sides.Bottom;

        Debug.Log($"tileId: {tileId}, id: {id}, " +
       $"Top: {tile.Adjacents[(int)Sides.Top]?.IsVisited}, " +
       $"Left: {tile.Adjacents[(int)Sides.Left]?.IsVisited}, " +
       $"Right: {tile.Adjacents[(int)Sides.Right]?.IsVisited}, " +
       $"Bottom: {tile.Adjacents[(int)Sides.Bottom]?.IsVisited}");

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

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }
        player = Instantiate(playerPrefab);
        player.MoveTo(Map.startTileId.Id);
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
        }
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