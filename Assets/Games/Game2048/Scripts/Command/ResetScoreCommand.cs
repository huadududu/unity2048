namespace Game2048Commands
{
    public struct ResetScoreCommand : ICommand
    {
        public void Excute()
        {
            var gameModel = Game2048.Get<GameModel>();
            gameModel.Score.Value = 0;
        }
    }
}
