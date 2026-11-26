using System;
using System.IO;
using System.Threading;
using YMH219.FlappyBird.Interfaces;

namespace YMH219.FlappyBird.Services
{
    /// <summary>
    /// En yüksek skoru highscore.txt dosyasında kalıcı olarak saklar.
    /// Geliştirme ortamında proje kök dizinini, dağıtım ortamında uygulama dizinini kullanır.
    /// </summary>
    public sealed class FileHighScoreRepository : IHighScoreRepository
    {
        private readonly string _primaryFilePath;
        private readonly string _fallbackFilePath;

        public FileHighScoreRepository(string? filePath = null)
        {
            _fallbackFilePath = Path.Combine(AppContext.BaseDirectory, "highscore.txt");

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _primaryFilePath = filePath;
            }
            else
            {
                string? projectDir = FindProjectDirectory();
                _primaryFilePath = projectDir != null
                    ? Path.Combine(projectDir, "highscore.txt")
                    : _fallbackFilePath;
            }
        }

        public int LoadHighScore()
        {
            int primaryScore = TryReadScore(_primaryFilePath);
            int fallbackScore = string.Equals(_primaryFilePath, _fallbackFilePath, StringComparison.OrdinalIgnoreCase)
                ? 0
                : TryReadScore(_fallbackFilePath);

            int bestScore = Math.Max(primaryScore, fallbackScore);

            if (!File.Exists(_primaryFilePath) || bestScore > primaryScore)
            {
                SaveHighScore(bestScore);
            }

            return bestScore;
        }

        public void SaveHighScore(int score)
        {
            int sanitized = Math.Max(0, score);
            WriteWithRetry(_primaryFilePath, sanitized.ToString());

            if (!string.Equals(_primaryFilePath, _fallbackFilePath, StringComparison.OrdinalIgnoreCase))
            {
                WriteWithRetry(_fallbackFilePath, sanitized.ToString());
            }
        }

        private static string? FindProjectDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory();
            if (Directory.GetFiles(currentDir, "*.csproj").Length > 0)
            {
                return currentDir;
            }

            DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 4 && dir != null; i++)
            {
                if (dir.GetFiles("*.csproj").Length > 0)
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }

        private static int TryReadScore(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return 0;
                }

                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd().Trim();
                return int.TryParse(content, out int score) && score >= 0 ? score : 0;
            }
            catch
            {
                return 0;
            }
        }

        private static void WriteWithRetry(string path, string content)
        {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    string? directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    using var writer = new StreamWriter(stream);
                    writer.Write(content);
                    writer.Flush();
                    return;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    Thread.Sleep(30);
                }
                catch
                {
                    return;
                }
            }
        }
    }
}
