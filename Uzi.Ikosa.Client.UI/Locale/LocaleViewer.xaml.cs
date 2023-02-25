using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for LocaleViewer.xaml
    /// </summary>
    public partial class LocaleViewer : UserControl, IPresentationInputBinder
    {
        #region ctor()
        public LocaleViewer()
        {
            try { InitializeComponent(); } catch { }
            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            _Visualization = new Visualization(this, grpTokens, grpRooms, grpAlphaRooms, grpTransients, v3dLocale);
            _ViewPoint = new FirstPersonViewPoint();
        }

        static LocaleViewer()
        {
            // active sound
            var _brush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            _brush.GradientStops.Add(new GradientStop(Colors.Cyan, 0));
            _brush.GradientStops.Add(new GradientStop(Colors.Green, 0.5));
            _brush.GradientStops.Add(new GradientStop(Colors.Cyan, 1));
            _ActiveSound = new DiffuseMaterial(_brush);
            _ActiveSound.Freeze();

            // inactive sound
            _brush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            _brush.GradientStops.Add(new GradientStop(Colors.Red, 0));
            _brush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
            _brush.GradientStops.Add(new GradientStop(Colors.Red, 1));
            _InActiveSound = new DiffuseMaterial(_brush);
            _InActiveSound.Freeze();

            // Marker
            _brush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            _brush.GradientStops.Add(new GradientStop(Colors.Violet, 0));
            _brush.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            _brush.GradientStops.Add(new GradientStop(Colors.Violet, 1));
            _Marker = new DiffuseMaterial(_brush);
            _Marker.Freeze();
        }
        #endregion

        #region state
        private readonly Visualization _Visualization;
        private BaseViewPoint _ViewPoint;
        #endregion

        #region static state
        private static readonly Vector3D _ZDrop = new Vector3D(0, 0, -5);
        private static readonly Vector3D _ZLift = new Vector3D(0, 0, 5);
        private static readonly Vector3D _YDrop = new Vector3D(0, -5, 0);
        private static readonly Vector3D _YLift = new Vector3D(0, 5, 0);
        private static readonly Vector3D _XDrop = new Vector3D(-5, 0, 0);
        private static readonly Vector3D _XLift = new Vector3D(5, 0, 0);
        private static readonly DiffuseMaterial _ActiveSound;
        private static readonly DiffuseMaterial _InActiveSound;
        private static readonly DiffuseMaterial _Marker;
        #endregion

        #region public ViewPointType ViewPointType { get; set; } (DEPENDENCY)
        public ViewPointType ViewPointType
        {
            get { return (ViewPointType)GetValue(ViewPointTypeProperty); }
            set { SetValue(ViewPointTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewPointTypeProperty =
            DependencyProperty.Register(nameof(ViewPointType), typeof(ViewPointType), typeof(LocaleViewer),
            new PropertyMetadata(ViewPointType.Undefined, ViewPointType_Changed));

        private static void ViewPointType_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            if (depends is LocaleViewer _viewer)
            {
                // create view point and make it the camera
                var _vpType = (ViewPointType)args.NewValue;

                // setting to undefined leaves the last camera in place
                if (_vpType != ViewPointType.Undefined)
                {
                    _viewer._ViewPoint = BaseViewPoint.CreateViewPoint((ViewPointType)args.NewValue);
                    _viewer._ViewPoint.ViewPointState = _viewer.ViewPointState;
                    _viewer.v3dLocale.DataContext = _viewer._ViewPoint;
                    //_viewer.v3dLocale.Camera = _viewer._ViewPoint.ViewPointCamera;

                    if (_viewer.SensorHost != null)
                    {
                        // if we already have a local view model, then grab it's sensor host info
                        _viewer._ViewPoint.SensorHost = _viewer.SensorHost;
                        _viewer.RedrawPresentations(false);
                        //_viewer._Visualization.ReAnimateTransients();
                    }
                }
            }
        }
        #endregion

        #region public uint ViewPointState { get; set; } (DEPENDENCY)
        public uint ViewPointState
        {
            get { return (uint)GetValue(ViewPointStateProperty); }
            set { SetValue(ViewPointStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewPointStateProperty =
            DependencyProperty.Register(nameof(ViewPointState), typeof(uint), typeof(LocaleViewer),
            new PropertyMetadata((uint)0, ViewPointState_Changed));

        private static void ViewPointState_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            if (depends is LocaleViewer _viewer)
            {
                // ignore changes to undefined viewpoint
                if (_viewer.ViewPointType != ViewPointType.Undefined)
                {
                    _viewer._ViewPoint.ViewPointState = (uint)args.NewValue;
                    if ((_viewer.ViewPointType == ViewPointType.GameBoard)
                        && (_viewer.LocaleViewModel.ShowOverlay || _viewer.LocaleViewModel.ShowExtraMarkers || _viewer.LocaleViewModel.ShowSounds))
                    {
                        _viewer.RedrawOverlay();
                    }
                }
            }
        }
        #endregion

        #region public SensorHostInfo SensorHost { get; set; } DEPENDENCY
        public SensorHostInfo SensorHost
        {
            get { return (SensorHostInfo)GetValue(SensorHostProperty); }
            set { SetValue(SensorHostProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Shadings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SensorHostProperty =
            DependencyProperty.Register(nameof(SensorHost), typeof(SensorHostInfo), typeof(LocaleViewer),
            new UIPropertyMetadata(new PropertyChangedCallback(Sensors_Changed)));

        private static void Sensors_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            if (depends is LocaleViewer _view)
            {
                if (args.OldValue is SensorHostInfo _old
                    && !_old.ID.Equals(_view.SensorHost?.ID))
                {
                    // switched sensorHost, not just a simple refresh
                    _view._ViewPoint = BaseViewPoint.CreateViewPoint(_view.ViewPointType);
                    _view._ViewPoint.ViewPointState = _view.ViewPointState;
                    _view.v3dLocale.DataContext = _view._ViewPoint;
                }

                // NOTE: Sensors may change from AimPointChanges, so Overlay should be redrawn as well
                _view._ViewPoint.SensorHost = _view.SensorHost;
                _view.RedrawOverlay();
            }
        }
        #endregion

        #region public LocaleViewModel LocaleViewModel { get; set; } DEPENDENCY
        public LocaleViewModel LocaleViewModel
        {
            get { return (LocaleViewModel)GetValue(LocaleViewModelProperty); }
            set { SetValue(LocaleViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Shadings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocaleViewModelProperty =
            DependencyProperty.Register(nameof(LocaleViewModel), typeof(LocaleViewModel), typeof(LocaleViewer),
            new UIPropertyMetadata(new PropertyChangedCallback(LocalViewModel_Changed)));

        private static void LocalViewModel_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            if (depends is LocaleViewer _view)
            {
                if (_view.LocaleViewModel != null)
                {
                    _view._ViewPoint.SensorHost = _view.LocaleViewModel.Sensors;
                    _view.RedrawTerrain();
                    _view.RedrawPresentations(true);
                    _view.RedrawOverlay();
                    _view.RedrawTransientVisualizers();
                }
            }
        }
        #endregion

        #region public ICellLocation TargetCell { get; set; } DEPENDENCY
        public ICellLocation TargetCell
        {
            get { return (ICellLocation)GetValue(TargetCellProperty); }
            set { SetValue(TargetCellProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetCellProperty =
            DependencyProperty.Register(nameof(TargetCell), typeof(ICellLocation), typeof(LocaleViewer), new UIPropertyMetadata(new PropertyChangedCallback(Overlay_Changed)));
        #endregion

        #region public bool ShowExtraMarkers { get; set; } DEPENDENCY
        public bool ShowExtraMarkers
        {
            get { return (bool)GetValue(ShowExtraMarkersProperty); }
            set { SetValue(ShowExtraMarkersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowExtraMarkersProperty =
            DependencyProperty.Register(nameof(ShowExtraMarkers), typeof(bool), typeof(LocaleViewer), new UIPropertyMetadata(new PropertyChangedCallback(Overlay_Changed)));
        #endregion

        #region public bool ShowSounds { get; set; } DEPENDENCY
        public bool ShowSounds
        {
            get { return (bool)GetValue(ShowSoundsProperty); }
            set { SetValue(ShowSoundsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowSoundsProperty =
            DependencyProperty.Register(nameof(ShowSounds), typeof(bool), typeof(LocaleViewer), new UIPropertyMetadata(new PropertyChangedCallback(Overlay_Changed)));
        #endregion

        #region public bool ShowOverlay { get; set; } DEPENDENCY
        public bool ShowOverlay
        {
            get { return (bool)GetValue(ShowOverlayProperty); }
            set { SetValue(ShowOverlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowOverlayProperty =
            DependencyProperty.Register(nameof(ShowOverlay), typeof(bool), typeof(LocaleViewer), new UIPropertyMetadata(new PropertyChangedCallback(Overlay_Changed)));
        #endregion

        #region private static void Overlay_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        private static void Overlay_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            if (depends is LocaleViewer _view)
            {
                if (_view.LocaleViewModel != null)
                {
                    _view.RedrawOverlay();
                }
            }
        }
        #endregion

        #region public IEnumerable<PresentationInfo> SelectedPresentations { get; set; } DEPENDENCY
        public IEnumerable<PresentationInfo> SelectedPresentations
        {
            get { return (IEnumerable<PresentationInfo>)GetValue(SelectedPresentationsProperty); }
            set { SetValue(SelectedPresentationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Shadings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPresentationsProperty =
            DependencyProperty.Register("SelectedPresentations", typeof(IEnumerable<PresentationInfo>), typeof(LocaleViewer),
            new UIPropertyMetadata(new PropertyChangedCallback(Presentations_Changed)));

        private static void Presentations_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            if (depends is LocaleViewer _view)
            {
                if (_view.LocaleViewModel != null)
                {
                    _view.RedrawPresentations(true);
                    _view.RedrawOverlay();
                    //_view._Visualization.ReAnimateTransients();
                }
            }
        }
        #endregion

        #region public IEnumerable<ICellLocation> QueuedLocations { get; set; } DEPENDENCY
        public IEnumerable<ICellLocation> QueuedLocations
        {
            get { return (IEnumerable<ICellLocation>)GetValue(QueuedLocationsProperty); }
            set { SetValue(QueuedLocationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QueuedCells.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QueuedLocationsProperty =
            DependencyProperty.Register(@"QueuedLocations", typeof(IEnumerable<ICellLocation>), typeof(LocaleViewer), new UIPropertyMetadata(new PropertyChangedCallback(Overlay_Changed)));
        #endregion

        #region public IEnumerable<Point3D> QueuedPoints { get; set; } DEPENDENCY
        public IEnumerable<Point3D> QueuedPoints
        {
            get { return (IEnumerable<Point3D>)GetValue(QueuedPointsProperty); }
            set { SetValue(QueuedPointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QueuedCells.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QueuedPointsProperty =
            DependencyProperty.Register(nameof(QueuedPoints), typeof(IEnumerable<Point3D>), typeof(LocaleViewer), new UIPropertyMetadata(new PropertyChangedCallback(Overlay_Changed)));
        #endregion

        #region private BuildableGroup GenerateShadeInfoModel(LocalMapInfo map, ShadingInfo shadeInfo)
        private BuildableGroup GenerateShadeInfoModel(LocalMapInfo map, ShadingInfo shadeInfo)
        {
            var _localOpaque = new Model3DGroup();
            var _localAlpha = new Model3DGroup();
            var _globalContext = new BuildableContext();
            var _faces = shadeInfo.AnchorFaces;
            var _region = map.GetRoom(shadeInfo.ID);

            // loop over region
            var _z = shadeInfo.Z;
            var _y = shadeInfo.Y;
            var _x = shadeInfo.X;
            var _ovrZ = _z + shadeInfo.ZHeight;
            var _ovrY = _y + shadeInfo.YLength;
            var _ovrX = _x + shadeInfo.XLength;
            foreach (var _effect in shadeInfo.VisualEffects)
            {
                if (_effect != Visualize.VisualEffect.Skip)
                {
                    var _cLoc = new CellPosition(shadeInfo.Z + _z, shadeInfo.Y + _y, shadeInfo.X + _x);

                    // render faces
                    var _cellOpaque = new Model3DGroup();
                    var _cellAlpha = new Model3DGroup();
                    var _group = new BuildableGroup { Context = _globalContext, Opaque = _cellOpaque, Alpha = _cellAlpha };
                    void _drawFace(int z, int y, int x, AnchorFace face, Vector3D vector3D)
                    {
                        CellStructureInfo _space;
                        var _panel = shadeInfo?.PanelShadings
                            .FirstOrDefault(_ps => _ps.AnchorFace == face && _ps.Z == z && _ps.Y == y && _ps.X == x);
                        if (_panel != null)
                        {
                            // panel override space
                            _space = map.GetCellSpaceInfo(_panel.CellSpace);
                        }
                        else
                        {
                            // normal structural space
                            _space = map[z, y, x];
                        }
                        _space.AddOuterSurface(_group, z, y, x, face, _effect, vector3D, _region);
                    }
                    if (_faces.Contains(AnchorFace.ZHigh)) _drawFace(_z - 1, _y, _x, AnchorFace.ZHigh, _ZDrop);
                    if (_faces.Contains(AnchorFace.ZLow)) _drawFace(_z + 1, _y, _x, AnchorFace.ZLow, _ZLift);
                    if (_faces.Contains(AnchorFace.YHigh)) _drawFace(_z, _y - 1, _x, AnchorFace.YHigh, _YDrop);
                    if (_faces.Contains(AnchorFace.YLow)) _drawFace(_z, _y + 1, _x, AnchorFace.YLow, _YLift);
                    if (_faces.Contains(AnchorFace.XHigh)) _drawFace(_z, _y, _x - 1, AnchorFace.XHigh, _XDrop);
                    if (_faces.Contains(AnchorFace.XLow)) _drawFace(_z, _y, _x + 1, AnchorFace.XLow, _XLift);

                    var _currStruc = map[_z, _y, _x];
                    _currStruc.AddInnerStructures(_group, _z, _y, _x, _effect);

                    // outwardly shaded faces that may need to be generated because the outward material blocks
                    // such as: large cylinder quarter faces
                    _currStruc.AddExteriorSurface(_group, _z, _y, _x, AnchorFace.ZHigh, _effect, _region);
                    _currStruc.AddExteriorSurface(_group, _z, _y, _x, AnchorFace.ZLow, _effect, _region);
                    _currStruc.AddExteriorSurface(_group, _z, _y, _x, AnchorFace.YHigh, _effect, _region);
                    _currStruc.AddExteriorSurface(_group, _z, _y, _x, AnchorFace.YLow, _effect, _region);
                    _currStruc.AddExteriorSurface(_group, _z, _y, _x, AnchorFace.XHigh, _effect, _region);
                    _currStruc.AddExteriorSurface(_group, _z, _y, _x, AnchorFace.XLow, _effect, _region);

                    // merge buildables
                    if (_cellOpaque.Children.Count > 0)
                    {
                        _cellOpaque.Transform = new TranslateTransform3D((_x - shadeInfo.X) * 5d, (_y - shadeInfo.Y) * 5d, (_z - shadeInfo.Z) * 5d);
                        _cellOpaque.Freeze();
                        _localOpaque.Children.Add(_cellOpaque);
                    }
                    if (_cellAlpha.Children.Count > 0)
                    {
                        _cellAlpha.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                        _cellAlpha.Freeze();
                        _localAlpha.Children.Add(_cellAlpha);
                    }
                }

                #region cell coordinates
                // end of loop counter increments
                _x++;
                if (_x >= _ovrX)
                {
                    _x = shadeInfo.X;
                    _y++;
                    if (_y >= _ovrY)
                    {
                        _y = shadeInfo.Y;
                        _z++;

                        // NOTE: this is a failsafe, shouldn't have more cells than effects
                        if (_z >= _ovrZ)
                            break;
                    }
                }
                #endregion
            }

            // merge buildable context
            var _move = new TranslateTransform3D(shadeInfo.X * 5d, shadeInfo.Y * 5d, shadeInfo.Z * 5d);
            Model3DGroup _getFinal(bool alpha, Model3DGroup gathered)
            {
                var _final = new Model3DGroup();
                foreach (var _m in _globalContext.GetModel3D(alpha))
                    _final.Children.Add(_m);
                if (gathered.Children.Count > 0)
                {
                    gathered.Transform = _move;
                    _final.Children.Add(gathered);
                }
                if (_final.Children.Count > 0)
                {
                    _final.Freeze();
                    return _final;
                }
                return null;
            }

            // return
            return new BuildableGroup
            {
                Alpha = _getFinal(true, _localAlpha),
                Opaque = _getFinal(false, _localOpaque)
            };
        }
        #endregion

        #region public void RedrawTerrain()
        public void RedrawTerrain()
        {
            if (LocaleViewModel != null)
            {
                var _shadings = LocaleViewModel.Shadings;
                var _map = LocaleViewModel.Map;
                if ((_shadings != null) && (_map != null))
                {
                    _Visualization.SetTerrain(from _shadeInfo in _shadings.AsParallel()
                                              select GenerateShadeInfoModel(_map, _shadeInfo));
                }

                #region inner brush
                PointEffect _effect = null;
                switch (ViewPointType)
                {
                    case ViewPointType.FirstPerson:
                        _effect = SensorHost.CenterEffect;
                        break;
                    case ViewPointType.AimPoint:
                        _effect = SensorHost.AimCellEffect;
                        break;
                    case ViewPointType.ThirdPerson:
                    default:
                        _effect = SensorHost.ThirdCameraEffect;
                        break;
                }

                if ((_effect == null) || string.IsNullOrEmpty(_effect.BrushKey) || string.IsNullOrEmpty(_effect.BrushSet))
                {
                    rectInner.Fill = null;
                }
                else
                {
                    var _brushes = _map.GetBrushCollectionViewModel(_effect.BrushSet);
                    if (_brushes != null)
                    {
                        var _myBrush = _brushes.BrushDefinitions.FirstOrDefault(_b => _b.BrushKey == _effect.BrushKey);
                        if (_myBrush != null)
                            rectInner.Fill = _myBrush.GetBrush(_effect.Effect);
                        else
                            rectInner.Fill = null;
                    }
                    else
                        rectInner.Fill = null;
                }
                #endregion
            }
        }
        #endregion

        #region attached PresentationInfo property for UI selection support
        public static PresentationInfo GetPresentationInfo(DependencyObject obj)
        {
            return (PresentationInfo)obj.GetValue(PresentationInfoProperty);
        }

        public static void SetPresentationInfo(DependencyObject obj, PresentationInfo value)
        {
            obj.SetValue(PresentationInfoProperty, value);
        }

        // Using a DependencyProperty as the backing store for PresentationInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PresentationInfoProperty =
            DependencyProperty.RegisterAttached(@"PresentationInfo", typeof(PresentationInfo), typeof(LocaleViewer),
            new UIPropertyMetadata(null));
        #endregion

        public Tuple<ModelUIElement3D, PresentationInfo> GetModelPresentation(Guid id)
            => (from _m in grpTokens.Children.OfType<ModelUIElement3D>()
                let _p = GetPresentationInfo(_m)
                where (_p != null) && (_p.PresentingIDs.Contains(id))
                select new Tuple<ModelUIElement3D, PresentationInfo>(_m, _p))
            .FirstOrDefault();

        #region public public RedrawPresentations()
        public void RedrawPresentations(bool animateMove)
        {
            if ((LocaleViewModel != null) && (SensorHost != null))
            {
                var _icons = LocaleViewModel.ObservableActor.IconResolver;
                var _models = LocaleViewModel.Map;
                var _presentations = LocaleViewModel.Presentations;
                var _highlighted = SelectedPresentations;
                if ((_icons != null)
                    && (_models != null)
                    && (_highlighted != null)
                    && (_presentations != null))
                {
                    var _id = Guid.Parse(SensorHost.ID);
                    var _drawSelf = _ViewPoint.IsSelfVisible;
                    var _state = LocaleViewModel.ObservableActor.Actor.SerialState;
                    // TODO: animation support
                    _Visualization.SetTokens(
                        from _p in _presentations
                        where _drawSelf || !_p.PresentingIDs.Contains(_id)
                        select _p.GetPresentable(_icons, _models, _highlighted, _ViewPoint.ViewPosition,
                            SensorHost.Heading, LocaleViewModel), animateMove, _state);
                }
            }
        }
        #endregion

        #region public void RedrawTransientVisualizers()
        public void RedrawTransientVisualizers()
        {
            if (LocaleViewModel != null)
            {
                _Visualization.SetTransients(LocaleViewModel.TransientVisualizers.Select(_tv => _tv.GetTransientVisualizer()));
                _Visualization.AnimateTransients();
            }
        }
        #endregion

        private ICellLocation SourceCell
            => SensorHost?.AimCell;

        private Point3D SourcePoint
            => SensorHost?.AimPoint3D ?? new Point3D();

        #region public void PointToPresentation(PresentationInfo presentation)
        public void PointToPresentation(PresentationInfo presentation)
        {
            var _sourcePt = SourcePoint;
            var _state = new GameBoardViewPointState(ViewPointState);
            var _width = ViewPointType == ViewPointType.GameBoard
                ? 0.5d + (_state.WidthCells * 0.1d)
                : 0.5d;
            if ((presentation.XLength == 1) && (presentation.YLength == 1) && (presentation.ZHeight == 1))
            {
                grpDrawing.Children.Add(DrawingTools.Line(
                    IGeometricHelper.GetPoint(presentation), _sourcePt,
                    DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
            }
            else
            {
                if (presentation is ModelPresentationInfo _model)
                {
                    var _tx = ModelGenerator.GetTransform(_model, _model.GetPoint3D(),
                        out var _, out var _,
                        out var _, out var _,
                        out var _);
                    _tx.Freeze();

                    var _pt = _model.IsAdjustingPosition
                        ? new Point3D()
                        : (_model.IsFullOrigin ? new Point3D() : new Point3D(0, 0, _model.CubeFitScale.Z * 2.5d));

                    grpDrawing.Children.Add(DrawingTools.Line(
                        _tx.Transform(_pt), _sourcePt,
                        DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                }
                else
                {
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.LowerZ, presentation.LowerY, presentation.LowerX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.LowerZ, presentation.LowerY, presentation.UpperX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.LowerZ, presentation.UpperY, presentation.LowerX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.LowerZ, presentation.UpperY, presentation.UpperX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.UpperZ, presentation.LowerY, presentation.LowerX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.UpperZ, presentation.LowerY, presentation.UpperX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.UpperZ, presentation.UpperY, presentation.LowerX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                    grpDrawing.Children.Add(DrawingTools.Line(
                        IGeometricHelper.GetPoint(new CellPosition(presentation.UpperZ, presentation.UpperY, presentation.UpperX)),
                        _sourcePt, DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue }), _width));
                }
            }
        }
        #endregion

        public void PointToCell(ICellLocation location)
        {
            DrawingTools.CellGlow(location.Z, location.Y, location.X);
            var _sourcePt = SourcePoint;
            grpDrawing.Children.Add(DrawingTools.Line(
                IGeometricHelper.GetPoint(location), _sourcePt,
                DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue })));
        }

        public void CellToCell(ICellLocation location)
        {
            DrawingTools.CellGlow(location.Z, location.Y, location.X);
            var _cell = SourceCell;
            DrawingTools.CellGlow(_cell.Z, _cell.Y, _cell.X, DrawingTools.Highlighter(Colors.Orange, 0.37d));
            var _sourcePt = IGeometricHelper.GetPoint(_cell);
            grpDrawing.Children.Add(DrawingTools.Line(
                IGeometricHelper.GetPoint(location), _sourcePt,
                DrawingTools.LineBrush(new Color[] { Colors.Yellow, Colors.Green, Colors.Blue })));
        }

        public void ClearPointerLines()
        {
            grpDrawing.Children.Clear();
        }

        #region public void RedrawOverlay()
        public void RedrawOverlay()
        {
            if (LocaleViewModel != null)
            {
                var _highlighted = SelectedPresentations;
                if (_highlighted != null)
                {
                    // clear
                    ClearPointerLines();

                    // gather
                    if (_highlighted.Any())
                    {
                        // draw lines
                        foreach (var _present in _highlighted)
                        {
                            PointToPresentation(_present);
                        }
                    }
                }

                // overlay visible if explicitly on, or there are queued cells to show
                var _qCells = (QueuedLocations?.Any() ?? false);
                var _qPts = (QueuedPoints?.Any() ?? false);
                v3dOverlay.Visibility = (LocaleViewModel.ShowOverlay || _qCells || _qPts
                    || LocaleViewModel.ShowSounds || LocaleViewModel.ShowExtraMarkers)
                    ? Visibility.Visible
                    : Visibility.Hidden;
                grpOverlay.Children.Clear();
                grpMarkers.Children.Clear();

                if (LocaleViewModel.ShowOverlay)
                {
                    if (TargetCell != null)
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        var _width = ViewPointType == ViewPointType.GameBoard
                            ? 0.5d + (_state.WidthCells * 0.1d)
                            : 0.5d + ((TargetCell.GetPoint() - SourcePoint).Length / 5 * 0.25);
                        switch (LocaleViewModel.ObservableActor.AimPointActivation)
                        {
                            case AimPointActivation.TargetCell:
                                {
                                    // show targeting cell
                                    grpOverlay.Children.Add(DrawingTools.BoxEdges((CellPosition)TargetCell, Brushes.Red, _width));
                                }
                                break;
                            case AimPointActivation.TargetIntersection:
                                {
                                    // show targeting cell
                                    grpOverlay.Children.Add(DrawingTools.CrossHairs(TargetCell.Point3D(), Brushes.Firebrick, _width, 2.1d));
                                }
                                break;
                        }
                    }
                }

                if (LocaleViewModel.ShowSounds)
                {
                    foreach (var _sound in LocaleViewModel.Sounds)
                    {
                        // TODO: InputBindings
                        var _tip = new ToolTipHelper(Resources)
                        {
                            ToolTipContent = string.Join("\n", _sound.Stream.Select(_s => _s.Description))
                        };
                        var _elem = new ModelUIElement3D
                        {
                            Model = ModelGenerator.GeneratePointer(SourcePoint, _sound.Vector,
                                _sound.Range, _sound.TimeFade, _sound.IsActive ? _ActiveSound : _InActiveSound)
                        };

                        // tool tip
                        _elem.MouseEnter += _tip.OnMouseEnter;
                        _elem.MouseLeave += _tip.OnMouseLeave;
                        grpMarkers.Children.Add(_elem);
                    }
                }

                if (LocaleViewModel.ShowExtraMarkers)
                {
                    foreach (var _marker in LocaleViewModel.ExtraInfos.OfType<ExtraInfoMarkerInfo>())
                    {
                        // TODO: InputBindings
                        var _tip = new ToolTipHelper(Resources)
                        {
                            ToolTipContent = new ContentStack(Orientation.Vertical, _marker.Informations.ToArray())
                            // string.Join("\n", _marker.Informations.Select(_i => _i.Message))
                        };

                        // vector
                        var _vector = _marker.Region.GetPoint3D() - SourcePoint;
                        var _range = _marker.DirectionOnly ? Math.Min(7.5d, _vector.Length) : _vector.Length;

                        var _elem = new ModelUIElement3D
                        {
                            Model = ModelGenerator.GenerateInfoMarker(SourcePoint, _marker.Region.GetPoint3D(),
                                _marker.DirectionOnly, _Marker)
                        };

                        // tool tip
                        _elem.MouseEnter += _tip.OnMouseEnter;
                        _elem.MouseLeave += _tip.OnMouseLeave;
                        grpMarkers.Children.Add(_elem);
                    }
                }

                if (_qCells)
                {
                    // show queued cells
                    foreach (var _cell in QueuedLocations)
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        var _width = ViewPointType == ViewPointType.GameBoard
                            ? 0.5d + (_state.WidthCells * 0.1d)
                            : 0.5d + ((_cell.GetPoint() - SourcePoint).Length / 5 * 0.25);
                        grpOverlay.Children.Add(DrawingTools.BoxEdges((CellPosition)_cell, Brushes.Cyan, _width, _width / 8d));
                    }
                }

                if (_qPts)
                {
                    foreach (var _pt in QueuedPoints)
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        var _width = ViewPointType == ViewPointType.GameBoard
                            ? 0.65d + (_state.WidthCells * 0.1d)
                            : 0.75d + ((_pt - SourcePoint).Length / 5 * 0.25);
                        grpOverlay.Children.Add(DrawingTools.CrossHairs(_pt, Brushes.Turquoise, _width, 1.8d));
                    }
                }
            }
        }
        #endregion

        #region IPresentationInputBinder Members

        public IEnumerable<InputBinding> GetBindings(Presentable presentable)
            => LocaleViewModel?.GetBindings(presentable) ?? new InputBinding[] { };

        #endregion

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.F10)
            {
                e.Handled = true;
            }
        }
    }
}
