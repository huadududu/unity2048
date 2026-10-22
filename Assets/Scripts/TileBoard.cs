using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileBoard : MonoBehaviour
{

    // public GameManager GameManager;
    public GameObject tilePrefab;
    public TileState[] tileStates;

    public TileGrid grid;
    private List<Tile> tiles = new List<Tile>();
    private bool waiting = false;

    private Vector2 startPos;
    private bool isSwiping = false;
    private const float minSwipeDistance = 100f;  // 小于这个距离算点击，不算滑动
    private Pooler tilePool;
    private const int k_StartingTilePoolSize = 16;



    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in tiles)
        {
            tile.cell = null;
            this.tilePool.Free(tile.gameObject);
            // Destroy(tile.gameObject);
        }
        tiles.Clear();
    }

    // 把低阶tile清除掉，避免占用空间
    public void ClearLowTile()
    {
        waiting = false;
        foreach (var cell in grid.cells)
        {
            Tile tile = cell.tile;
            if (tile == null) continue;
            int idx = GameManager.Instance.getCreateStateIndex();
            idx = Mathf.Max(idx - 3, 3);
            if (tile.state.index <= idx)
            {
                cell.tile = null;
                tile.cell = null;
                tiles.Remove(tile);
                this.tilePool.Free(tile.gameObject);
                // Destroy(tile.gameObject);
            }
        }
    }

    public void FreeTile(GameObject tileObj)
    {
        this.tilePool.Free(tileObj);
    }

    public void CreateTile()
    {
        GameObject tileObj = tilePool.Get();
        Tile tile = tileObj.GetComponent<Tile>();
        tileObj.transform.SetParent(grid.transform, false);
        tileObj.transform.localScale = Vector3.one;
        // Tile tile = Instantiate(tilePrefab, grid.transform);
        // if (GameManager.Instance.score >= 1028){
        //     tile.SetState(tileStates[10+Random.Range(0, 3)]); // 教程更新
        // }
        // else{
        int idx = GameManager.Instance.getCreateStateIndex();
        int min = Mathf.Max(idx - 3, 0);
        tile.SetState(tileStates[Random.Range(min, idx)]); // 教程更新
        // }
        tile.LinkCell(grid.getRandomEmptyCell());
        tiles.Add(tile);
    }
    public void CreateTile(int x, int y, int stateIndex = 0)
    {
        // Tile tile = Instantiate(tilePrefab, grid.transform);
        GameObject tileObj = tilePool.Get();
        Tile tile = tileObj.GetComponent<Tile>();
        tileObj.transform.SetParent(grid.transform, false);
        tileObj.transform.localScale = Vector3.one;
        // if (GameManager.Instance.score >= 1028){
        //     tile.SetState(tileStates[10+Random.Range(0, 3)]); // 教程更新
        // }
        // else{
        // tile.SetState(tileStates[Random.Range(0, 2)]); // 教程更新
        tile.SetState(tileStates[stateIndex]); // 教程更新
        // }
        TileCell emptyCell = grid.GetCell(x, y);
        tile.LinkCell(emptyCell);
        tiles.Add(tile);
    }



    // Start is called before the first frame update
    // void Start()
    // {
    // 
    // }

    // // Update is called once per frame
    private void Update()
    {
        if (waiting)
        {
            Debug.Log("waiting");
            return;
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && isSwiping == false)
            {
                startPos = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                DetectSwipe(touch.position - startPos);
                isSwiping = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down, 0, 1, grid.height - 2, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
    }

    void DetectSwipe(Vector2 delta)
    {
        Debug.LogFormat("Swipe detected: {0}, {1}", delta.x, delta.y);
        // 主要方向是水平的，且距离够长
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > minSwipeDistance)
        {
            if (delta.x > 0)
                Move(Vector2Int.right, grid.width - 2, -1, 0, 1);
            else
                Move(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && Mathf.Abs(delta.y) > minSwipeDistance)
        {
            if (delta.y > 0)
                Move(Vector2Int.up, 0, 1, 1, 1);
            else
                Move(Vector2Int.down, 0, 1, grid.height - 2, -1);
        }
    }

    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;
        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);
                if (cell.isOccupied())
                {
                    changed |= MoveTile(cell.tile, direction);
                }

            }
        }
        if (changed)
        {
            waiting = true;
            StartCoroutine(WaitForChanges());
        }
    }

    private IEnumerator WaitForChanges()
    {

        yield return new WaitForSeconds(0.1f);

        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        if (tiles.Count != grid.size)
        {
            CreateTile();
        }

        waiting = false;
        if (CheckForGameOver())
        {
            GameManager.Instance.GameOver();
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjcent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjcent != null)
        {
            if (adjcent.isOccupied())
            {
                if (CanMerge(tile, adjcent.tile))
                {
                    MergeTiles(tile, adjcent.tile);
                    return true;
                }
                break;
            }
            newCell = adjcent;
            adjcent = grid.GetAdjacentCell(adjcent, direction);
        }
        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.state == b.state && !b.locked;
    }

    private void MergeTiles(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);
        int index = b.state.index;
        // int index = Mathf.Clamp(b.state.index, 0, tileStates.Length - 1);
        // int index = Mathf.Clamp(tileStates.IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        TileState newState = tileStates[index];

        b.SetState(newState);

        // GameManager.Instance.IncreaseScore(newState.number);
        GameManager.Instance.IncreaseScore((int)Mathf.Pow(2, newState.index));
        GameManager.Instance.UpdateMaxStateIndex(index);
    }

    public bool CheckForGameOver()
    {

        if (tiles.Count != grid.size)
        {
            return false;
        }
        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);
            if (up != null && CanMerge(tile, up.tile))
            {
                return false;
            }
            if (down != null && CanMerge(tile, down.tile))
            {
                return false;
            }
            if (left != null && CanMerge(tile, left.tile))
            {
                return false;
            }
            if (right != null && CanMerge(tile, right.tile))
            {
                return false;
            }
        }
        return true;
    }

    void Awake()
    {
        this.tilePool = new Pooler(tilePrefab, k_StartingTilePoolSize);

    }
}
