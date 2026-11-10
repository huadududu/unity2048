using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 集中加载 Tile 相关的 Addressables 资源（预制体 + TileState 配置），并负责句柄释放。
/// </summary>
public class TileAssetProvider
{
    // Addressables 中配置的地址与标签，编辑器脚本 AddressablesSetup 会写入同样的值。
    public const string TilePrefabAddress = "TilePrefab";
    public const string TileStateLabel = "tile-state";

    private AsyncOperationHandle<GameObject> tilePrefabHandle;
    private AsyncOperationHandle<IList<TileState>> tileStatesHandle;

    public GameObject TilePrefab { get; private set; }
    public TileState[] TileStates { get; private set; }

    /// <summary>
    /// 依次加载预制体与全部 TileState 配置，失败时对应属性保持为空。
    /// </summary>
    public IEnumerator LoadAsync()
    {
        yield return LoadTilePrefabAsync();
        yield return LoadTileStatesAsync();
    }

    public void Release()
    {
        if (tilePrefabHandle.IsValid())
        {
            Addressables.Release(tilePrefabHandle);
        }
        if (tileStatesHandle.IsValid())
        {
            Addressables.Release(tileStatesHandle);
        }
    }

    private IEnumerator LoadTilePrefabAsync()
    {
        tilePrefabHandle = Addressables.LoadAssetAsync<GameObject>(TilePrefabAddress);
        yield return tilePrefabHandle;

        if (tilePrefabHandle.Status == AsyncOperationStatus.Succeeded)
        {
            TilePrefab = tilePrefabHandle.Result;
            Debug.Log("[TileAssetProvider] 已通过 Addressables 加载 Tile 预制体。");
        }
        else
        {
            Debug.LogError($"[TileAssetProvider] Tile 预制体加载失败，请检查地址 {TilePrefabAddress} 或先构建 Addressables 内容。");
        }
    }

    private IEnumerator LoadTileStatesAsync()
    {
        tileStatesHandle = Addressables.LoadAssetsAsync<TileState>(TileStateLabel, null);
        yield return tileStatesHandle;

        if (tileStatesHandle.Status == AsyncOperationStatus.Succeeded
            && tileStatesHandle.Result != null
            && tileStatesHandle.Result.Count > 0)
        {
            // 标签加载不保证顺序，按 index 排序，保证 tileStates[index] 与方块等级一致。
            List<TileState> loadedStates = new List<TileState>(tileStatesHandle.Result);
            loadedStates.Sort((a, b) => a.index.CompareTo(b.index));
            TileStates = loadedStates.ToArray();
            Debug.Log($"[TileAssetProvider] 已通过 Addressables 加载 {TileStates.Length} 个 TileState 配置。");
        }
        else
        {
            Debug.LogError($"[TileAssetProvider] TileState 加载失败，请检查标签 {TileStateLabel} 或先构建 Addressables 内容。");
        }
    }
}
