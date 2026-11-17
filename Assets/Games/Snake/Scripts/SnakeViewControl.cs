using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Snake 视图层：启动时读档初始化最高分，订阅 SnakeModel 分数变化并刷新 UI；
/// 同时根据屏幕安全区下移顶部 UI，避免刘海屏、挖孔屏与状态栏遮挡。
/// </summary>
public class SnakeViewControl : MonoBehaviour
{
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtBestScore;
    public RectTransform topBar;

    private Canvas canvas;
    private float topBarBaseY;
    private float topInsetPixels;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        if (topBar == null)
        {
            return;
        }

        canvas = topBar.GetComponentInParent<Canvas>();
        // 记录场景里设计好的初始位置，后续都在这个基准上叠加安全区偏移
        topBarBaseY = topBar.anchoredPosition.y;
    }

    private void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            topInsetPixels = GetTopInsetPixels();
        }

        ApplyTopBarPosition();
    }

    // 微信小游戏没有实现 Screen.safeArea，只能从微信系统信息里取安全区；
    // 其他平台继续用引擎的 Screen.safeArea
    private float GetTopInsetPixels()
    {
#if (WEIXINMINIGAME || UNITY_WEIXINMINIGAME) && !UNITY_EDITOR
        try
        {
            WeChatWASM.SystemInfo info = WeChatWASM.WX.GetSystemInfoSync();
            if (info != null)
            {
                // 微信返回的是逻辑像素，按屏幕宽度换算成 Unity 侧像素
                float windowWidth = info.windowWidth > 0 ? (float)info.windowWidth : (float)info.screenWidth;
                float pixelsPerUnit = windowWidth > 0 ? Screen.width / windowWidth : 1f;
                float top = info.safeArea != null ? (float)info.safeArea.top : 0f;

                // 右上角胶囊按钮不在安全区内，额外避让
                WeChatWASM.ClientRect button = WeChatWASM.WX.GetMenuButtonBoundingClientRect();
                if (button != null)
                {
                    top = Mathf.Max(top, (float)button.bottom + 4f);
                }

                if (top > 0f)
                {
                    return top * pixelsPerUnit;
                }
            }
        }
        catch (System.Exception)
        {
            // 微信接口不可用时退回引擎安全区
        }
#endif
        Rect safeArea = Screen.safeArea;
        if (safeArea.width <= 0f || safeArea.height <= 0f)
        {
            return 0f;
        }
        return Mathf.Max(0f, Screen.height - safeArea.yMax);
    }

    // 安全区像素先除以 Canvas 缩放系数换算成 UI 单位，再从 Top 的设计位置下移
    private void ApplyTopBarPosition()
    {
        if (topBar == null)
        {
            return;
        }

        float scale = canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
        float targetY = topBarBaseY - topInsetPixels / scale;

        if (!Mathf.Approximately(topBar.anchoredPosition.y, targetY))
        {
            topBar.anchoredPosition = new Vector2(topBar.anchoredPosition.x, targetY);
        }
    }

    private void Start()
    {
        new SnakeCommands.InitBestScoreCommand().Excute();

        var snakeModel = SnakeGame.Get<SnakeModel>();
        snakeModel.Score.OnValueChanged += SetScore;
        snakeModel.BestScore.OnValueChanged += SetBestScore;
        SetScore(snakeModel.Score.Value);
        SetBestScore(snakeModel.BestScore.Value);
    }

    private void OnDestroy()
    {
        SnakeGame.Get<SnakeModel>().Score.OnValueChanged -= SetScore;
        SnakeGame.Get<SnakeModel>().BestScore.OnValueChanged -= SetBestScore;
    }

    private void SetScore(int score)
    {
        txtScore.text = score.ToString();
    }
    private void SetBestScore(int bestScore)
    {
        txtBestScore.text = bestScore.ToString();
    }
}
