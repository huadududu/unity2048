using UnityEngine;

/// <summary>
/// 公共方向输入事件：DirectionInput 识别到方向（键盘或触摸滑动）后广播，携带方向向量。
/// 各游戏订阅后按自身规则决定是否响应（2048 移动方块，Snake 过滤反向后转向）。
/// </summary>
public class DirectionInputEvent : Event<DirectionInputEvent, Vector2Int>
{
}
