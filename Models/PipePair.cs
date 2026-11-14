using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace YMH219.FlappyBird.Models
{
    /// <summary>
    /// Üst ve alt borudan oluşan, çarpışma dikdörtgenlerini yöneten engel çifti.
    /// </summary>
    public sealed class PipePair : GameObject
    {
        private const float CapHeight = 22f;
        private const float CapOverhang = 5f;

        private readonly int _playfieldHeight;
        private float _gapY;
        private float _gapHeight;
        private float _scrollSpeed;

        public bool IsPassed { get; set; }

        public float ScrollSpeed
        {
            get => _scrollSpeed;
            set => _scrollSpeed = Math.Max(60f, value);
        }

        public RectangleF TopPipeBounds => new RectangleF(X, 0f, Width, _gapY);

        public RectangleF BottomPipeBounds => new RectangleF(
            X,
            _gapY + _gapHeight,
            Width,
            Math.Max(0f, _playfieldHeight - (_gapY + _gapHeight)));

        public PipePair(float startX, float width, int playfieldHeight, float gapY, float gapHeight, float scrollSpeed)
            : base(startX, 0f, width, playfieldHeight)
        {
            _playfieldHeight = playfieldHeight;
            _gapY = gapY;
            _gapHeight = gapHeight;
            ScrollSpeed = scrollSpeed;
        }

        public void Reposition(float newX, float newGapY, float newGapHeight)
        {
            X = newX;
            _gapY = newGapY;
            _gapHeight = newGapHeight;
            IsPassed = false;
        }

        public override void Update(float deltaTime)
        {
            X -= _scrollSpeed * deltaTime;
        }

        public override bool IntersectsWith(RectangleF target)
        {
            return TopPipeBounds.IntersectsWith(target) || BottomPipeBounds.IntersectsWith(target);
        }

        public override void Draw(Graphics graphics)
        {
            DrawSinglePipe(graphics, TopPipeBounds, isTop: true);
            DrawSinglePipe(graphics, BottomPipeBounds, isTop: false);
        }

        private static void DrawSinglePipe(Graphics graphics, RectangleF rect, bool isTop)
        {
            if (rect.Height <= 0f)
            {
                return;
            }

            using (var pipeBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(116, 206, 62),
                Color.FromArgb(56, 134, 34),
                LinearGradientMode.Horizontal))
            using (var borderPen = new Pen(Color.FromArgb(36, 84, 20), 2f))
            {
                graphics.FillRectangle(pipeBrush, rect);
                graphics.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);

                float capY = isTop ? Math.Max(0f, rect.Bottom - CapHeight) : rect.Y;
                var capRect = new RectangleF(
                    rect.X - CapOverhang,
                    capY,
                    rect.Width + CapOverhang * 2f,
                    Math.Min(CapHeight, rect.Height));

                graphics.FillRectangle(pipeBrush, capRect);
                graphics.DrawRectangle(borderPen, capRect.X, capRect.Y, capRect.Width, capRect.Height);
            }
        }
    }
}
