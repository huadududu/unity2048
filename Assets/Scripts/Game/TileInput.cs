using UnityEngine;

/// <summary>
/// 输入层：识别键盘/触摸滑动方向并广播 TileInputEvent。
/// 只负责“识别”，是否响应、能否移动由订阅方 TileBoard 依据自身状态决定。
/// </summary>
public class TileInput : MonoBehaviour
{
    private Vector2 startPos;
    private bool isSwiping = false;
    private const float minSwipeDistance = 100f;  // 小于这个距离算点击，不算滑动

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
                DetectSwipe(touch.position - startPos);
                isSwiping = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TileInputEvent.Trigger(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TileInputEvent.Trigger(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TileInputEvent.Trigger(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TileInputEvent.Trigger(Vector2Int.right);
        }
    }

    private void DetectSwipe(Vector2 delta)
    {
        Debug.LogFormat("Swipe detected: {0}, {1}", delta.x, delta.y);
        // 主要方向是水平的，且距离够长
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > minSwipeDistance)
        {
            TileInputEvent.Trigger(delta.x > 0 ? Vector2Int.right : Vector2Int.left);
        }
        else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && Mathf.Abs(delta.y) > minSwipeDistance)
        {
            TileInputEvent.Trigger(delta.y > 0 ? Vector2Int.up : Vector2Int.down);
        }
    }
}
