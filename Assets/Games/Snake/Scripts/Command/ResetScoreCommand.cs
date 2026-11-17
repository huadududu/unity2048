namespace SnakeCommands
{
    public struct ResetScoreCommand : ICommand
    {
        public void Excute()
        {
            SnakeGame.Get<SnakeModel>().Score.Value = 0;
        }
    }
}
