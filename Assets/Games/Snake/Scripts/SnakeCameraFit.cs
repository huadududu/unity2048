using UnityEngine;

/// <summary>
/// 让正交相机按宽度适配贪吃蛇棋盘（棋盘位于世界原点，含四周墙壁宽 17 单位）。
/// 屏幕宽度始终对应棋盘宽度，高度由屏幕宽高比决定：竖屏下方会多出纵向空间，横屏下会裁掉上下部分。
/// </summary>
[RequireComponent(typeof(Camera))]
public class SnakeCameraFit : MonoBehaviour
{
    // 棋盘含墙壁的半宽，修改棋盘尺寸时需同步。
    public float halfWidth = 8.5f;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Fit();
    }

    private void Update()
    {
        // 分辨率 / 窗口尺寸变化（含转屏）时重新适配
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            Fit();
        }
    }

    private void Fit()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                return;
            }
        }

        cam.orthographic = true;
        // 正交尺寸即可视半高：orthographicSize * aspect = 可视半宽。
        // 按宽度适配时固定可视半宽为 halfWidth，半高随宽高比得出。
        cam.orthographicSize = halfWidth / Mathf.Max(cam.aspect, 0.01f);
    }
}
