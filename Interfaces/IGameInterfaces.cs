using System.Drawing;

namespace YMH219.FlappyBird.Interfaces
{
    /// <summary>
    /// Ekrana GDI+ ile çizilebilen oyun nesnelerini tanımlar.
    /// </summary>
    public interface IDrawable
    {
        void Draw(Graphics graphics);
    }

    /// <summary>
    /// Kare (frame) bazlı durumu güncellenebilen oyun nesnelerini tanımlar.
    /// </summary>
    public interface IUpdatable
    {
        void Update(float deltaTime);
    }

    /// <summary>
    /// Çarpışma kontrolü yapılabilen oyun nesnelerini tanımlar.
    /// </summary>
    public interface ICollidable
    {
        RectangleF Bounds { get; }
        bool IntersectsWith(RectangleF target);
    }
}
