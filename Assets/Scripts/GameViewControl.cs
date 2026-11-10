using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameViewControl : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI bestScoreText;
    public GameObject continueNode;

    public void NewGame()
    {

        new ResetScoreCommand().Excute();
        SetScore(GameApp.Get<GameModel>().Score.Value);
        SetBestScore(GameApp.Get<GameModel>().BestScore.Value);
        board.NewGame();
        board.enabled = true;
        gameOver.gameObject.SetActive(false);
    }

    //继续游戏
    public void ContinueGame()
    {
        continueNode.SetActive(false);
        board.ClearLowTile();
        gameOver.interactable = false;
        gameOver.gameObject.SetActive(false);
        board.enabled = true;

    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;
        gameOver.alpha = 0f;
        gameOver.gameObject.SetActive(true);
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
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;

    }
    private void SetScore(int score)
    {
        txtScore.text = score.ToString();
    }
    private void SetBestScore(int score)
    {
        bestScoreText.text = score.ToString();
    }

    // Start is called before the first frame update
    // 等 Addressables 异步加载完成后再开始游戏
    private IEnumerator Start()
    {
        yield return board.InitializeAsync();

        new InitBestScoreCommand().Excute();
       
        GameInit();
        if (!board.IsReady)
        {
            Debug.LogError("[GameManager] 棋盘资源未就绪，游戏未启动。");
            yield break;
        }

        NewGame();
    }

    void GameInit()
    {
        GameOverEvent.Register(GameOver);
        GameModel gameModel = GameApp.Get<GameModel>();
        gameModel.Score.OnValueChanged+=SetScore;
        gameModel.BestScore.OnValueChanged+=SetBestScore;
    }
    // Update is called once per frame
    void Update()
    {

    }
    void OnDestroy()
    {
        GameOverEvent.UnRegister(GameOver);
    }   
}
