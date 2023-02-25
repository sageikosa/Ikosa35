using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class BrushCollectionViewModel : IResolveBitmapImage, IResolveMaterial
    {
        #region private data
        private BrushCollection _Brushes = null;
        private Dictionary<string, BitmapImageViewModel> _Images = null;
        #endregion

        public BrushCollectionViewModel(BrushCollectionInfo info, LocalMapInfo map)
        {
            BrushCollectionInfo = info;
            LocalMap = map;
            Model = null;
            ResolveMaterialCollection();
        }

        public BrushCollectionViewModel(BrushCollectionInfo info, Model3DViewModel model)
        {
            BrushCollectionInfo = info;
            Model = model;
            LocalMap = model.LocalMap;
            ResolveMaterialCollection();
        }

        public BrushCollectionInfo BrushCollectionInfo { get; set; }

        public Model3DViewModel Model { get; private set; }

        public LocalMapInfo LocalMap { get; private set; }

        #region public BrushCollection BrushDefinitions { get; }
        public virtual BrushCollection BrushDefinitions
        {
            get
            {
                return _Brushes;
            }
        }

        private void ResolveMaterialCollection()
        {
            // only need resolvers for a limited time
            try
            {
                IkosaImageSource.PushResolver(this);

                using (var _memStream = new MemoryStream(BrushCollectionInfo.Bytes))
                {
                    _Brushes = XamlReader.Load(_memStream) as BrushCollection;
                }

                // ensure the brush knows who owns it
                foreach (var _iBrush in _Brushes)
                {
                    _iBrush.Owner = this;
                    _iBrush.ClearCache();
                }
            }
            finally
            {
                IkosaImageSource.PullResolver(this);
            }
        }
        #endregion

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            string _key = key.ToString();
            if (BrushCollectionInfo.BitmapImages.Contains(_key))
            {
                if (_Images == null)
                    _Images = new Dictionary<string, BitmapImageViewModel>();
                if (!_Images.ContainsKey(_key) && (LocalMap != null))
                {
                    if (!string.IsNullOrEmpty(BrushCollectionInfo.ModelName))
                        _Images.Add(_key, new BitmapImageViewModel(LocalMap.MyProxy.Service.GetBitmapImageForMetaModelBrushesCollection(BrushCollectionInfo.ModelName, _key)));
                    else
                        _Images.Add(_key, new BitmapImageViewModel(LocalMap.MyProxy.Service.GetBitmapImageForBrushCollection(BrushCollectionInfo.Name, _key)));
                }
                return _Images[_key].GetImage(effect);
            }
            return null;
        }

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            string _key = key.ToString();
            if (BrushCollectionInfo.BitmapImages.Contains(_key))
            {
                if (_Images == null)
                    _Images = new Dictionary<string, BitmapImageViewModel>();
                if (!_Images.ContainsKey(_key) && (LocalMap != null))
                {
                    _Images.Add(_key, new BitmapImageViewModel(LocalMap.MyProxy.Service.GetBitmapImageForBrushCollection(BrushCollectionInfo.Name, _key)));
                }
                return _Images[_key];
            }
            return null;
        }

        public IResolveBitmapImage IResolveBitmapImageParent
        {
            get { return (IResolveBitmapImage)Model ?? (IResolveBitmapImage)LocalMap; }
        }

        /// <summary>Only used by package editors to inspect</summary>
        public IEnumerable<Visualize.Packaging.BitmapImagePartListItem> ResolvableImages { get { yield break; } }

        #endregion

        #region IResolveMaterial Members

        public System.Windows.Media.Media3D.Material GetMaterial(object key, VisualEffect effect)
        {
            var _brush = BrushDefinitions.FirstOrDefault(_b => _b.BrushKey.Equals(key.ToString()));
            if (_brush != null)
            {
                return _brush.GetMaterial(effect);
            }
            return null;
        }

        public IResolveMaterial IResolveMaterialParent { get { return null; } }

        /// <summary>Only used by package editors to inspect</summary>
        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes { get { yield break; } }

        #endregion
    }
}