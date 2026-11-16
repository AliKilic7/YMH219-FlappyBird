using System;
using System.IO;
using YMH219.FlappyBird.Interfaces;

namespace YMH219.FlappyBird.Services
{
    /// <summary>
    /// En yüksek skoru uygulama dizinindeki highscore.txt dosyasında saklar.
    /// </summary>
    public sealed class FileHighScoreRepository : IHighScoreRepository
    {
        private readonly string _filePath;

        public FileHighScoreRepository(string? filePath = null)
        {
            _filePath = string.IsNullOrWhiteSpace(filePath)
                ? Path.Combine(AppContext.BaseDirectory, "highscore.txt")
                : filePath;
        }

        public int LoadHighScore()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    File.WriteAllText(_filePath, "0");
                    return 0;
                }

                string content = File.ReadAllText(_filePath).Trim();
                return int.TryParse(content, out int score) && score >= 0 ? score : 0;
            }
            catch
            {
                return 0;
            }
        }

        public void SaveHighScore(int score)
        {
            try
            {
                string? directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_filePath, Math.Max(0, score).ToString());
            }
            catch
            {
                // Dosya izin hatası oyunu kesintiye uğratmamalıdır.
            }
        }
    }
}
