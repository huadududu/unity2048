using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Food : MonoBehaviour
{

    public Collider2D gameArea;
    private Snake snake;

    private void OnTriggerEnter2D(Collider2D other){
        RandomPosition();
    }
    private void Awake(){
        snake = FindObjectOfType<Snake>();
    }

    //随机生成一个位置
    public void RandomPosition(){
        Bounds bound = gameArea.bounds;
        Debug.LogFormat("bound:{0}, {1}, {2}, {3}", bound.min.x, bound.max.x, bound.min.y, bound.max.y);
        int x = Mathf.RoundToInt(Random.Range(bound.min.x, bound.max.x));
        int y = Mathf.RoundToInt(Random.Range(bound.min.y, bound.max.y));
        while(snake.Occupies(x, y)){
            x++;
            if(x > bound.max.x){
                x = Mathf.RoundToInt(bound.min.x);
                y++;
                if(y > bound.max.y){
                    y = Mathf.RoundToInt(bound.min.y);
                }
            }
        }
        transform.position = new Vector2(x, y);
    }
    // Start is called before the first frame update
    void Start()
    {   
        RandomPosition();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
