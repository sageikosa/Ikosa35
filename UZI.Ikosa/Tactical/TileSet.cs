using System;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using System.Windows.Media;
using Uzi.Visualize.Contracts.Tactical;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class TileSet
    {
        #region construction
        public TileSet(string name, string tileSource, LocalMap map)
        {
            _Map = map;
            _Name = name;
            _Key = tileSource;
        }
        #endregion

        #region state
        private LocalMap _Map;
        private string _Name;
        private string _Key;
        private int _XMinusMaterialIndex;
        private int _XPlusMaterialIndex;
        private int _YMinusMaterialIndex;
        private int _YPlusMaterialIndex;
        private int _ZPlusMaterialIndex;
        private int _ZMinusMaterialIndex;
        private int _WedgeMaterialIndex;
        private int _InnerMaterialIndex;

        [NonSerialized, JsonIgnore]
        private BrushCollection _Brushes = null;
        [NonSerialized, JsonIgnore]
        private string _CachedKey = string.Empty;
        #endregion

        public LocalMap Map => _Map;
        public string Name => _Name;
        public string BrushCollectionKey { get => _Key; set => _Key = value; }

        #region public BrushCollection BrushCollection { get; }
        public BrushCollection BrushCollection
        {
            get
            {
                // found!
                if ((_Brushes != null) && string.Equals(_CachedKey, _Key))
                    return _Brushes;

                // look...
                var _resolve = _Map.Resources as IResolveBrushCollection;
                while (_resolve != null)
                {
                    var _brushes = _resolve.GetBrushCollection(_Key);
                    if (_brushes != null)
                    {
                        // save and return
                        _Brushes = _brushes;
                        _CachedKey = _Key;
                        return _brushes;
                    }

                    // climb and look
                    _resolve = _resolve.IResolveBrushCollectionParent;
                }

                // nothing
                _Brushes = new BrushCollection();
                return _Brushes;
            }
        }
        #endregion

        public int GetSideIndexMaterialIndex(SideIndex side)
        {
            switch (side)
            {
                case SideIndex.Top: return TopSideMaterialIndex;
                case SideIndex.Bottom: return BottomSideMaterialIndex;
                case SideIndex.Front: return InsideMaterialIndex;
                case SideIndex.Back: return OutsideMaterialIndex;
                case SideIndex.Left: return LeftSideMaterialIndex;
                case SideIndex.Right: return RightSideMaterialIndex;
                default:
                    return RightSideMaterialIndex;
            }
        }

        #region public int GetAnchorFaceMaterialIndex(AnchorFace face)
        public int GetAnchorFaceMaterialIndex(AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.XLow:
                    return XMinusMaterialIndex;
                case AnchorFace.XHigh:
                    return XPlusMaterialIndex;
                case AnchorFace.YLow:
                    return YMinusMaterialIndex;
                case AnchorFace.YHigh:
                    return YPlusMaterialIndex;
                case AnchorFace.ZLow:
                    return ZMinusMaterialIndex;
                case AnchorFace.ZHigh:
                default:
                    return ZPlusMaterialIndex;
            }
        }
        #endregion

        public int XMinusMaterialIndex { get => _XMinusMaterialIndex; set => _XMinusMaterialIndex = value; }
        public int XPlusMaterialIndex { get => _XPlusMaterialIndex; set => _XPlusMaterialIndex = value; }
        public int YMinusMaterialIndex { get => _YMinusMaterialIndex; set => _YMinusMaterialIndex = value; }
        public int YPlusMaterialIndex { get => _YPlusMaterialIndex; set => _YPlusMaterialIndex = value; }
        public int ZMinusMaterialIndex { get => _ZMinusMaterialIndex; set => _ZMinusMaterialIndex = value; }
        public int ZPlusMaterialIndex { get => _ZPlusMaterialIndex; set => _ZPlusMaterialIndex = value; }
        public int WedgeMaterialIndex { get => _WedgeMaterialIndex; set => _WedgeMaterialIndex = value; }
        public int InnerMaterialIndex { get => _InnerMaterialIndex; set => _InnerMaterialIndex = value; }

        #region public bool GetAnchorFaceAlpha(AnchorFace face)
        public bool GetAnchorFaceAlpha(AnchorFace face)
        {
            try
            {
                if (BrushCollection == null)
                    return false;

                var _index = GetAnchorFaceMaterialIndex(face);
                if (_index < BrushCollection.Count)
                    return BrushCollection[_index].Opacity < 1d;
            }
            catch { }
            return false;
        }
        #endregion

        #region public Material GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect)
        public Material GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                var _index = GetAnchorFaceMaterialIndex(face);
                if (_index < BrushCollection.Count)
                    return BrushCollection[_index].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material XMinusMaterial(VisualEffect effect)
        public Material XMinusMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_XMinusMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_XMinusMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material XPlusMaterial(VisualEffect effect)
        public Material XPlusMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_XPlusMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_XPlusMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material YMinusMaterial(VisualEffect effect)
        public Material YMinusMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_YMinusMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_YMinusMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material YPlusMaterial(VisualEffect effect)
        public Material YPlusMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_YPlusMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_YPlusMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material ZMinusMaterial(VisualEffect effect)
        public Material ZMinusMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_ZMinusMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_ZMinusMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material ZPlusMaterial(VisualEffect effect)
        public Material ZPlusMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_ZPlusMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_ZPlusMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public bool GetWedgeAlpha()
        public bool GetWedgeAlpha()
        {
            try
            {
                if (BrushCollection == null)
                    return false;

                if (_WedgeMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_WedgeMaterialIndex].Opacity < 1d;
            }
            catch { }
            return false;
        }
        #endregion

        #region public Material WedgeMaterial(VisualEffect effect)
        public Material WedgeMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_WedgeMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_WedgeMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public bool GetInnerAlpha()
        public bool GetInnerAlpha()
        {
            try
            {
                if (BrushCollection == null)
                    return false;

                if (_InnerMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_InnerMaterialIndex].Opacity < 1d;
            }
            catch { }
            return false;
        }
        #endregion

        #region public Material InnerMaterial(VisualEffect effect)
        public Material InnerMaterial(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return BrushDefinition.MissingMaterial;

                if (_InnerMaterialIndex < BrushCollection.Count)
                    return BrushCollection[_InnerMaterialIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Brush InnerBrush(VisualEffect effect)
        public Brush InnerBrush(VisualEffect effect)
        {
            try
            {
                if (BrushCollection == null)
                    return Brushes.Magenta;

                if (_InnerMaterialIndex < BrushCollection.Count)
                    if ((effect == VisualEffect.Unseen) && GetInnerAlpha())
                        return BrushCollection[_InnerMaterialIndex].GetBrush(VisualEffect.DimTo25);
                return BrushCollection[_InnerMaterialIndex].GetBrush(effect);
            }
            catch { }
            return null;
        }
        #endregion

        public int InsideMaterialIndex { get => _ZPlusMaterialIndex; set => _ZPlusMaterialIndex = value; }
        public int OutsideMaterialIndex { get => _ZMinusMaterialIndex; set => _ZMinusMaterialIndex = value; }
        public int TopSideMaterialIndex { get => _YPlusMaterialIndex; set => _YPlusMaterialIndex = value; }
        public int BottomSideMaterialIndex { get => _YMinusMaterialIndex; set => _YMinusMaterialIndex = value; }
        public int RightSideMaterialIndex { get => _XPlusMaterialIndex; set => _XPlusMaterialIndex = value; }
        public int LeftSideMaterialIndex { get => _XMinusMaterialIndex; set => _XMinusMaterialIndex = value; }
        public Material InsideMaterial(VisualEffect effect) => ZPlusMaterial(effect);
        public Material OutsideMaterial(VisualEffect effect) => ZMinusMaterial(effect);
        public Material TopSideMaterial(VisualEffect effect) => YPlusMaterial(effect);
        public Material BottomSideMaterial(VisualEffect effect) => YMinusMaterial(effect);
        public Material RightSideMaterial(VisualEffect effect) => XPlusMaterial(effect);
        public Material LeftSideMaterial(VisualEffect effect) => XMinusMaterial(effect);
        public bool InsideAlpha => GetAnchorFaceAlpha(AnchorFace.ZHigh);
        public bool OutsideAlpha => GetAnchorFaceAlpha(AnchorFace.ZLow);
        public bool TopSideAlpha => GetAnchorFaceAlpha(AnchorFace.YHigh);
        public bool BottomSideAlpha => GetAnchorFaceAlpha(AnchorFace.YLow);
        public bool RightSideAlpha => GetAnchorFaceAlpha(AnchorFace.XHigh);
        public bool LeftSideAlpha => GetAnchorFaceAlpha(AnchorFace.XLow);

        public TileSetInfo ToTileSetInfo(string cellMaterial)
        {
            var _info = new TileSetInfo
            {
                CellMaterial = cellMaterial,
                Name = Name,
                BrushCollectionKey = BrushCollectionKey,
                XMinusIndex = XMinusMaterialIndex,
                XPlusIndex = XPlusMaterialIndex,
                YMinusIndex = YMinusMaterialIndex,
                YPlusIndex = YPlusMaterialIndex,
                ZMinusIndex = ZMinusMaterialIndex,
                ZPlusIndex = ZPlusMaterialIndex,
                WedgeIndex = WedgeMaterialIndex,
                InnerIndex = InnerMaterialIndex
            };
            return _info;
        }
    }
}