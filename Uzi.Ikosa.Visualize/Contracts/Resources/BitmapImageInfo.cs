using System.Runtime.Serialization;
using Uzi.Visualize.Packaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class BitmapImageInfo
    {
        #region construction
        public BitmapImageInfo()
        {
        }

        public BitmapImageInfo(BitmapImagePart imagePart)
        {
            Name = imagePart.Name;
            Bytes = imagePart.StreamBytes;
        }
        #endregion

        #region private data
        private BitmapImage _Image = null;
        #endregion

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public byte[] Bytes { get; set; }

        #region public BitmapSource Image { get; }
        /// <summary>Provide BitmapImage</summary>
        public BitmapSource Image
        {
            get
            {
                if (_Image == null)
                {
                    var _memStream = new MemoryStream(Bytes);
                    _Image = new BitmapImage();
                    try
                    {
                        _Image.BeginInit();
                        _Image.StreamSource = _memStream;
                    }
                    catch
                    {
                        // NOTE: image may blow up
                    }
                    finally
                    {
                        _Image.EndInit();
                        _Image.Freeze();
                    }
                }
                return _Image;
            }
        }
        #endregion

        public BitmapSource GetSizedImage(int width, int height)
        {
            try
            {
                var _bitmap = BitmapFactory.ConvertToPbgra32Format(this.Image);
                _bitmap = _bitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.Bilinear);
                _bitmap.Freeze();
                return _bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
