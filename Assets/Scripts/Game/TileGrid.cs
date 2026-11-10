using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{

    public TileRow[] rows { get; private set; }//
    public TileCell[] cells { get; private set; }//
    // // public Tile tile;//关联的tile

    public int size => cells.Length;
    public int height => rows.Length;
    public int width => size / height;

    // private
    private void Awake()
    {
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();
    }

    private void Start()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < rows[i].cells.Length; j++)
            {
                rows[i].cells[j].coordinates = new Vector2Int(j, i);
            }
        }
    }

    // public
    public TileCell GetCell(Vector2Int coord)
    {
        return GetCell(coord.x, coord.y);
    }
    public TileCell GetCell(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return rows[y].cells[x];
        }
        else
        {
            return null;
        }
    }

    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinates = cell.coordinates;
        coordinates.x += direction.x;
        coordinates.y -= direction.y;
        return GetCell(coordinates);
    }
    /// <summary>
    /// 获取一个随机空单元格作为新块的生成点
    /// 两遍扫描：先数出空格总数，再取第 k 个空格，无额外内存分配
    /// </summary>
    /// <returns>空单元格或 null</returns>
    public TileCell GetRandomEmptyCell()
    {
        int emptyCount = 0;
        foreach (var cell in cells)
        {
            if (cell.IsEmpty())
            {
                emptyCount++;
            }
        }
        if (emptyCount == 0)
        {
            return null;
        }

        int pick = Random.Range(0, emptyCount);
        foreach (var cell in cells)
        {
            if (cell.IsEmpty())
            {
                if (pick == 0)
                {
                    return cell;
                }
                pick--;
            }
        }
        return null;
    }

    // // public bool IsEmpty(){
    // //     return tile = null;
    // // }

    // // public bool isOccupied(){
    // //     return tile != null;
    // // }

    // // Start is called before the first frame update
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {

    // }
}
