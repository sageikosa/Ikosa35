using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public interface IZoomIcons
    {
        double ZoomLevel { get; set; }
        double UnZoomLevel { get; set; }
        Guid ZoomedIcon { get; set; }
    }
}
