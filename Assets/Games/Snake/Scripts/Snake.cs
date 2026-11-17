using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public Transform segmentPrefab;
    public Vector2Int direction = Vector2Int.right;
    public float speed = 60f;
    public int speedLevel = 1;
    public int initSize = 4;
    public bool MoveThroughtWalls = true;
    private List<Transform> segments = new List<Transform>{};
    private Vector2Int input;
    private float nextUpdate;// 下一次判定移动更新时间

    private void OnTriggerEnter2D(Collider2D other){
        // Debug.LogFormat("{0}, {1}", other.gameObject.name, other.gameObject.CompareTag("Wall"));
        if(other.gameObject.CompareTag("Food")){
            Grow();
            new SnakeCommands.AddScoreCommand().Excute();
        }else if (other.gameObject.CompareTag("Obstacle")){
            //碰到障碍物
            ResetGame();
        }else if (other.gameObject.CompareTag("Wall")){
            //碰到墙壁
            if(MoveThroughtWalls){
                MoveThroughtWall(other.transform);
            }else{
                ResetGame();
            }
        }
    }

    // 重启游戏
    public void ResetGame(){
        // todoz
        new SnakeCommands.ResetScoreCommand().Excute();
        direction = Vector2Int.right;
        transform.position = Vector3.zero;
        //去掉蛇头
        for(int i = 1; i< segments.Count; i++){
            Destroy(segments[i].gameObject);
        }
        segments.Clear();
        segments.Add(transform);
        for(int i = 0; i< initSize -1; i++){
            Grow();
        }
    }
    // 穿墙操作
    private void MoveThroughtWall(Transform wall){
        Vector3 position = transform.position;
        // Debug.LogFormat("cross wall:{0}, {1}", wall.position.x, direction.x);
        if(direction.x != 0f){
            position.x = Mathf.RoundToInt(-wall.position.x + direction.x);
        }else if(direction.y != 0f){
            Debug.LogFormat("cross wall:{0}, {1}", wall.position.y, direction.y);
            position.y = Mathf.RoundToInt(-wall.position.y + direction.y);
        }
        transform.position = position;
    }

    // 蛇长身体
    public void Grow(){
        Transform segment = Instantiate(segmentPrefab);
        segment.position = segments[^1].position;
        segments.Add(segment);
    }

    // 坐标是否被蛇身体占用
    public bool Occupies(int x, int y){
        foreach (var seg in segments){
          if (x == Mathf.RoundToInt(seg.position.x) && 
            y == Mathf.RoundToInt(seg.position.y)){
                return true;
            }
        }        
        return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        ResetGame();
        
    }

    // 公共输入事件：横向移动的时候只接受上下，纵向移动的时候只接受左右（防止180度掉头）
    private void OnDirectionInput(Vector2Int dir){
        if(direction.x != 0 && dir.y != 0){
            input = dir;
        }else if(direction.y != 0 && dir.x != 0){
            input = dir;
        }
    }

    private void Awake(){
        DirectionInputEvent.Register(OnDirectionInput);
    }

    private void OnDestroy(){
        DirectionInputEvent.UnRegister(OnDirectionInput);
    }
    private void FixedUpdate(){
        if(Time.time < nextUpdate){
            return;
        }

        if(input != Vector2Int.zero){
            direction = input;
        }
        for(int i = segments.Count -1; i > 0; i--){
            segments[i].position = segments[i-1].position;
        }
        int x = Mathf.RoundToInt(transform.position.x) + direction.x;
        int y= Mathf.RoundToInt(transform.position.y) + direction.y;
        transform.position = (Vector3)new Vector2(x, y);
        nextUpdate = Time.time + (1f/(speed + speedLevel));
    }

}