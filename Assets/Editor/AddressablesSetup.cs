using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 一次性生成 Addressables 配置：设置、组、条目和标签。
/// 菜单：Tools/2048/Setup Addressables
/// 批处理：-executeMethod AddressablesSetup.SetupAndBuild
/// </summary>
public static class AddressablesSetup
{
    private const string GroupName = "TileAssets";
    private const string PrefabPath = "Assets/Prefabs/Tile.prefab";
    private const string TilesFolder = "Assets/Tiles";
    private const string PrefabAddress = "TilePrefab";
    private const string TileStateLabel = "tile-state";

    [MenuItem("Tools/2048/Setup Addressables")]
    public static void Setup()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettings.Create(
                AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                true,
                true);
            AddressableAssetSettingsDefaultObject.Settings = settings;
            Debug.Log("[AddressablesSetup] 已创建 Addressables 设置。");
        }

        AddressableAssetGroup group = settings.FindGroup(GroupName);
        if (group == null)
        {
            group = settings.CreateGroup(
                GroupName,
                false,
                false,
                false,
                null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
            Debug.Log($"[AddressablesSetup] 已创建资源组 {GroupName}。");
        }

        BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
        if (bundledSchema != null)
        {
            bundledSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundledSchema.IncludeInBuild = true;
        }

        AddEntry(settings, group, PrefabPath, PrefabAddress, null);

        int stateCount = 0;
        string[] stateGuids = AssetDatabase.FindAssets("t:TileState", new[] { TilesFolder });
        foreach (string stateGuid in stateGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(stateGuid);
            TileState state = AssetDatabase.LoadAssetAtPath<TileState>(path);
            if (state == null)
            {
                continue;
            }

            AddEntry(settings, group, path, $"TileState_{state.index}", TileStateLabel);
            stateCount++;
        }

        // 打包 Player 时自动构建 Addressables 内容，保持原有“点一次 Build 就能出包”的流程。
        settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;

        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(group);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AddressablesSetup] 完成：预制体 1 个，TileState {stateCount} 个，标签 {TileStateLabel}。");
    }

    [MenuItem("Tools/2048/Build Addressables Content")]
    public static void BuildContent()
    {
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"[AddressablesSetup] 内容构建失败：{result.Error}");
            if (Application.isBatchMode)
            {
                throw new Exception(result.Error);
            }
            return;
        }

        Debug.Log($"[AddressablesSetup] 内容构建成功：{result.OutputPath}");
    }

    public static void SetupAndBuild()
    {
        Setup();
        BuildContent();
    }

    [MenuItem("Tools/2048/Validate Addressables Load")]
    public static void ValidateLoad()
    {
        Addressables.InitializeAsync().WaitForCompletion();

        AsyncOperationHandle<GameObject> prefabHandle =
            Addressables.LoadAssetAsync<GameObject>(PrefabAddress);
        GameObject prefab = prefabHandle.WaitForCompletion();

        AsyncOperationHandle<IList<TileState>> statesHandle =
            Addressables.LoadAssetsAsync<TileState>(TileStateLabel, null);
        IList<TileState> states = statesHandle.WaitForCompletion();

        bool prefabOk = prefabHandle.Status == AsyncOperationStatus.Succeeded && prefab != null;
        bool statesOk = statesHandle.Status == AsyncOperationStatus.Succeeded
                        && states != null
                        && states.Count > 0;

        Debug.Log($"[AddressablesSetup] 加载验证：预制体 {(prefabOk ? "成功" : "失败")}，" +
                  $"TileState {(statesOk ? states.Count + " 个" : "失败")}。");

        if (statesHandle.IsValid())
        {
            Addressables.Release(statesHandle);
        }
        if (prefabHandle.IsValid())
        {
            Addressables.Release(prefabHandle);
        }

        if (!prefabOk || !statesOk)
        {
            if (Application.isBatchMode)
            {
                throw new Exception("Addressables 加载验证失败。");
            }
        }
    }

    private static void AddEntry(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        string assetPath,
        string address,
        string label)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError($"[AddressablesSetup] 找不到资源：{assetPath}");
            return;
        }

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        if (entry == null)
        {
            Debug.LogError($"[AddressablesSetup] 添加条目失败：{assetPath}");
            return;
        }

        entry.address = address;
        if (!string.IsNullOrEmpty(label))
        {
            entry.SetLabel(label, true, true, false);
        }
    }
}
