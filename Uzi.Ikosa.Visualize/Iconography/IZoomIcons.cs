using System;

namespace Uzi.Visualize
{
    public interface IZoomIcons
    {
        double ZoomLevel { get; set; }
        double UnZoomLevel { get; set; }
        Guid ZoomedIcon { get; set; }
    }
}
