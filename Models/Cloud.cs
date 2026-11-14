using System.Drawing;

namespace YMH219.FlappyBird.Models
{
    /// <summary>
    /// Arka planda paralaks hızıyla hareket eden GDI+ dekoratif bulut nesnesi.
    /// </summary>
    public sealed class Cloud : GameObject
    {
        private readonly float _speed;
        private readonly int _canvasWidth;

        public Cloud(float x, float y, float width, float speed, int canvasWidth)
            : base(x, y, width, width * 0.45f)
        {
            _speed = speed;
            _canvasWidth = canvasWidth;
        }

        public override void Update(float deltaTime)
        {
            X -= _speed * deltaTime;
            if (X + Width < -20f)
            {
                X = _canvasWidth + 20f;
            }
        }

        public override void Draw(Graphics graphics)
        {
            using var cloudBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
            graphics.FillEllipse(cloudBrush, X, Y + Height * 0.25f, Width * 0.55f, Height * 0.7f);
            graphics.FillEllipse(cloudBrush, X + Width * 0.22f, Y, Width * 0.55f, Height * 0.85f);
            graphics.FillEllipse(cloudBrush, X + Width * 0.48f, Y + Height * 0.28f, Width * 0.52f, Height * 0.68f);
        }
    }
}
