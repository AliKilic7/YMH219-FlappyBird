using System.Windows.Forms;

namespace YMH219.FlappyBird.UI
{
    /// <summary>
    /// Titreşimsiz GDI+ çizimi için DoubleBuffered özellikli özel çizim paneli.
    /// </summary>
    public sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            UpdateStyles();
        }
    }
}
