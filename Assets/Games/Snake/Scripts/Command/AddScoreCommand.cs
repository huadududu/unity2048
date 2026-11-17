using UnityEngine;

namespace SnakeCommands
{
    public struct AddScoreCommand : ICommand
    {
        public void Excute()
        {
            var snakeModel = SnakeGame.Get<SnakeModel>();
            snakeModel.Score.Value += 1;
            if (snakeModel.Score.Value > snakeModel.BestScore.Value)
            {
                snakeModel.BestScore.Value = snakeModel.Score.Value;
                PlayerPrefs.SetInt("snake_hiscore", snakeModel.BestScore.Value);
            }
        }
    }
}
