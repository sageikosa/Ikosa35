using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Visualize.Packaging;
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MetaModelEditor.xaml
    /// </summary>
    public partial class MetaModelEditor : UserControl
    {
        public static RoutedCommand DefineBrush = new RoutedCommand();
        public static RoutedCommand ClearXRef = new RoutedCommand();
        public static RoutedCommand CloneFragment = new RoutedCommand();

        public MetaModelEditor()
        {
            InitializeComponent();
            tvwFolders.ItemContainerStyleSelector = new MetaModelEditorTreeViewStyleSelector(this);
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(MetaModelEditor_DataContextChanged);
        }

        public MetaModel MetaModel
            => DataContext as MetaModel;

        void MetaModelEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }

        #region private void Refresh()
        private void Refresh(bool renderOnly = false)
        {
            mdlHolder.Children.Clear();
            var _meta = MetaModel;
            if (_meta != null)
            {
                MetaModelResolutionStack.IsHitTestable = true;
                _meta.FlushCache();
                var _mdl = _meta.ResolveModel();
                MetaModelResolutionStack.IsHitTestable = false;
                if (_mdl != null)
                {
                    mdlHolder.Children.Add(_mdl);
                }

                if (!renderOnly)
                {
                    tvwFolders.ItemsSource = null;
                    tvwFolders.ItemsSource = MetaModel.State.RootList;
                    lvwDefaultBrushes.ItemsSource = null;
                    lvwDefaultBrushes.ItemsSource = MetaModel.State.DefaultBrushes.Ordered;
                    lvwDefaultInts.ItemsSource = null;
                    lvwDefaultInts.ItemsSource = MetaModel.State.MasterIntReferences.Ordered;
                    lvwDefaultDoubles.ItemsSource = null;
                    lvwDefaultDoubles.ItemsSource = MetaModel.State.MasterDoubleReferences.Ordered;
                }
            }
        }
        #endregion

        private void ClearSelection()
        {
            var _meta = MetaModel;
            if (_meta != null)
            {
                _meta.State.RootFragment.UnSelect();
            }
            if (CurrentFragGeometry != null)
            {
                CurrentFragGeometry.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_geom_PropertyChanged);
            }

            grdFragOffset.DataContext = null;
            grdFragOrigin.DataContext = null;
            grdFragRotations.DataContext = null;
            grdFragScale.DataContext = null;
            lvwIntVals.DataContext = null;
            lvwDoubleVals.DataContext = null;
        }

        #region private void btnRefresh_Click(object sender, RoutedEventArgs e)
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            Refresh();
        }
        #endregion

        #region private void YieldCheck_Click(object sender, RoutedEventArgs e)
        private void YieldCheck_Click(object sender, RoutedEventArgs e)
        {
            Refresh(true);
        }
        #endregion

        #region Brush Assign
        private void ctxBrushAssign_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem _menu = e.Source as MenuItem;
            if (_menu != null)
            {
                _menu.Items.Clear();
                foreach (var _brushDef in MetaModel.ResolvableBrushes.OrderBy(_b => _b.BrushDefinition.BrushKey))
                {
                    _menu.Items.Add(_brushDef.BrushDefinition);
                }
            }
        }

        private void ctxBrushAssign_Click(object sender, RoutedEventArgs e)
        {
            var _menu = sender as MenuItem;
            if (_menu != null)
            {
                var _bXRef = _menu.Tag as BrushCrossRefNode;
                if (_bXRef != null)
                {
                    var _bDef = (e.OriginalSource as MenuItem).Header as BrushDefinition;
                    if (_bDef != null)
                    {
                        _bXRef.BrushKey = _bDef.BrushKey;
                        Refresh();
                    }
                }
            }
        }
        #endregion

        #region Fragment Assign
        private void ctxFragmentAssign_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem _menu = e.Source as MenuItem;
            if (_menu != null)
            {
                _menu.Items.Clear();
                foreach (var _fragItem in MetaModel.ResolvableFragments.OrderBy(_f => _f.MetaModelFragment.Name))
                {
                    _menu.Items.Add(_fragItem.MetaModelFragment);
                }
            }
        }

        private void ctxFragmentAssign_Click(object sender, RoutedEventArgs e)
        {
            var _menu = sender as MenuItem;
            if (_menu != null)
            {
                var _fXRef = _menu.Tag as MetaModelFragmentNode;
                if (_fXRef != null)
                {
                    var _frag = (e.OriginalSource as MenuItem).Header as MetaModelFragment;
                    if (_frag != null)
                    {
                        _fXRef.FragmentKey = _frag.Name;
                        _fXRef.IsExpanded = true;
                        Refresh();
                    }
                }
            }
        }
        #endregion

        #region cbClearXRef
        private void cbClearXRef_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _str = e.Parameter.ToString();
            if (_str == @"DefaultBrush")
            {
                var _brush = lvwDefaultBrushes.SelectedItem as BrushCrossRefNode;
                e.CanExecute = (_brush != null) && !string.IsNullOrEmpty(_brush.BrushKey);
            }
            else
            {
                var _root = e.Parameter as MetaModelRootNode;
                if (_root != null)
                {
                    e.CanExecute = true;
                }
                else
                {
                    var _frag = e.Parameter as MetaModelFragmentNode;
                    if (_frag != null)
                    {
                        e.CanExecute = !string.IsNullOrEmpty(_frag.FragmentKey);
                    }
                    else
                    {
                        var _brush = e.Parameter as BrushCrossRefNode;
                        e.CanExecute = (_brush != null) && !string.IsNullOrEmpty(_brush.BrushKey);
                    }
                }
            }
            e.Handled = true;
        }

        private void cbClearXRef_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _str = e.Parameter.ToString();
            if (_str == @"DefaultBrush")
            {
                var _brush = lvwDefaultBrushes.SelectedItem as BrushCrossRefNode;
                if (_brush != null)
                {
                    _brush.BrushKey = null;
                }

                Refresh();
            }
            else
            {
                var _frag = e.Parameter as MetaModelFragmentNode;
                if (_frag != null)
                {
                    _frag.Clear();
                    var _parent = MetaModel.State.FindParent(_frag);
                    if ((_parent != null) && (_parent.Count((f) => f.ReferenceKey == _frag.ReferenceKey) > 1))
                    {
                        _parent.Remove(_frag);
                    }

                    Refresh();
                }
                else
                {
                    var _brush = e.Parameter as BrushCrossRefNode;
                    if (_brush != null)
                    {
                        _brush.BrushKey = null;
                    }

                    Refresh();
                }
            }
            e.Handled = true;
        }
        #endregion

        #region Fragment Clone and Assign
        private void ctxFragmentClone_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem _menu = e.Source as MenuItem;
            if (_menu != null)
            {
                _menu.Items.Clear();
                foreach (var _fragItem in MetaModel.ResolvableFragments.OrderBy(_f => _f.MetaModelFragment.Name))
                {
                    _menu.Items.Add(_fragItem.MetaModelFragment);
                }
            }
        }

        private void ctxFragmentClone_Click(object sender, RoutedEventArgs e)
        {
            var _menu = sender as MenuItem;
            if (_menu != null)
            {
                // reference to clone
                var _fXRef = _menu.Tag as MetaModelFragmentNode;
                if (_fXRef != null)
                {
                    // get fragment
                    var _frag = (e.OriginalSource as MenuItem).Header as MetaModelFragment;
                    if (_frag != null)
                    {
                        // add a new reference with the fragment
                        var _parent = MetaModel.State.FindParent(_fXRef);
                        if (_parent != null)
                        {
                            _parent.Add(new MetaModelFragmentNode
                            {
                                ReferenceKey = _fXRef.ReferenceKey,
                                FragmentKey = _frag.Name,
                                IsExpanded = true
                            });
                        }

                        Refresh();
                    }
                }
            }
        }
        #endregion

        #region DefaultBrush Menu
        private void miDefaultBrush_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var _menu = e.Source as MenuItem;
            if (_menu != null)
            {
                _menu.Items.Clear();
                foreach (var _brushDef in MetaModel.ResolvableBrushes.OrderBy(_b => _b.BrushDefinition.BrushKey))
                {
                    _menu.Items.Add(_brushDef.BrushDefinition);
                }
            }
        }

        private void miDefaultBrush_Click(object sender, RoutedEventArgs e)
        {
            var _menu = sender as MenuItem;
            if (_menu != null)
            {
                var _bXRef = lvwDefaultBrushes.SelectedItem as BrushCrossRefNode;
                if (_bXRef != null)
                {
                    var _bDef = (e.OriginalSource as MenuItem).Header as BrushDefinition;
                    if (_bDef != null)
                    {
                        _bXRef.BrushKey = _bDef.BrushKey;
                        Refresh();
                    }
                }
            }
        }
        #endregion

        private FragGeometry CurrentFragGeometry
        {
            get { return grdFragOffset.DataContext as FragGeometry; }
        }

        #region private void tvwFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        private void tvwFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ClearSelection();

            var _root = e.NewValue as MetaModelRootNode;
            var _frag = e.NewValue as MetaModelFragmentNode;
            grdFragOffset.IsEnabled = (_frag != null) && (_root == null);
            grdFragOrigin.IsEnabled = (_frag != null) && (_root == null);
            grdFragRotations.IsEnabled = (_frag != null) && (_root == null);
            grdFragScale.IsEnabled = (_frag != null) && (_root == null);
            lvwIntVals.IsEnabled = _frag != null;
            lvwDoubleVals.IsEnabled = _frag != null;
            if ((_frag != null) && (_root == null))
            {
                var _geom = new FragGeometry(_frag);
                grdFragOffset.DataContext = _geom;
                grdFragOrigin.DataContext = _geom;
                grdFragRotations.DataContext = _geom;
                grdFragScale.DataContext = _geom;
                _geom.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_geom_PropertyChanged);
            }
            if (_frag != null)
            {
                _frag.IsSelected = true;
                lvwIntVals.DataContext = _frag;
                lvwDoubleVals.DataContext = _frag;
            }
            Refresh(true);
        }
        #endregion

        void _geom_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Refresh(true);
        }

        private void vp3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var _pt = e.GetPosition(vp3D);
            VisualTreeHelper.HitTest(vp3D, null, HitTestDown, new PointHitTestParameters(_pt));
        }

        #region private MetaModelFragmentNode GetModel3DGroupWithNode(Model3DGroup container, MetaModelFragmentNode current, Model3D modelHit)
        private MetaModelFragmentNode GetModel3DGroupWithNode(Model3DGroup container, MetaModelFragmentNode current, Model3D modelHit)
        {
            var _returnNode = MetaModelFragmentNode.GetMetaModelFragmentNode(container) ?? current;

            // does container hold the model
            if (container.Children.Contains(modelHit))
            {
                return _returnNode;
            }

            // container children
            var _result = (from _grp in container.Children.OfType<Model3DGroup>()
                           select GetModel3DGroupWithNode(_grp, _returnNode, modelHit))
                           .FirstOrDefault(_n => _n != null);

            // found nothing
            return _result;
        }
        #endregion

        #region private HitTestResultBehavior HitTestDown(HitTestResult result)
        private HitTestResultBehavior HitTestDown(HitTestResult result)
        {
            var _mesh = result as RayMeshGeometry3DHitTestResult;
            if (_mesh != null)
            {
                var _frag = GetModel3DGroupWithNode(mdlHolder, null, _mesh.ModelHit);
                var _root = _frag as MetaModelRootNode;
                if (_frag != null)
                {
                    ClearSelection();
                    if (_root == null)
                    {
                        grdFragOffset.IsEnabled = true;
                        grdFragOrigin.IsEnabled = true;
                        grdFragRotations.IsEnabled = true;
                        grdFragScale.IsEnabled = true;
                        var _geom = new FragGeometry(_frag);
                        grdFragOffset.DataContext = _geom;
                        grdFragOrigin.DataContext = _geom;
                        grdFragRotations.DataContext = _geom;
                        grdFragScale.DataContext = _geom;
                        _geom.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_geom_PropertyChanged);
                    }

                    lvwIntVals.IsEnabled = true;
                    lvwDoubleVals.IsEnabled = true;
                    _frag.IsSelected = true;
                    lvwIntVals.DataContext = _frag;
                    lvwDoubleVals.DataContext = _frag;

                    Refresh(true);
                    return HitTestResultBehavior.Stop;
                }
            }
            return HitTestResultBehavior.Continue;
        }
        #endregion

        private void DoubleUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Refresh(true);
        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Refresh(true);
        }
    }
}