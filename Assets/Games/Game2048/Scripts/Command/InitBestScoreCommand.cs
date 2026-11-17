using UnityEngine;

namespace Game2048Commands
{
    public struct InitBestScoreCommand : ICommand
    {
        public void Excute()
        {
            int mScore = PlayerPrefs.GetInt("hiscore", 0);
            var gameModel = Game2048.Get<GameModel>();
            gameModel.BestScore.Value = mScore;
        }
    }
}
