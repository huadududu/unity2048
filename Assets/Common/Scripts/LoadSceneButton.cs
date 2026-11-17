using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 挂在 uGUI Button 上：点击时加载 targetScene 指定的场景。
/// 用于主菜单进入各游戏、游戏内退出返回主菜单。
/// </summary>
public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Load);
    }

    private void Load()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[LoadSceneButton] targetScene 未设置：" + name, this);
            return;
        }

        SceneManager.LoadScene(targetScene);
    }
}
