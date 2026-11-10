using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameObject tilePrefab;
    public TileState[] tileStates;

    public TileGrid grid;
    private List<Tile> tiles = new List<Tile>();

    private bool ready = false;
    public bool IsReady => ready;

    // 在途动画计数：一次移动产生的所有移动/合并动画全部结束后，才解锁、生成新块并判断结束
    private int pendingMoves = 0;

    private Pooler tilePool;
    private const int k_StartingTilePoolSize = 16;
    private TileAssetProvider assetProvider = new TileAssetProvider();

    // 合并中的配对：key 为正在飞向目标的 tile，value 为动画结束后要弹跳表现的升级 tile
    private readonly Dictionary<GameObject, Tile> pendingMergePops = new Dictionary<GameObject, Tile>();

    private void Awake()
    {
        // 输入、动画完成、回收三类事件都由持有规则与流程状态的 TileBoard 统一订阅处理
        TileInputEvent.Register(OnTileMoveInput);
        TileMoveCompletedEvent.Register(OnTileMoveCompleted);
        TileRecycleEvent.Register(OnTileRecycle);
    }

    private void OnTileRecycle(GameObject tileObj)
    {
        FreeTile(tileObj);
    }

    /// <summary>
    /// 响应输入方向。是否响应由棋盘自身状态决定：
    /// 组件被禁用（游戏结束）、资源未就绪、上一轮动画未结束时都直接忽略。
    /// </summary>
    private void OnTileMoveInput(Vector2Int direction)
    {
        if (!enabled || !ready || pendingMoves > 0)
        {
            return;
        }
        Move(direction);
    }

    /// <summary>
    /// 在途动画计数归零后继续流程，不再依赖与动画时长保持一致的定时等待
    /// </summary>
    private void OnTileMoveCompleted(GameObject tileObj)
    {
        // 合并动画结束时，让升级后的目标块弹跳一下（与流程计数相互独立）
        if (pendingMergePops.Remove(tileObj, out Tile merged))
        {
            merged.Pop();
        }

        // 只统计由 Move 发起的动画；外部直接调用 MoveTo/Merge 产生的动画不驱动流程
        if (pendingMoves == 0)
        {
            return;
        }

        pendingMoves--;
        if (pendingMoves == 0)
        {
            OnAllMovesCompleted();
        }
    }

    private void OnAllMovesCompleted()
    {
        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        if (tiles.Count != grid.size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            GameOverEvent.Trigger();
        }
    }

    /// <summary>
    /// 清空整个棋盘
    /// </summary>
    public void ClearBoard()
    {
        // 动画中的 tile 被直接回收后不会再发完成事件，流程计数与合并弹跳配对一并清零
        pendingMoves = 0;
        pendingMergePops.Clear();

        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in tiles)
        {
            tile.cell = null;
            this.tilePool.Free(tile.gameObject);
        }
        tiles.Clear();
    }
    public void NewGame()
    {
        this.ClearBoard();
        this.CreateTile();
        this.CreateTile();
        this.CreateTile();
    }

    /// <summary>
    /// 清除低阶的 tile，保留高阶的 tile，避免占用空间
    /// 优化：只遍历已占用的单元格，而不是所有单元格
    /// </summary>
    public void ClearLowTile()
    {
        // 防御性清零：继续游戏时确保流程不卡在途状态，也清掉残留的弹跳配对
        pendingMoves = 0;
        pendingMergePops.Clear();

        // 获取当前最大状态索引
        var gameModel = GameApp.Get<GameModel>();
        int maxStateIndex = gameModel.MaxTileIndex.Value;
        // 保留最高3个等级的 tile，清除更低的
        int clearThreshold = Mathf.Max(maxStateIndex - 3, 3);

        // 优化：只遍历已占用的单元格
        foreach (var cell in grid.cells)
        {
            Tile tile = cell.tile;
            if (tile == null) continue;

            // 如果 tile 的等级低于清除阈值，则清除它
            if (tile.state.index <= clearThreshold)
            {
                cell.tile = null;
                tile.cell = null;
                tiles.Remove(tile);
                this.tilePool.Free(tile.gameObject);
            }
        }
    }

    /// <summary>
    /// 释放 tile 对象
    /// </summary>
    public void FreeTile(GameObject tileObj)
    {
        this.tilePool.Free(tileObj);
    }

    public void CreateTile()
    {
        GameObject tileObj = this.tilePool.Get();
        Tile tile = tileObj.GetComponent<Tile>();
        tileObj.transform.SetParent(grid.transform, false);
        tileObj.transform.localScale = Vector3.one;
        var gameModel = GameApp.Get<GameModel>();
        int idx = gameModel.MaxTileIndex.Value;
        int min = Mathf.Max(idx - 3, 0);
        tile.SetState(tileStates[Random.Range(min, idx)]); // 教程更新
        // }
        tile.LinkCell(grid.GetRandomEmptyCell());
        tile.Spawn();
        tiles.Add(tile);
    }
    public void CreateTile(int x, int y, int stateIndex = 0)
    {
        // Tile tile = Instantiate(tilePrefab, grid.transform);
        GameObject tileObj = tilePool.Get();
        Tile tile = tileObj.GetComponent<Tile>();
        tileObj.transform.SetParent(grid.transform, false);
        tileObj.transform.localScale = Vector3.one;
        tile.SetState(tileStates[stateIndex]);
        TileCell emptyCell = grid.GetCell(x, y);
        tile.LinkCell(emptyCell);
        tile.Spawn();
        tiles.Add(tile);
    }



    /// <summary>
    /// 执行一次方向移动。
    /// 扫描顺序由方向推导：从最靠近目的地一侧的行/列开始，保证靠前的块先落位；
    /// 每个成功的移动/合并都会启动一个动画并计入 pendingMoves。
    /// </summary>
    private void Move(Vector2Int direction)
    {
        int startX = direction.x > 0 ? grid.width - 2 : (direction.x < 0 ? 1 : 0);
        int incrementX = direction.x > 0 ? -1 : 1;
        int startY = direction.y > 0 ? 1 : (direction.y < 0 ? grid.height - 2 : 0);
        int incrementY = direction.y < 0 ? -1 : 1;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);
                if (cell.isOccupied() && MoveTile(cell.tile, direction))
                {
                    pendingMoves++;
                }
            }
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
        // 记录配对：a 飞向目标格的动画结束后，b 弹跳表现升级
        pendingMergePops[a.gameObject] = b;
        a.Merge(b.cell);
        int index = b.state.index;
        TileState newState = tileStates[index];

        b.SetState(newState);
        new AddScoreCommand(newState.index).Excute();
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

    /// <summary>
    /// 通过 TileAssetProvider 异步加载资源，任一加载失败都会记录错误并停止初始化，避免带着空棋盘继续运行。
    /// </summary>
    public IEnumerator InitializeAsync()
    {
        if (ready)
        {
            yield break;
        }

        yield return assetProvider.LoadAsync();

        tilePrefab = assetProvider.TilePrefab;
        tileStates = assetProvider.TileStates;

        if (tilePrefab == null || tileStates == null || tileStates.Length == 0)
        {
            Debug.LogError("[TileBoard] Addressables 资源未就绪，棋盘无法初始化。");
            yield break;
        }

        this.tilePool = new Pooler(tilePrefab, k_StartingTilePoolSize);
        ready = true;
    }

    private void OnDestroy()
    {
        TileInputEvent.UnRegister(OnTileMoveInput);
        TileMoveCompletedEvent.UnRegister(OnTileMoveCompleted);
        TileRecycleEvent.UnRegister(OnTileRecycle);
        assetProvider.Release();
    }
}
