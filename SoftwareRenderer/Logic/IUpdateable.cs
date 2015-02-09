using System;

namespace SoftwareRenderer.Logic
{
    public interface IUpdateable
    {
        void Update(TimeSpan elapsedTime);
        void Render();
    }
}
