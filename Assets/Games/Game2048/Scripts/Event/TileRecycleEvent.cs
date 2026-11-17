using UnityEngine;

/// <summary>
/// tile 合并动画结束后由 Tile 广播，携带 tile 节点；
/// TileBoard 订阅此事件并把节点回收到对象池。
/// </summary>
public class TileRecycleEvent : Event<TileRecycleEvent, GameObject>
{

}
