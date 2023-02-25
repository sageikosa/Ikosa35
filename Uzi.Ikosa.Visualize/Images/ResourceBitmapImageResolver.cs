using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize
{
    public class ResourceBitmapImageResolver : IResolveBitmapImage
    {
        public ResourceBitmapImageResolver(IEnumerable<IResolveBitmapImage> resolvers)
        {
            if (resolvers != null)
                _Resolvers = resolvers.ToList();
            else
                _Resolvers = new List<IResolveBitmapImage>();
        }

        private List<IResolveBitmapImage> _Resolvers;
        public IEnumerable<IResolveBitmapImage> Resolvers { get { return _Resolvers.Select(_r => _r); } }

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            foreach (var _resource in _Resolvers)
            {
                var _img = _resource.GetImage(key, effect);
                if (_img != null)
                    return _img;
            }
            return null;
        }

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            foreach (var _resource in _Resolvers)
            {
                var _getter = _resource.GetIGetImageByEffect(key);
                if (_getter != null)
                    return _getter;
            }
            return null;
        }

        public IResolveBitmapImage IResolveBitmapImageParent { get { return null; } }

        public IEnumerable<BitmapImagePartListItem> ResolvableImages
        {
            get { return _Resolvers.SelectMany(_r => _r.ResolvableImages); }
        }

        #endregion
    }
}