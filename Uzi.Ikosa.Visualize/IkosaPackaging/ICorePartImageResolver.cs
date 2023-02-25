using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Ikosa.Packaging;

namespace Uzi.Visualize.IkosaPackaging
{
    /// <summary>Resolves bitmap images related under an ICorePart</summary>
    public class ICorePartImageResolver : IResolveBitmapImage
    {
        /// <summary>Resolves bitmap images related under an ICorePart</summary>
        public ICorePartImageResolver(ICorePart part)
        {
            _Part = part;
        }

        private ICorePart _Part;

        public ICorePart Part { get { return _Part; } }

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            if (_Part != null)
            {
                var _image = _Part.FindBasePart(key.ToString()) as BitmapImagePart;
                if (_image != null)
                    return _image.GetImage(effect);
            }
            return null;
        }

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            if (_Part != null)
                return _Part.FindBasePart(key.ToString()) as BitmapImagePart;
            return null;
        }

        public IResolveBitmapImage IResolveBitmapImageParent { get { return null; } }

        public IEnumerable<BitmapImagePartListItem> ResolvableImages
        {
            get
            {
                if (_Part != null)
                    return _Part.Relationships.OfType<BitmapImagePart>()
                        .Select(_bip => new BitmapImagePartListItem
                        {
                            BitmapImagePart = _bip,
                            IsLocal = true
                        });

                // empty list
                return new BitmapImagePartListItem[] { };
            }
        }

        #endregion
    }
}
