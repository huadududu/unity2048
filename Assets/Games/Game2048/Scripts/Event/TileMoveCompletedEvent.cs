using UnityEngine;

/// <summary>
/// tile 移动/合并动画结束后由 Tile 广播，携带 tile 节点；
/// TileBoard 订阅并统计在途动画，全部结束后才继续流程（替代定时等待）。
/// </summary>
public class TileMoveCompletedEvent : Event<TileMoveCompletedEvent, GameObject>
{

}
