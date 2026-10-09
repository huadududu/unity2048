using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{

    private static GameManager _instance;

    public static GameManager Instance{
        get{
            if (_instance == null){
                _instance = FindAnyObjectByType<GameManager>();
            }
            return _instance;
        }
    }
    public TileBoard board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI bestScoreText;
    private int score = 0;


    public void NewGame(){
        SetScore(0);

        bestScoreText.text = LoadHiScore().ToString();
        
        gameOver.alpha = 0f;
        gameOver.interactable = false;
        // gameOver.enabled = false;
        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
        Debug.Log("test start");
        //test
    }

    public void GameOver(){
        board.enabled = false;
        gameOver.interactable = true;
        PlayerPrefs.Save();
        Debug.Log("test gameover");
        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay){
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while(elapsed < duration){
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;

    }

    public void IncreaseScore(int point){
        SetScore(score + point);
    }

    private void SetScore(int score){
        this.score = score;
        txtScore.text = score.ToString();
        SaveHiscore();
    }

    private void SaveHiscore(){
        int hiscore = LoadHiScore();
        if(score > hiscore){
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    private int LoadHiScore(){
        return PlayerPrefs.GetInt("hiscore", 0);

    }

    // Start is called before the first frame update
    void Start()
    {
        NewGame();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
