using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class TileSetViewModel
    {
        public TileSetViewModel(TileSetInfo info, LocalMapInfo map)
        {
            TileSetInfo = info;
            LocalMap = map;
        }

        public TileSetInfo TileSetInfo { get; set; }
        public LocalMapInfo LocalMap { get; set; }
        public BrushCollectionViewModel BrushCollection { get { return LocalMap.GetBrushCollectionViewModel(TileSetInfo.BrushCollectionKey); } }

        #region public bool GetAnchorFaceAlpha(AnchorFace face)
        public bool GetAnchorFaceAlpha(AnchorFace face)
        {
            try
            {
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return false;

                var _index = GetAnchorFaceMaterialIndex(face);
                if (_index < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[_index].Opacity < 1d;
            }
            catch { }
            return false;
        }
        #endregion

        #region public bool GetWedgeAlpha()
        public bool GetWedgeAlpha()
        {
            try
            {
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return false;

                if (TileSetInfo.WedgeIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.WedgeIndex].Opacity < 1d;
            }
            catch { }
            return false;
        }
        #endregion

        public int InsideMaterialIndex  => GetAnchorFaceMaterialIndex(AnchorFace.ZHigh);
        public int OutsideMaterialIndex => GetAnchorFaceMaterialIndex(AnchorFace.ZLow);
        public int TopSideMaterialIndex => GetAnchorFaceMaterialIndex(AnchorFace.YHigh);
        public int BottomSideMaterialIndex => GetAnchorFaceMaterialIndex(AnchorFace.YLow);
        public int RightSideMaterialIndex => GetAnchorFaceMaterialIndex(AnchorFace.XHigh);
        public int LeftSideMaterialIndex => GetAnchorFaceMaterialIndex(AnchorFace.XLow);

        #region public int GetAnchorFaceMaterialIndex(AnchorFace face)
        public int GetAnchorFaceMaterialIndex(AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.XLow:
                    return TileSetInfo.XMinusIndex;
                case AnchorFace.XHigh:
                    return TileSetInfo.XPlusIndex;
                case AnchorFace.YLow:
                    return TileSetInfo.YMinusIndex;
                case AnchorFace.YHigh:
                    return TileSetInfo.YPlusIndex;
                case AnchorFace.ZLow:
                    return TileSetInfo.ZMinusIndex;
                case AnchorFace.ZHigh:
                default:
                    return TileSetInfo.ZPlusIndex;
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

        #region public Material XMinusMaterial(VisualEffect effect)
        public Material XMinusMaterial(VisualEffect effect)
        {
            try
            {
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.XMinusIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.XMinusIndex].GetMaterial(effect);
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
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.XPlusIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.XPlusIndex].GetMaterial(effect);
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
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.YMinusIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.YMinusIndex].GetMaterial(effect);
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
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.YPlusIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.YPlusIndex].GetMaterial(effect);
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
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.ZMinusIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.ZMinusIndex].GetMaterial(effect);
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
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.ZPlusIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.ZPlusIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        #region public Material WedgeMaterial(VisualEffect effect)
        public Material WedgeMaterial(VisualEffect effect)
        {
            try
            {
                var _brushes = BrushCollection;
                if (_brushes == null)
                    return BrushDefinition.MissingMaterial;

                if (TileSetInfo.WedgeIndex < _brushes.BrushDefinitions.Count)
                    return _brushes.BrushDefinitions[TileSetInfo.WedgeIndex].GetMaterial(effect);
            }
            catch { }
            return null;
        }
        #endregion

        public Material InsideMaterial(VisualEffect effect) { return ZPlusMaterial(effect); }
        public Material OutsideMaterial(VisualEffect effect) { return ZMinusMaterial(effect); }
        public Material TopSideMaterial(VisualEffect effect) { return YPlusMaterial(effect); }
        public Material BottomSideMaterial(VisualEffect effect) { return YMinusMaterial(effect); }
        public Material RightSideMaterial(VisualEffect effect) { return XPlusMaterial(effect); }
        public Material LeftSideMaterial(VisualEffect effect) { return XMinusMaterial(effect); }

        public bool InsideAlpha { get { return GetAnchorFaceAlpha(AnchorFace.ZHigh); } }
        public bool OutsideAlpha { get { return GetAnchorFaceAlpha(AnchorFace.ZLow); } }
        public bool TopSideAlpha { get { return GetAnchorFaceAlpha(AnchorFace.YHigh); } }
        public bool BottomSideAlpha { get { return GetAnchorFaceAlpha(AnchorFace.YLow); } }
        public bool RightSideAlpha { get { return GetAnchorFaceAlpha(AnchorFace.XHigh); } }
        public bool LeftSideAlpha { get { return GetAnchorFaceAlpha(AnchorFace.XLow); } }
    }
}
