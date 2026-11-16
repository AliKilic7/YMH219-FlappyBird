using System;

namespace YMH219.FlappyBird.Core
{
    public sealed class ScoreChangedEventArgs : EventArgs
    {
        public int Score { get; }
        public int HighScore { get; }
        public int SpeedLevel { get; }

        public ScoreChangedEventArgs(int score, int highScore, int speedLevel)
        {
            Score = score;
            HighScore = highScore;
            SpeedLevel = speedLevel;
        }
    }

    public sealed class GameOverEventArgs : EventArgs
    {
        public int FinalScore { get; }
        public int HighScore { get; }
        public bool IsNewRecord { get; }

        public GameOverEventArgs(int finalScore, int highScore, bool isNewRecord)
        {
            FinalScore = finalScore;
            HighScore = highScore;
            IsNewRecord = isNewRecord;
        }
    }

    public sealed class GameStateChangedEventArgs : EventArgs
    {
        public GameState NewState { get; }

        public GameStateChangedEventArgs(GameState newState)
        {
            NewState = newState;
        }
    }
}
