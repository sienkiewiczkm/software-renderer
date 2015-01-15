using System.Windows.Media.Imaging;

namespace SoftwareRenderer.Rendering
{
    public interface IRenderWindow
    {
        WriteableBitmap Framebuffer { get; set; }
    }
}
