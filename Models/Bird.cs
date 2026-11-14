using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace YMH219.FlappyBird.Models
{
    /// <summary>
    /// Yer çekimi ve zıplama fiziğine sahip oyuncu kuş varlığı.
    /// </summary>
    public sealed class Bird : GameObject
    {
        private const float DefaultGravity = 1350f;
        private const float DefaultJumpImpulse = -410f;
        private const float MaxFallSpeed = 620f;

        private float _velocityY;
        private float _wingPhase;
        private readonly float _startX;
        private readonly float _startY;

        public float VelocityY => _velocityY;
        public float Gravity { get; set; } = DefaultGravity;
        public float JumpImpulse { get; set; } = DefaultJumpImpulse;

        public override RectangleF Bounds => new RectangleF(X + 4f, Y + 4f, Width - 8f, Height - 8f);

        public Bird(float startX, float startY, float size = 36f)
            : base(startX, startY, size, size * 0.82f)
        {
            _startX = startX;
            _startY = startY;
        }

        public void Reset()
        {
            X = _startX;
            Y = _startY;
            _velocityY = 0f;
            _wingPhase = 0f;
        }

        public void Jump()
        {
            _velocityY = JumpImpulse;
        }

        public override void Update(float deltaTime)
        {
            _velocityY = Math.Min(_velocityY + Gravity * deltaTime, MaxFallSpeed);
            Y += _velocityY * deltaTime;

            if (Y < 0f)
            {
                Y = 0f;
                _velocityY = 0f;
            }

            _wingPhase += deltaTime * 14f;
        }

        public override void Draw(Graphics graphics)
        {
            GraphicsState state = graphics.Save();
            float centerX = X + Width / 2f;
            float centerY = Y + Height / 2f;
            float angle = Math.Clamp(_velocityY * 0.085f, -28f, 65f);

            graphics.TranslateTransform(centerX, centerY);
            graphics.RotateTransform(angle);
            graphics.TranslateTransform(-centerX, -centerY);

            // Gövde
            using (var bodyBrush = new LinearGradientBrush(
                new RectangleF(X, Y, Width, Height),
                Color.FromArgb(255, 224, 60),
                Color.FromArgb(245, 166, 35),
                LinearGradientMode.Vertical))
            using (var outlinePen = new Pen(Color.FromArgb(120, 72, 10), 2f))
            {
                graphics.FillEllipse(bodyBrush, X, Y, Width, Height);
                graphics.DrawEllipse(outlinePen, X, Y, Width, Height);
            }

            // Kanat (animasyonlu)
            float wingOffsetY = (float)Math.Sin(_wingPhase) * 3.2f;
            using (var wingBrush = new SolidBrush(Color.FromArgb(255, 245, 185)))
            using (var wingPen = new Pen(Color.FromArgb(140, 90, 20), 1.5f))
            {
                var wingRect = new RectangleF(X + Width * 0.18f, Y + Height * 0.36f + wingOffsetY, Width * 0.44f, Height * 0.36f);
                graphics.FillEllipse(wingBrush, wingRect);
                graphics.DrawEllipse(wingPen, wingRect);
            }

            // Göz
            float eyeSize = Width * 0.28f;
            var eyeRect = new RectangleF(X + Width * 0.56f, Y + Height * 0.16f, eyeSize, eyeSize);
            graphics.FillEllipse(Brushes.White, eyeRect);
            graphics.DrawEllipse(Pens.Black, eyeRect);
            graphics.FillEllipse(Brushes.Black, eyeRect.X + eyeSize * 0.45f, eyeRect.Y + eyeSize * 0.28f, eyeSize * 0.4f, eyeSize * 0.4f);

            // Gaga
            PointF[] beak =
            {
                new PointF(X + Width * 0.85f, Y + Height * 0.42f),
                new PointF(X + Width + 9f, Y + Height * 0.54f),
                new PointF(X + Width * 0.82f, Y + Height * 0.68f)
            };
            using (var beakBrush = new SolidBrush(Color.FromArgb(242, 92, 34)))
            {
                graphics.FillPolygon(beakBrush, beak);
                graphics.DrawPolygon(Pens.Maroon, beak);
            }

            graphics.Restore(state);
        }
    }
}
