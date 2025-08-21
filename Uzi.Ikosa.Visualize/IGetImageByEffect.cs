using System;
using System.Windows.Media.Imaging;

namespace Uzi.Visualize
{
    public interface IGetImageByEffect
    {
        BitmapSource GetImage(VisualEffect effect);
    }

    public class MissingProvider : IGetImageByEffect
    {
        private static BitmapSource _Missing;

        static MissingProvider()
        {
            _Missing = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoImage.png"));
            _Missing.Freeze();
        }

        public BitmapSource GetImage(VisualEffect effect)
            => _Missing;
    }
}
