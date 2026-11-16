namespace YMH219.FlappyBird.Interfaces
{
    /// <summary>
    /// En yüksek skor kalıcılığını soyutlayan depo arayüzü.
    /// </summary>
    public interface IHighScoreRepository
    {
        int LoadHighScore();
        void SaveHighScore(int score);
    }
}
