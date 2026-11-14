using System;
using System.Drawing;
using YMH219.FlappyBird.Interfaces;

namespace YMH219.FlappyBird.Models
{
    /// <summary>
    /// Tüm oyun varlıkları için soyut temel sınıf (Abstraction &amp; Encapsulation).
    /// </summary>
    public abstract class GameObject : IDrawable, IUpdatable, ICollidable
    {
        private float _x;
        private float _y;
        private float _width;
        private float _height;

        public float X
        {
            get => _x;
            protected set => _x = value;
        }

        public float Y
        {
            get => _y;
            protected set => _y = value;
        }

        public float Width
        {
            get => _width;
            protected set => _width = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(Width));
        }

        public float Height
        {
            get => _height;
            protected set => _height = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(Height));
        }

        public virtual RectangleF Bounds => new RectangleF(_x, _y, _width, _height);

        protected GameObject(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public abstract void Update(float deltaTime);

        public abstract void Draw(Graphics graphics);

        public virtual bool IntersectsWith(RectangleF target)
        {
            return Bounds.IntersectsWith(target);
        }
    }
}
