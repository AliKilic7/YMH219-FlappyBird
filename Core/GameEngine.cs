using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using YMH219.FlappyBird.Interfaces;
using YMH219.FlappyBird.Models;

namespace YMH219.FlappyBird.Core
{
    /// <summary>
    /// Oyun döngüsü, çarpışma algılama, skor ve hız artışını yöneten ana motor.
    /// </summary>
    public sealed class GameEngine
    {
        private const float BasePipeSpeed = 165f;
        private const float SpeedIncrementPerStep = 24f;
        private const int PointsPerSpeedStep = 5;
        private const float PipeWidth = 66f;
        private const float PipeSpacing = 250f;
        private const float BaseGapHeight = 148f;
        private const float GroundHeight = 68f;

        private readonly int _canvasWidth;
        private readonly int _canvasHeight;
        private readonly int _playfieldHeight;
        private readonly Random _random = new();
        private readonly IHighScoreRepository _highScoreRepository;
        private readonly List<PipePair> _pipes = new();
        private readonly List<Cloud> _clouds = new();
        private readonly List<GameObject> _renderables = new();

        private float _groundOffset;

        public Bird PlayerBird { get; }
        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int SpeedLevel => 1 + (Score / PointsPerSpeedStep);
        public float CurrentPipeSpeed => BasePipeSpeed + (Score / PointsPerSpeedStep) * SpeedIncrementPerStep;

        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        public event EventHandler<GameOverEventArgs>? GameOver;
        public event EventHandler<GameStateChangedEventArgs>? StateChanged;

        public GameEngine(int canvasWidth, int canvasHeight, IHighScoreRepository highScoreRepository)
        {
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
            _playfieldHeight = canvasHeight - (int)GroundHeight;
            _highScoreRepository = highScoreRepository ?? throw new ArgumentNullException(nameof(highScoreRepository));

            HighScore = _highScoreRepository.LoadHighScore();
            PlayerBird = new Bird(110f, _playfieldHeight * 0.42f);

            InitializeClouds();
            InitializePipes();
            RebuildRenderList();
        }

        public void StartNewGame()
        {
            Score = 0;
            PlayerBird.Reset();
            ResetPipes();
            SetState(GameState.Playing);
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(Score, HighScore, SpeedLevel));
        }

        public void ResetToReady()
        {
            Score = 0;
            PlayerBird.Reset();
            ResetPipes();
            SetState(GameState.Ready);
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(Score, HighScore, SpeedLevel));
        }

        public void Jump()
        {
            if (State == GameState.Ready)
            {
                StartNewGame();
            }

            if (State == GameState.Playing)
            {
                PlayerBird.Jump();
            }
        }

        public void Update(float deltaTime)
        {
            if (deltaTime <= 0f || deltaTime > 0.1f)
            {
                deltaTime = 0.016f;
            }

            foreach (Cloud cloud in _clouds)
            {
                cloud.Update(deltaTime);
            }

            if (State != GameState.Playing)
            {
                return;
            }

            _groundOffset = (_groundOffset + CurrentPipeSpeed * deltaTime) % 36f;
            PlayerBird.Update(deltaTime);

            float currentGap = Math.Max(112f, BaseGapHeight - (SpeedLevel - 1) * 5f);
            float rightMostX = _pipes.Max(p => p.X);

            foreach (PipePair pipe in _pipes)
            {
                pipe.ScrollSpeed = CurrentPipeSpeed;
                pipe.Update(deltaTime);

                if (!pipe.IsPassed && pipe.X + pipe.Width < PlayerBird.X)
                {
                    pipe.IsPassed = true;
                    IncrementScore();
                }

                if (pipe.X + pipe.Width < -10f)
                {
                    float nextX = Math.Max(_canvasWidth + 20f, rightMostX + PipeSpacing);
                    rightMostX = nextX;
                    pipe.Reposition(nextX, GenerateRandomGapY(currentGap), currentGap);
                }
            }

            CheckCollisions();
        }

        public void Render(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            DrawSkyBackground(graphics);

            // Polimorfizm: tüm GameObject örnekleri ortak Draw(Graphics) arayüzüyle çizilir.
            foreach (GameObject entity in _renderables)
            {
                entity.Draw(graphics);
            }

            DrawGround(graphics);
        }

        private void InitializeClouds()
        {
            _clouds.Clear();
            _clouds.Add(new Cloud(60f, 55f, 92f, 22f, _canvasWidth));
            _clouds.Add(new Cloud(280f, 95f, 115f, 16f, _canvasWidth));
            _clouds.Add(new Cloud(520f, 45f, 84f, 26f, _canvasWidth));
        }

        private void InitializePipes()
        {
            _pipes.Clear();
            for (int i = 0; i < 3; i++)
            {
                float startX = _canvasWidth + 60f + i * PipeSpacing;
                _pipes.Add(new PipePair(
                    startX,
                    PipeWidth,
                    _playfieldHeight,
                    GenerateRandomGapY(BaseGapHeight),
                    BaseGapHeight,
                    BasePipeSpeed));
            }
        }

        private void ResetPipes()
        {
            for (int i = 0; i < _pipes.Count; i++)
            {
                _pipes[i].ScrollSpeed = BasePipeSpeed;
                _pipes[i].Reposition(_canvasWidth + 60f + i * PipeSpacing, GenerateRandomGapY(BaseGapHeight), BaseGapHeight);
            }
        }

        private void RebuildRenderList()
        {
            _renderables.Clear();
            _renderables.AddRange(_clouds);
            _renderables.AddRange(_pipes);
            _renderables.Add(PlayerBird);
        }

        private float GenerateRandomGapY(float gapHeight)
        {
            int minY = 55;
            int maxY = Math.Max(minY + 20, _playfieldHeight - (int)gapHeight - 55);
            return _random.Next(minY, maxY);
        }

        private void IncrementScore()
        {
            Score++;
            if (Score > HighScore)
            {
                HighScore = Score;
                _highScoreRepository.SaveHighScore(HighScore);
            }

            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(Score, HighScore, SpeedLevel));
        }

        private void CheckCollisions()
        {
            RectangleF birdBounds = PlayerBird.Bounds;

            if (birdBounds.Bottom >= _playfieldHeight)
            {
                TriggerGameOver();
                return;
            }

            foreach (PipePair pipe in _pipes)
            {
                if (pipe.IntersectsWith(birdBounds))
                {
                    TriggerGameOver();
                    return;
                }
            }
        }

        private void TriggerGameOver()
        {
            bool isNewRecord = Score > 0 && Score >= HighScore;
            if (Score > HighScore)
            {
                HighScore = Score;
                _highScoreRepository.SaveHighScore(HighScore);
                isNewRecord = true;
            }

            SetState(GameState.GameOver);
            GameOver?.Invoke(this, new GameOverEventArgs(Score, HighScore, isNewRecord));
        }

        private void SetState(GameState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(newState));
        }

        private void DrawSkyBackground(Graphics graphics)
        {
            using var skyBrush = new LinearGradientBrush(
                new Rectangle(0, 0, _canvasWidth, _playfieldHeight),
                Color.FromArgb(105, 196, 245),
                Color.FromArgb(196, 236, 255),
                LinearGradientMode.Vertical);
            graphics.FillRectangle(skyBrush, 0, 0, _canvasWidth, _playfieldHeight);
        }

        private void DrawGround(Graphics graphics)
        {
            using var soilBrush = new SolidBrush(Color.FromArgb(218, 198, 134));
            using var grassBrush = new SolidBrush(Color.FromArgb(95, 186, 55));
            using var stripeBrush = new SolidBrush(Color.FromArgb(72, 156, 38));
            using var borderPen = new Pen(Color.FromArgb(62, 112, 28), 2f);

            graphics.FillRectangle(soilBrush, 0, _playfieldHeight, _canvasWidth, GroundHeight);
            graphics.FillRectangle(grassBrush, 0, _playfieldHeight, _canvasWidth, 16f);

            for (float x = -_groundOffset; x < _canvasWidth + 36f; x += 36f)
            {
                PointF[] stripe =
                {
                    new PointF(x, _playfieldHeight),
                    new PointF(x + 18f, _playfieldHeight),
                    new PointF(x + 8f, _playfieldHeight + 16f),
                    new PointF(x - 10f, _playfieldHeight + 16f)
                };
                graphics.FillPolygon(stripeBrush, stripe);
            }

            graphics.DrawLine(borderPen, 0, _playfieldHeight, _canvasWidth, _playfieldHeight);
        }
    }
}
