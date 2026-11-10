using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCell : MonoBehaviour
{

    public Vector2Int coordinates;//坐标
    public Tile tile;//关联的tile

    public bool IsEmpty()
    {
        return tile == null;
    }

    public bool isOccupied()
    {
        return tile != null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
