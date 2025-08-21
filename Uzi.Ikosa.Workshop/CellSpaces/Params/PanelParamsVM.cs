using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Uzi.Visualize;
using System.Windows;
using Uzi.Ikosa.Tactical;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Workshop
{
    public class PanelParamsVM : INotifyPropertyChanged
    {
        public PanelParamsVM(PanelParams param)
        {
            _Params = param;
            _TriangleCorners = new object[] { TriangleCorner.UpperLeft, TriangleCorner.LowerLeft, TriangleCorner.UpperRight, TriangleCorner.LowerRight };
            _FaceEdges = new object[] { FaceEdge.Top, FaceEdge.Bottom, FaceEdge.Left, FaceEdge.Right };
            _Interiors = new[] { PanelInterior.None, PanelInterior.Diagonal, PanelInterior.Slope, PanelInterior.Bend };
        }

        private PanelParams _Params;
        private object[] _FaceEdges;
        private object[] _TriangleCorners;
        private PanelInterior[] _Interiors;

        #region INotifyPropertyChanged Members
        private void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void DoAllPropertyChanged(string propSuffix)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(string.Concat(@"PanelType", propSuffix)));
                PropertyChanged(this, new PropertyChangedEventArgs(string.Concat(@"PanelParamData", propSuffix)));
                PropertyChanged(this, new PropertyChangedEventArgs(string.Concat(@"PanelParamDataItems", propSuffix)));
                PropertyChanged(this, new PropertyChangedEventArgs(string.Concat(@"PanelParamDataVisibility", propSuffix)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(InteriorControlsVisibility)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DiagonalFaceVisibility)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(OtherFaceVisibility)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SlopeIndexVisibility)));
            }
        }

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public PanelParams Params { get { return _Params; } }

        public PanelType PanelTypeZLow { get { return _Params.PanelTypeZLow; } set { _Params.PanelTypeZLow = value; DoAllPropertyChanged(@"ZLow"); } }
        public PanelType PanelTypeYLow { get { return _Params.PanelTypeYLow; } set { _Params.PanelTypeYLow = value; DoAllPropertyChanged(@"YLow"); } }
        public PanelType PanelTypeXLow { get { return _Params.PanelTypeXLow; } set { _Params.PanelTypeXLow = value; DoAllPropertyChanged(@"XLow"); } }
        public PanelType PanelTypeZHigh { get { return _Params.PanelTypeZHigh; } set { _Params.PanelTypeZHigh = value; DoAllPropertyChanged(@"ZHigh"); } }
        public PanelType PanelTypeYHigh { get { return _Params.PanelTypeYHigh; } set { _Params.PanelTypeYHigh = value; DoAllPropertyChanged(@"YHigh"); } }
        public PanelType PanelTypeXHigh { get { return _Params.PanelTypeXHigh; } set { _Params.PanelTypeXHigh = value; DoAllPropertyChanged(@"XHigh"); } }

        public FaceEdge EdgeZLow { get { return _Params.EdgeZLow; } set { _Params.EdgeZLow = value; DoPropertyChanged(@"EdgeZLow"); } }
        public FaceEdge EdgeYLow { get { return _Params.EdgeYLow; } set { _Params.EdgeYLow = value; DoPropertyChanged(@"EdgeYLow"); } }
        public FaceEdge EdgeXLow { get { return _Params.EdgeXLow; } set { _Params.EdgeXLow = value; DoPropertyChanged(@"EdgeXLow"); } }
        public FaceEdge EdgeZHigh { get { return _Params.EdgeZHigh; } set { _Params.EdgeZHigh = value; DoPropertyChanged(@"EdgeZHigh"); } }
        public FaceEdge EdgeYHigh { get { return _Params.EdgeYHigh; } set { _Params.EdgeYHigh = value; DoPropertyChanged(@"EdgeYHigh"); } }
        public FaceEdge EdgeXHigh { get { return _Params.EdgeXHigh; } set { _Params.EdgeXHigh = value; DoPropertyChanged(@"EdgeXHigh"); } }

        public TriangleCorner CornerZLow { get { return _Params.CornerZLow; } set { _Params.CornerZLow = value; DoPropertyChanged(@"CornerZLow"); } }
        public TriangleCorner CornerYLow { get { return _Params.CornerYLow; } set { _Params.CornerYLow = value; DoPropertyChanged(@"CornerYLow"); } }
        public TriangleCorner CornerXLow { get { return _Params.CornerXLow; } set { _Params.CornerXLow = value; DoPropertyChanged(@"CornerXLow"); } }
        public TriangleCorner CornerZHigh { get { return _Params.CornerZHigh; } set { _Params.CornerZHigh = value; DoPropertyChanged(@"CornerZHigh"); } }
        public TriangleCorner CornerYHigh { get { return _Params.CornerYHigh; } set { _Params.CornerYHigh = value; DoPropertyChanged(@"CornerYHigh"); } }
        public TriangleCorner CornerXHigh { get { return _Params.CornerXHigh; } set { _Params.CornerXHigh = value; DoPropertyChanged(@"CornerXHigh"); } }

        public object PanelParamDataZLow
        {
            get { return ((PanelTypeZLow == PanelType.Corner) || (PanelTypeZLow == PanelType.MaskedCorner)) ? (object)EdgeZLow : CornerZLow; }
            set { if ((PanelTypeZLow != PanelType.Corner) && (PanelTypeZLow != PanelType.MaskedCorner))
                {
                    CornerZLow = (TriangleCorner)value;
                }
                else
                {
                    EdgeZLow = (FaceEdge)value;
                }
            }
        }
        public object PanelParamDataYLow
        {
            get { return ((PanelTypeYLow == PanelType.Corner) || (PanelTypeYLow == PanelType.MaskedCorner)) ? (object)EdgeYLow : CornerYLow; }
            set { if ((PanelTypeYLow != PanelType.Corner) && (PanelTypeYLow != PanelType.MaskedCorner))
                {
                    CornerYLow = (TriangleCorner)value;
                }
                else
                {
                    EdgeYLow = (FaceEdge)value;
                }
            }
        }
        public object PanelParamDataXLow
        {
            get { return ((PanelTypeXLow == PanelType.Corner) || (PanelTypeXLow == PanelType.MaskedCorner)) ? (object)EdgeXLow : CornerXLow; }
            set { if ((PanelTypeXLow != PanelType.Corner) && (PanelTypeXLow != PanelType.MaskedCorner))
                {
                    CornerXLow = (TriangleCorner)value;
                }
                else
                {
                    EdgeXLow = (FaceEdge)value;
                }
            }
        }
        public object PanelParamDataZHigh
        {
            get { return ((PanelTypeZHigh == PanelType.Corner) || (PanelTypeZHigh == PanelType.MaskedCorner)) ? (object)EdgeZHigh : CornerZHigh; }
            set { if ((PanelTypeZHigh != PanelType.Corner) && (PanelTypeZHigh != PanelType.MaskedCorner))
                {
                    CornerZHigh = (TriangleCorner)value;
                }
                else
                {
                    EdgeZHigh = (FaceEdge)value;
                }
            }
        }
        public object PanelParamDataYHigh
        {
            get { return ((PanelTypeYHigh == PanelType.Corner) || (PanelTypeYHigh == PanelType.MaskedCorner)) ? (object)EdgeYHigh : CornerYHigh; }
            set { if ((PanelTypeYHigh != PanelType.Corner) && (PanelTypeYHigh != PanelType.MaskedCorner))
                {
                    CornerYHigh = (TriangleCorner)value;
                }
                else
                {
                    EdgeYHigh = (FaceEdge)value;
                }
            }
        }
        public object PanelParamDataXHigh
        {
            get { return ((PanelTypeXHigh == PanelType.Corner) || (PanelTypeXHigh == PanelType.MaskedCorner)) ? (object)EdgeXHigh : CornerXHigh; }
            set { if ((PanelTypeXHigh != PanelType.Corner) && (PanelTypeXHigh != PanelType.MaskedCorner))
                {
                    CornerXHigh = (TriangleCorner)value;
                }
                else
                {
                    EdgeXHigh = (FaceEdge)value;
                }
            }
        }

        public IEnumerable<object> PanelParamDataItemsZLow { get { return ((PanelTypeZLow == PanelType.Corner) || (PanelTypeZLow == PanelType.MaskedCorner)) ? _FaceEdges : _TriangleCorners; } }
        public IEnumerable<object> PanelParamDataItemsYLow { get { return ((PanelTypeYLow == PanelType.Corner) || (PanelTypeYLow == PanelType.MaskedCorner)) ? _FaceEdges : _TriangleCorners; } }
        public IEnumerable<object> PanelParamDataItemsXLow { get { return ((PanelTypeXLow == PanelType.Corner) || (PanelTypeXLow == PanelType.MaskedCorner)) ? _FaceEdges : _TriangleCorners; } }
        public IEnumerable<object> PanelParamDataItemsZHigh { get { return ((PanelTypeZHigh == PanelType.Corner) || (PanelTypeZHigh == PanelType.MaskedCorner)) ? _FaceEdges : _TriangleCorners; } }
        public IEnumerable<object> PanelParamDataItemsYHigh { get { return ((PanelTypeYHigh == PanelType.Corner) || (PanelTypeYHigh == PanelType.MaskedCorner)) ? _FaceEdges : _TriangleCorners; } }
        public IEnumerable<object> PanelParamDataItemsXHigh { get { return ((PanelTypeXHigh == PanelType.Corner) || (PanelTypeXHigh == PanelType.MaskedCorner)) ? _FaceEdges : _TriangleCorners; } }

        public Visibility PanelParamDataVisibilityZLow { get { return (!PanelTypeZLow.IsInteriorBindable()) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility PanelParamDataVisibilityYLow { get { return (!PanelTypeYLow.IsInteriorBindable()) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility PanelParamDataVisibilityXLow { get { return (!PanelTypeXLow.IsInteriorBindable()) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility PanelParamDataVisibilityZHigh { get { return (!PanelTypeZHigh.IsInteriorBindable()) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility PanelParamDataVisibilityYHigh { get { return (!PanelTypeYHigh.IsInteriorBindable()) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility PanelParamDataVisibilityXHigh { get { return (!PanelTypeXHigh.IsInteriorBindable()) ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility InteriorControlsVisibility { get { return _Params.IsInteriorBindable ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility DiagonalFaceVisibility
            => _Params.IsInteriorBindable && _Params.PanelInterior != PanelInterior.None
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility OtherFaceVisibility
            => _Params.IsInteriorBindable && (_Params.PanelInterior == PanelInterior.Diagonal || _Params.PanelInterior == PanelInterior.Bend)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility SlopeIndexVisibility
            => _Params.IsInteriorBindable && _Params.PanelInterior == PanelInterior.Slope
            ? Visibility.Visible
            : Visibility.Collapsed;

        public IEnumerable<PanelInterior> InteriorTypes { get { return _Interiors; } }
        public PanelInterior Interior
        {
            get { return _Params.PanelInterior; }
            set
            {
                _Params.PanelInterior = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DiagonalFaceVisibility)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(OtherFaceVisibility)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SlopeIndexVisibility)));
            }
        }
        public IEnumerable<AnchorFace> SourceFaces { get { return PanelCellSpace.AllFaces; } }
        public IEnumerable<AnchorFace> SinkFaces { get { return PanelCellSpace.AllFaces; } }
        public IEnumerable<OptionalAnchorFace> OtherFaces
        {
            get
            {
                yield return OptionalAnchorFace.None;
                foreach (var _f in PanelCellSpace.AllFaces)
                {
                    yield return _f.ToOptionalAnchorFace();
                }

                yield break;
            }
        }
        public AnchorFace SourceFace { get { return _Params.SourceFace; } set { _Params.SourceFace = value; } }
        public AnchorFace SinkFace { get { return _Params.SinkFace; } set { _Params.SinkFace = value; } }
        public OptionalAnchorFace OtherFace { get { return _Params.OtherFace; } set { _Params.OtherFace = value; } }

        public IEnumerable<PanelFill> PanelFills
        {
            get
            {
                yield return PanelFill.Fill0;
                yield return PanelFill.Fill1;
                yield return PanelFill.Fill2;
                yield return PanelFill.Fill3;
                yield break;
            }
        }
        public PanelFill PanelFill { get { return _Params.PanelFill; } set { _Params.PanelFill = value; } }

        public byte SlopeIndex { get { return _Params.SlopeIndex; } set { _Params.SlopeIndex = value; } }

        public IEnumerable<byte> SlopeIndexes
        {
            get
            {
                yield return 0;
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
                yield return 6;
                yield return 7;
                yield return 8;
                yield return 9;
                yield return 10;
                yield return 11;
                yield return 12;
                yield return 13;
                yield return 14;
                yield return 15;
                yield break;
            }
        }
    }
}