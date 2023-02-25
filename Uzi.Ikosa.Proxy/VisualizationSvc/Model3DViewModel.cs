using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.IO;
using System.Windows.Markup;
using Uzi.Visualize.Contracts;
using Uzi.Visualize.Packaging;
using System;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class Model3DViewModel : IResolveBitmapImage
    {
        public Model3DViewModel(Model3DInfo model3DInfo, LocalMapInfo map)
        {
            _Info = model3DInfo;
            LocalMap = map;
            _Brushes = new BrushCollectionViewModel(_Info.Brushes, this);
        }

        #region private data
        private Model3DInfo _Info;
        private Dictionary<string, BitmapImageViewModel> _Images = null;
        private LocalMapInfo _Map;
        private BrushCollectionViewModel _Brushes;

        protected List<Type> _SETypes = null;
        protected List<Tuple<ModelCacheSelector, Model3D>> _ModelCache = new List<Tuple<ModelCacheSelector, Model3D>>();
        #endregion

        public Model3DInfo Info { get { return _Info; } }
        public BrushCollectionViewModel Brushes { get { return _Brushes; } }

        /// <summary>Sets local map on model and brushes, also sets model on brushes</summary>
        public LocalMapInfo LocalMap
        {
            get { return _Map; }
            private set { _Map = value; }
        }

        /// <summary>Reloads model from part stream or file</summary>
        public void RefreshModel()
        {
            _ModelCache.Clear();
        }

        #region public Model3D Model { get; }
        public virtual Model3D Model
        {
            get { return ResolveModel(); }
        }

        public virtual Model3D ResolveModel()
        {
            ModelCacheSelector _selector = null;
            if (_SETypes != null)
            {
                // try the sense effect cache...
                _selector = new ModelCacheSelector((
                    from _t in _SETypes
                    let _i = (MarkupExtension)Activator.CreateInstance(_t)
                    select (VisualEffect)_i.ProvideValue(null)).ToList(),
                    ExternalVal.Values);
                var _entry = _ModelCache
                    .FirstOrDefault(_ce => _ce.Item1.Equals(_selector));
                if (_entry != null)
                    return _entry.Item2;
            }
            else
            {
                _selector = new ModelCacheSelector(null, ExternalVal.Values);
            }

            // only need resolvers for a limited time
            try
            {
                Model3D _mdl = null;
                VisualEffectMaterial.PushResolver(_Brushes);
                IkosaImageSource.PushResolver(this);
                if (_SETypes == null)
                {
                    // referenced sense extensions
                    _SETypes = new List<Type>();
                    SenseEffectExtension.ReferencedEffect = (type) => { if (!_SETypes.Contains(type)) _SETypes.Add(type); };
                }

                var _memStream = new MemoryStream(Info.Bytes);
                _mdl = XamlReader.Load(_memStream) as Model3D;
                if (_ModelCache
                    .FirstOrDefault(_ce => _ce.Item1.Equals(_selector)) == null)
                {
                    _mdl.Freeze();
                    _ModelCache.Add(new Tuple<ModelCacheSelector, Model3D>(_selector.GetCacheKey(), _mdl));
                }
                _memStream.Close();
                return _mdl;
            }
            finally
            {
                SenseEffectExtension.ReferencedEffect = null;
                IkosaImageSource.PullResolver(this);
                VisualEffectMaterial.PullResolver(_Brushes);
            }
        }
        #endregion

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            string _key = key.ToString();
            if (Info.BitmapImages.Contains(_key))
            {
                if (_Images == null)
                    _Images = new Dictionary<string, BitmapImageViewModel>();
                if (!_Images.ContainsKey(_key) && (LocalMap != null))
                {
                    _Images.Add(_key, new BitmapImageViewModel(LocalMap.MyProxy.Service.GetBitmapImageForModel(Info.Name, _key)));
                }
                return _Images[_key].GetImage(effect);
            }
            return null;
        }

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            string _key = key.ToString();
            if (Info.BitmapImages.Contains(_key))
            {
                if (_Images == null)
                    _Images = new Dictionary<string, BitmapImageViewModel>();
                if (!_Images.ContainsKey(_key) && (LocalMap != null))
                {
                    _Images.Add(_key, new BitmapImageViewModel(LocalMap.MyProxy.Service.GetBitmapImageForModel(Info.Name, _key)));
                }
                return _Images[_key];
            }
            return null;
        }

        public IResolveBitmapImage IResolveBitmapImageParent { get { return LocalMap; } }

        /// <summary>Only used by package editors to inspect</summary>
        public IEnumerable<Visualize.Packaging.BitmapImagePartListItem> ResolvableImages { get { yield break; } }

        #endregion
    }
}