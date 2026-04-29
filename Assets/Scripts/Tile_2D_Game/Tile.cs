using UnityEngine;

public enum Sides
{
    None = -1,
    Top,
    Left,
    Right,
    Bottom,
    
}

public class Tile
{
    public int Id;
    public Tile[] Adjacents = new Tile[4];
    public int AutoTileId;

    public bool IsVisited = false;

    public bool CanMove => Weight != int.MaxValue;

    // TileTypes. Grass(15)=1, Tree(16)=2, Hills(17)=4, Mountains(18)=MAX(통과 불가), Towns(19)=1, Castle(20)=1, Monster(21)=1.

    public static readonly int[] tableWeight =
    {
        int.MaxValue,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        2, 4, int.MaxValue, 1, 1, 1,
    };

    public int Weight => tableWeight[AutoTileId + 1];

    public Tile previousTile = null;

    public void ClearPreviousTile()
    {
        previousTile = null;
    }

    public void UpdateAutoTileId()
    {
        AutoTileId = 0;
        for (int i = 0; i < Adjacents.Length; i++)
        {
            if (Adjacents[i] != null)
            {
                // 1000 Bottom
                // 0100 Right
                // 0010 Left
                // 0001 Top

                AutoTileId |= 1 << i;
            }
        }
    }

    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < Adjacents.Length; i++)
        {
            if (Adjacents[i] == null) continue;

            if (Adjacents[i].Id == tile.Id)
            {
                Adjacents[i] = null;
                UpdateAutoTileId();
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        for (int i = 0; i < Adjacents.Length; i++)
        {
            if (Adjacents[i] == null) continue;

            Adjacents[i].RemoveAdjacents(this);
            Adjacents[i] = null;
        }

        UpdateAutoTileId();
    }
}
