using UnityEngine;

/// <summary>
/// 输入层识别到移动方向后广播，携带方向向量；
/// TileBoard 订阅后执行移动，是否响应由棋盘自身状态决定。
/// </summary>
public class TileInputEvent : Event<TileInputEvent, Vector2Int>
{

}
