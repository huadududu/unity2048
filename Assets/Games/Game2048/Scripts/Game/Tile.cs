using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Tile : MonoBehaviour
{

    [HideInInspector]
    public TileState state;// 状态

    [HideInInspector]
    public TileCell cell;

    [HideInInspector]
    public bool locked;

    public Image imgBackground;
    public TextMeshProUGUI txtNumber;


    public void SetState(TileState state)
    {
        this.state = state;
        imgBackground.color = state.backgroundColor;
        txtNumber.color = state.textColor;
        txtNumber.text = Mathf.Pow(2, state.index).ToString();
    }

    /// <summary>
    /// 将 tile 链接到单元格
    /// 优化：移动后更新空单元格缓存
    /// </summary>
    public void LinkCell(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }
        this.cell = cell;
        this.cell.tile = this;
        transform.position = cell.transform.position;
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(MoveAnimate(cell.transform.position, false));

    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }
        this.cell = null;
        cell.tile.locked = true;
        StartCoroutine(MoveAnimate(cell.transform.position, true));

    }

    private IEnumerator MoveAnimate(Vector3 to, bool merging)
    {

        float elapsed = 0f;
        float duration = 0.1f;
        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
        if (merging)
        {
            TileRecycleEvent.Trigger(gameObject);
        }
        TileMoveCompletedEvent.Trigger(gameObject);
    }

    /// <summary>
    /// 出生表现：从 0 弹性放大到 1，纯视觉不影响流程
    /// </summary>
    public void Spawn()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(SpawnAnimate());
    }

    /// <summary>
    /// 合并升级后的表现：放大再回落，纯视觉不影响流程
    /// </summary>
    public void Pop()
    {
        StartCoroutine(PopAnimate());
    }

    private IEnumerator SpawnAnimate()
    {
        const float duration = 0.15f;
        // easeOutBack：先冲过 1 再回落，出生感更有弹性
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            transform.localScale = Vector3.one * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private IEnumerator PopAnimate()
    {
        const float duration = 0.15f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.one * (1f + 0.2f * Mathf.Sin(t * Mathf.PI));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
