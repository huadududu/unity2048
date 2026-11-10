
using UnityEngine;

public struct AddScoreCommand : ICommand
{
    private int mScore;
    public AddScoreCommand(int index)
    {
        int score = (int)Mathf.Pow(2, index);
        mScore = score;
        if (index >= 8)
        {
            var gameModel = GameApp.Get<GameModel>();
            gameModel.MaxTileIndex.Value = index - 5;
        }
    }
    public void Excute()
    {
        var gameModel = GameApp.Get<GameModel>();
        Debug.Log("AddScoreCommand Excute mScore = " + mScore);
        gameModel.Score.Value += mScore;
        if (gameModel.Score.Value > gameModel.BestScore.Value)
        {
            gameModel.BestScore.Value = gameModel.Score.Value;
            PlayerPrefs.SetInt("hiscore", gameModel.BestScore.Value);
        }
    }
}