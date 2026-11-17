using UnityEngine;

/// <summary>
/// 公共输入层：识别键盘 WASD/方向键与触摸滑动方向，广播 DirectionInputEvent。
/// 只负责“识别”，是否响应、如何过滤由各游戏订阅方决定。
/// </summary>
public class DirectionInput : MonoBehaviour
{
    [SerializeField] private bool enableSwipe = true;
    [SerializeField] private float minSwipeDistance = 100f; // 小于这个距离算点击，不算滑动

    private Vector2 startPos;
    private bool isSwiping = false;

    // Update is called once per frame
    private void Update()
    {
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
                if (enableSwipe)
                {
                    DetectSwipe(touch.position - startPos);
                }
                isSwiping = false;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            DirectionInputEvent.Trigger(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DirectionInputEvent.Trigger(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            DirectionInputEvent.Trigger(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            DirectionInputEvent.Trigger(Vector2Int.right);
        }
    }

    private void DetectSwipe(Vector2 delta)
    {
        // 主要方向是水平的，且距离够长
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > minSwipeDistance)
        {
            DirectionInputEvent.Trigger(delta.x > 0 ? Vector2Int.right : Vector2Int.left);
        }
        else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && Mathf.Abs(delta.y) > minSwipeDistance)
        {
            DirectionInputEvent.Trigger(delta.y > 0 ? Vector2Int.up : Vector2Int.down);
        }
    }
}
