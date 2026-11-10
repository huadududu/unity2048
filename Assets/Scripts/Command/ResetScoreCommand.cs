public struct ResetScoreCommand : ICommand
{
    public void Excute()
    {
        var gameModel = GameApp.Get<GameModel>();
        gameModel.Score.Value = 0;
    }
}
