using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{

    // public GameManager GameManager;
    public Tile tilePrefab;
    public TileState[] tileStates;

    public TileGrid grid;
    private List<Tile> tiles = new List<Tile>();
    private bool waiting = false;

    public void ClearBoard(){
        foreach( var cell in grid.cells){
            cell.tile = null;
        }

        foreach(var tile in tiles){
            Destroy(tile.gameObject);
        }
        tiles.Clear();
    }

    public void CreateTile(){
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[Random.Range(0, 2)]); // 教程更新
        tile.LinkCell(grid.getRandomEmptyCell());
        tiles.Add(tile);
    }


    // Start is called before the first frame update
    // void Start()
    // {
        // 
    // }

    // // Update is called once per frame
    private void Update()
    {
        if(waiting){
            return;
        }
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)){
            Move(Vector2Int.up, 0, 1, 1, 1);
        }else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)){
            Move(Vector2Int.left, 1, 1, 0, 1);
        }else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)){
            Move(Vector2Int.down, 0, 1, grid.height - 2 , -1);
        }else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)){
            Move(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
    }
    
    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY){
        bool changed = false;
        for(int x = startX; x >=0 && x < grid.width; x+=incrementX){
            for(int y = startY; y >=0 && y < grid.height; y+= incrementY){
                TileCell cell = grid.GetCell(x, y);
                if(cell.isOccupied()){
                    changed |= MoveTile(cell.tile, direction);
                }

            }
        }   
        if(changed){
            waiting = true;
            StartCoroutine(WaitForChanges());
        }
    }

    private IEnumerator WaitForChanges(){
    
        yield return new WaitForSeconds(0.1f);

        foreach(var tile in tiles){
            tile.locked = false;
        }

        if(tiles.Count != grid.size){
            CreateTile();
        }

        waiting = false;
        if(CheckForGameOver()){
            GameManager.Instance.GameOver();
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction){
        TileCell newCell = null;
        TileCell adjcent = grid.GetAdjacentCell(tile.cell, direction);

        while(adjcent != null){
           if(adjcent.isOccupied()){
               if(CanMerge(tile, adjcent.tile)){
                   MergeTiles(tile, adjcent.tile);
                   return true;
               }
               break;
           }
           newCell = adjcent;
           adjcent = grid.GetAdjacentCell(adjcent, direction);
        }
        if(newCell != null){
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool CanMerge(Tile a, Tile b){
        return a.state == b.state && !b.locked;
    }

    private void MergeTiles(Tile a, Tile b){
        tiles.Remove(a);
        a.Merge(b.cell);
        int index = b.state.index;
        // int index = Mathf.Clamp(b.state.index, 0, tileStates.Length - 1);
        // int index = Mathf.Clamp(tileStates.IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        TileState newState = tileStates[index];

        b.SetState(newState);

        GameManager.Instance.IncreaseScore(newState.number);
        // GameManager.Instance.IncreaseScore(Mathf.Pow(2, newState.index));
    }

    public bool CheckForGameOver(){

        if(tiles.Count != grid.size){
            return false;
        }
        foreach(var tile in tiles){
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);
            if(up != null && CanMerge(tile, up.tile)){
                return false;
            }
            if(down != null && CanMerge(tile, down.tile)){
                return false;
            }
            if(left != null && CanMerge(tile, left.tile)){
                return false;
            }
            if(right != null && CanMerge(tile, right.tile)){
                return false;
            }
        }
        return true;
    }
}
