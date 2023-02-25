using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Uzi.Ikosa.UI
{
    public class TabItemVM
    {
        public ImageSource ImageSource { get; set; }
        public string Title { get; set; }
        public bool IsSelected { get; set; }
        public object Content { get; set; }

        public static ImageSource ImageSourceFromPath(string assembly, string path)
            => new BitmapImage(
                new Uri($@"pack://application:,,,/{assembly};component{path}"));
    }
}
