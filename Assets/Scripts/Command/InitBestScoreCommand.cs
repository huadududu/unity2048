public struct InitBestScoreCommand : ICommand
{
    public void Excute()
    {
        int mScore = PlayerPrefs.GetInt("hiscore", 0);
        var gameModel = GameApp.Get<GameModel>();
        gameModel.BestScore.Value = mScore;
    }
}