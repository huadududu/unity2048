using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameManager>();
            }
            return _instance;
        }
    }
    public TileBoard board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI bestScoreText;
    public GameObject continueNode;
    public int score { get; private set; }


    private int maxStateIndex = 0;



    public void NewGame()
    {
        SetScore(0);
        // UpdateMaxStateIndex(2);
        this.maxStateIndex = 2;

        bestScoreText.text = LoadHiScore().ToString();

        gameOver.alpha = 0f;
        gameOver.interactable = false;
        // gameOver.enabled = false;
        board.ClearBoard();
        // board.CreateTile(0, 0, 0);
        // board.CreateTile(2, 0, 0);
        // board.CreateTile(3, 0, 1);
        board.CreateTile();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
        // continueNode.SetActive(true);
        // Debug.Log("test start");
        //test
    }

    //继续游戏
    public void ContinueGame()
    {
        continueNode.SetActive(false);
        board.ClearLowTile();
        gameOver.interactable = false;
        gameOver.alpha = 0f;
        board.enabled = true;

    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;
        PlayerPrefs.Save();
        // Debug.Log("test gameover");
        StartCoroutine(Fade(gameOver, 1f, 1f));
        continueNode.SetActive(true);
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            print(canvasGroup.alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;

    }

    public void UpdateMaxStateIndex(int stateIndex)
    {
        this.maxStateIndex = Mathf.Max(this.maxStateIndex, stateIndex);
    }

    public int getCreateStateIndex()
    {
        if (this.maxStateIndex >= 8)
        {
            return this.maxStateIndex - 5;
        }
        else
        {
            return 2;
        }
    }
    public void IncreaseScore(int point)
    {
        SetScore(score + point);
    }

    private void SetScore(int score)
    {
        this.score = score;
        txtScore.text = score.ToString();
        SaveHiscore();
    }

    private void SaveHiscore()
    {
        int hiscore = LoadHiScore();
        if (score > hiscore)
        {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    private int LoadHiScore()
    {
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
