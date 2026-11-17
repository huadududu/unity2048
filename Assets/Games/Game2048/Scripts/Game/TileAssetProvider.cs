using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 集中加载 Tile 相关的资源（预制体 + TileState 配置）。
/// 资源放在 Resources 目录下，随首包一起打进包体内，小游戏里不需要 Addressables 与 CDN 即可离线加载。
/// </summary>
public class TileAssetProvider
{
    // Resources 路径（不含扩展名），对应 Assets/Games/Game2048/Resources 下的资源与目录。
    public const string TilePrefabPath = "Prefabs/Tile";
    public const string TileStatesFolder = "Tiles";

    public GameObject TilePrefab { get; private set; }
    public TileState[] TileStates { get; private set; }

    /// <summary>
    /// 依次加载预制体与全部 TileState 配置，失败时对应属性保持为空。
    /// </summary>
    public IEnumerator LoadAsync()
    {
        LoadTilePrefab();
        LoadTileStates();
        yield break;
    }

    public void Release()
    {
        // Resources 资源由 Unity 统一管理生命周期，这里只清空引用。
        TilePrefab = null;
        TileStates = null;
    }

    private void LoadTilePrefab()
    {
        TilePrefab = Resources.Load<GameObject>(TilePrefabPath);

        if (TilePrefab != null)
        {
            Debug.Log("[TileAssetProvider] 已加载 Tile 预制体。");
        }
        else
        {
            Debug.LogError($"[TileAssetProvider] Tile 预制体加载失败，请检查 Resources/{TilePrefabPath}.prefab 是否存在。");
        }
    }

    private void LoadTileStates()
    {
        TileState[] loadedStates = Resources.LoadAll<TileState>(TileStatesFolder);

        if (loadedStates == null || loadedStates.Length == 0)
        {
            Debug.LogError($"[TileAssetProvider] TileState 加载失败，请检查 Resources/{TileStatesFolder} 目录。");
            return;
        }

        // 加载不保证顺序，按 index 排序，保证 tileStates[index] 与方块等级一致。
        List<TileState> sortedStates = new List<TileState>(loadedStates);
        sortedStates.Sort((a, b) => a.index.CompareTo(b.index));
        TileStates = sortedStates.ToArray();
        Debug.Log($"[TileAssetProvider] 已加载 {TileStates.Length} 个 TileState 配置。");
    }
}
