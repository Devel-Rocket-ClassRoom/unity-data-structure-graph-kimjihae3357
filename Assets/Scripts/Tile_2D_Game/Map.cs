using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    // Coast 0 ~ 14,
    Grass = 15,
    Tree,
    Hills,
    Mountains,
    Towns,
    Castle,
    Monster
}

public class Map
{
    public int Rows = 0;
    public int Columns = 0;

    public Tile[] Tiles;

    public Tile[] CoastTiles => Tiles.Where(t => t.AutoTileId >= 0 && t.AutoTileId < (int)TileTypes.Grass).ToArray();
    public Tile[] LandTiles => Tiles.Where(t => t.AutoTileId == (int)TileTypes.Grass).ToArray();

    public Tile startTileId;
    public Tile castleTileId;

    public void Init(int rows, int cols)
    {
        Rows = rows;
        Columns = cols;

        Tiles = new Tile[rows * cols];
        for (int i = 0; i < Tiles.Length; i++)
        {
            Tiles[i] = new();
            Tiles[i].Id = i;
        }

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                int index = r * Columns + c;
                var adgacents = Tiles[index].Adjacents;
                if (r - 1 >= 0)
                {
                    adgacents[(int)Sides.Top] = Tiles[index - Columns]; // Up
                }

                if (c + 1 < Columns)
                {
                    adgacents[(int)Sides.Right] = Tiles[index + 1]; // Right
                }

                if (c - 1 >= 0)
                {
                    adgacents[(int)Sides.Left] = Tiles[index - 1]; // Left
                }

                if (r + 1 < Rows)
                {
                    adgacents[(int)Sides.Bottom] = Tiles[index + Columns]; // Down
                }
            }
        }

        for (int i = 0; i < Tiles.Length; i++)
        {
            Tiles[i].UpdateAutoTileId();
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            int rdmIdx = Random.Range(0, i + 1);
            (tiles[rdmIdx], tiles[i]) = (tiles[i], tiles[rdmIdx]);
        }
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);
        int total = Mathf.FloorToInt(tiles.Length * percent);
        for (int i = 0; i < total; i++)
        {
            if (tileType == TileTypes.Empty)
            {
                tiles[i].ClearAdjacents();
            }

            tiles[i].AutoTileId = (int)tileType;
        }
    }

    public bool CreateIsLand(
        float erodePercent,
        int erodeIterations,
        float lakePercent,
        float treePercent,
        float hillPercent,
        float mountainPercent,
        float townPercent,
        float monsterPercent) // Castle 1개만
    {
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        var towns = Tiles.Where(x => x.AutoTileId == (int)TileTypes.Towns).ToArray();
        ShuffleTiles(towns);
        startTileId = towns[0];
        castleTileId = towns[1];
        castleTileId.AutoTileId = (int)TileTypes.Castle;

        //var path = PathFindingAstar(startTileId, castleTileId);
        return true;
    }

    public List<Tile> PathFindingAstar(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();

        path.Clear();

        var distances = new int[Tiles.Length];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }
        distances[startTile.Id] = 0;

        PriorityQueue<Tile, int> pq = new PriorityQueue<Tile, int>();
        pq.Enqueue(startTile, 0 + Heuristic(startTile, endTile));
        var visited = new HashSet<Tile>();
        Tile[] previous = new Tile[Tiles.Length];
        while (pq.Count > 0)
        {
            var u = pq.Dequeue();
            if (visited.Contains(u))
            {
                continue;
            }
            visited.Add(u);
            if (u == endTile)
            {
                var temp = u;
                while (temp != null)
                {
                    path.Add(temp);
                    temp = previous[temp.Id];
                }
                path.Reverse();
                break;
            }
            foreach (var v in u.Adjacents)
            {
                if (v == null)
                    continue;

                if (!v.CanMove || visited.Contains(v))
                {
                    continue;
                }

                int newDist = distances[u.Id] + v.Weight;

                if (previous[v.Id] == null || newDist < distances[v.Id])
                {
                    distances[v.Id] = newDist;
                    previous[v.Id] = u;
                    pq.Enqueue(v, newDist + Heuristic(v, endTile));
                }
            }
        }
        return path;
    }
    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.Id % Columns;
        int ay = a.Id / Columns;

        int bx = b.Id % Columns;
        int by = b.Id / Columns;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }
}