using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using YMH219.FlappyBird.Core;
using YMH219.FlappyBird.Services;
using YMH219.FlappyBird.UI;

namespace YMH219.FlappyBird
{
    /// <summary>
    /// Çift tamponlu GDI+ tuvalini, klavye olaylarını ve HUD bileşenlerini yöneten ana form.
    /// </summary>
    public sealed class MainForm : Form
    {
        private const int CanvasWidth = 640;
        private const int CanvasHeight = 500;

        private readonly DoubleBufferedPanel _canvas;
        private readonly Label _lblScore;
        private readonly Label _lblHighScore;
        private readonly Label _lblSpeed;
        private readonly Button _btnRestart;
        private readonly System.Windows.Forms.Timer _gameTimer;
        private readonly Stopwatch _frameWatch = new();
        private readonly GameEngine _engine;

        public MainForm()
        {
            Text = "YMH 219 - Flappy Bird (OOP & GDI+)";
            ClientSize = new Size(CanvasWidth, CanvasHeight + 52);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            DoubleBuffered = true;

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(12, 8, 12, 8)
            };

            _lblScore = CreateHudLabel("Skor: 0", 12);
            _lblHighScore = CreateHudLabel("En Yüksek: 0", 150);
            _lblSpeed = CreateHudLabel("Hız: 1x", 320);

            _btnRestart = new Button
            {
                Text = "Yeni Oyun / Yeniden Başlat (R)",
                Width = 210,
                Height = 34,
                Location = new Point(CanvasWidth - 222, 9),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false,
                Cursor = Cursors.Hand
            };
            _btnRestart.FlatAppearance.BorderSize = 0;
            _btnRestart.Click += (_, _) => RestartGame();

            topBar.Controls.Add(_lblScore);
            topBar.Controls.Add(_lblHighScore);
            topBar.Controls.Add(_lblSpeed);
            topBar.Controls.Add(_btnRestart);

            _engine = new GameEngine(CanvasWidth, CanvasHeight, new FileHighScoreRepository());

            _canvas = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.SkyBlue
            };
            _canvas.Paint += Canvas_Paint;
            _canvas.MouseDown += (_, _) => _engine.Jump();

            _engine.ScoreChanged += Engine_ScoreChanged;
            _engine.GameOver += Engine_GameOver;
            _engine.StateChanged += (_, _) => _canvas.Invalidate();

            Controls.Add(_canvas);
            Controls.Add(topBar);

            UpdateHud(_engine.Score, _engine.HighScore, _engine.SpeedLevel);

            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _gameTimer.Tick += GameTimer_Tick;
            _frameWatch.Start();
            _gameTimer.Start();
        }

        private static Label CreateHudLabel(string text, int x)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, 15),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Space:
                case Keys.Up:
                    _engine.Jump();
                    return true;
                case Keys.R:
                    RestartGame();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RestartGame()
        {
            try
            {
                _engine.StartNewGame();
                _frameWatch.Restart();
                _canvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oyun başlatılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            float dt = (float)_frameWatch.Elapsed.TotalSeconds;
            _frameWatch.Restart();

            _engine.Update(dt);
            _canvas.Invalidate();
        }

        private void Engine_ScoreChanged(object? sender, ScoreChangedEventArgs e)
        {
            UpdateHud(e.Score, e.HighScore, e.SpeedLevel);
        }

        private void Engine_GameOver(object? sender, GameOverEventArgs e)
        {
            UpdateHud(e.FinalScore, e.HighScore, _engine.SpeedLevel);
            _canvas.Invalidate();
        }

        private void UpdateHud(int score, int highScore, int speedLevel)
        {
            _lblScore.Text = $"Skor: {score}";
            _lblHighScore.Text = $"En Yüksek: {highScore}";
            _lblSpeed.Text = $"Hız: {speedLevel}x";
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            _engine.Render(e.Graphics);

            if (_engine.State == GameState.Ready)
            {
                DrawOverlay(
                    e.Graphics,
                    "HAZIR MISIN?",
                    "Zıplamak için SPACE / YUKARI OK veya tıkla\nHer 5 skorda hız artar!");
            }
            else if (_engine.State == GameState.GameOver)
            {
                DrawOverlay(
                    e.Graphics,
                    "GAME OVER",
                    $"Skorunuz: {_engine.Score}   |   En Yüksek: {_engine.HighScore}\nYeniden başlatmak için R tuşuna basın veya butona tıklayın");
            }
        }

        private static void DrawOverlay(Graphics graphics, string title, string subtitle)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var dimBrush = new SolidBrush(Color.FromArgb(135, 15, 23, 42));
            graphics.FillRectangle(dimBrush, 0, 0, CanvasWidth, CanvasHeight);

            var box = new RectangleF(CanvasWidth / 2f - 210f, CanvasHeight / 2f - 85f, 420f, 160f);
            using var cardBrush = new SolidBrush(Color.FromArgb(245, 255, 255, 255));
            using var borderPen = new Pen(Color.FromArgb(51, 65, 85), 3f);
            graphics.FillRectangle(cardBrush, box);
            graphics.DrawRectangle(borderPen, box.X, box.Y, box.Width, box.Height);

            using var titleFont = new Font("Segoe UI", 22f, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 10.5f, FontStyle.Regular);
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(
                title,
                titleFont,
                title == "GAME OVER" ? Brushes.Crimson : Brushes.DarkSlateBlue,
                new RectangleF(box.X, box.Y + 14f, box.Width, 46f),
                sf);

            graphics.DrawString(
                subtitle,
                subFont,
                Brushes.DimGray,
                new RectangleF(box.X + 16f, box.Y + 62f, box.Width - 32f, 82f),
                sf);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameTimer.Stop();
                _gameTimer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
