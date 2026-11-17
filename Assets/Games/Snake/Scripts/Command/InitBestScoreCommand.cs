using UnityEngine;

namespace SnakeCommands
{
    public struct InitBestScoreCommand : ICommand
    {
        public void Excute()
        {
            int bestScore = PlayerPrefs.GetInt("snake_hiscore", 0);
            var snakeModel = SnakeGame.Get<SnakeModel>();
            snakeModel.BestScore.Value = bestScore;
        }
    }
}
