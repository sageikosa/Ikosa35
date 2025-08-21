using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Visualize;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for BrushCollectionPartControl.xaml
    /// </summary>
    public partial class BrushCollectionPartControl : UserControl
    {
        public BrushCollectionPartControl()
        {
            InitializeComponent();
            DataContextChanged += BrushCollectionPartControl_DataContextChanged;
        }

        private void RebindDataContext()
        {
            var _collection = DataContext;
            DataContext = null;
            DataContext = _collection;
        }

        private void BrushCollectionPartControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Collection?.IResolveBitmapImageParent is Model3DPart)
                && !(Collection?.IResolveBitmapImageParent is MetaModel))
            {
                itemsMaterials.ItemContainerStyle = Resources[@"styleModelBound"] as Style;
            }
            else
            {
                itemsMaterials.ItemContainerStyle = Resources[@"styleGeneral"] as Style;
            }
        }

        public BrushCollectionPart Collection => DataContext as BrushCollectionPart;

        #region private void btnAdd_Click(object sender, RoutedEventArgs e)
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var _addMenu = new ContextMenu();
            var _factory = new FrameworkElementFactory(typeof(WrapPanel));
            _factory.SetValue(WrapPanel.MaxWidthProperty, 768d);
            _addMenu.ItemsPanel = new ItemsPanelTemplate(_factory);
            _addMenu.ItemTemplate = Resources[@"mtImage"] as DataTemplate;
            foreach (var _image in Collection.ResolvableImages)
            {
                _addMenu.Items.Add(_image);
            }
            _addMenu.PlacementTarget = btnAdd;
            _addMenu.Placement = PlacementMode.Bottom;
            _addMenu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(ContextMenu_Click));
            _addMenu.IsOpen = true;
        }
        #endregion

        private void ctxBefore_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            GetMenuImages(e);
        }

        private void ctxAfter_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            GetMenuImages(e);
        }

        private void GetMenuImages(RoutedEventArgs e)
        {
            if (e.Source is MenuItem _menu)
            {
                _menu.Items.Clear();
                foreach (var _image in Collection.ResolvableImages)
                {
                    _menu.Items.Add(_image);
                }
            }
        }

        #region private void ContextMenu_Click(object sender, RoutedEventArgs e)
        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // get selected bitmap
            var _bipl = (e.OriginalSource as MenuItem).Header as BitmapImagePartListItem;

            // create a material group with an Ikosa resolveable imageBrush (and resolve it!)
            var _brushDef = new ImageBrushDefinition(Collection)
            {
                BrushKey = _bipl.BitmapImagePart.Name,
                ImageKey = _bipl.BitmapImagePart.Name,
                Opacity = 1d
            };

            // figure out where to add it
            var _menu = (sender is ContextMenu)
                ? (sender as ContextMenu)
                : (sender as MenuItem).Parent as ContextMenu;
            if (_menu != null)
            {
                if (_menu.DataContext is BrushCollectionPart)
                {
                    // will add to end
                    Collection.BrushDefinitions.Add(_brushDef);
                }
                else
                {
                    var _mItem = e.Source as MenuItem;
                    var _after = _mItem.Header.ToString().Equals(@"Add After");
                    var _index = Convert.ToInt32(_mItem.Tag) + (_after ? 1 : 0);
                    Collection.BrushDefinitions.Insert(_index, _brushDef);
                }

                // clear and reset data context (since Collection isn't a dependency collection)
                RebindDataContext();
            }
            e.Handled = true;
        }
        #endregion

        #region private void cmDelete_Click(object sender, RoutedEventArgs e)
        private void cmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem _mItem)
            {
                Collection.BrushDefinitions.RemoveAt(Convert.ToInt32(_mItem.Tag));

                // rebind
                RebindDataContext();
            }
            e.Handled = true;
        }
        #endregion

        #region private void btnAddSolid_Click(object sender, RoutedEventArgs e)
        private void btnAddSolid_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new SolidColor(Collection.BrushDefinitions, string.Empty, Colors.Magenta, false)
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                Collection.BrushDefinitions.Add(_dlg.GetBrushDefinition());
                RebindDataContext();
            }
        }
        #endregion

        #region private void btnAddGradient_Click(object sender, RoutedEventArgs e)
        private void btnAddGradient_Click(object sender, RoutedEventArgs e)
        {
            var _stops = new GradientStopCollection
            {
                new GradientStop(Colors.Black, 0),
                new GradientStop(Colors.White, 1)
            };
            var _dlg = new LinearGradient(Collection.BrushDefinitions, string.Empty, _stops, 0d, false,
                ColorInterpolationMode.SRgbLinearInterpolation, GradientSpreadMethod.Pad, 1)
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                Collection.BrushDefinitions.Add(_dlg.GetBrushDefinition());
                RebindDataContext();
            }
        }
        #endregion

        #region private void btnAddStacked_Click(object sender, RoutedEventArgs e)
        private void btnAddStacked_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new StackedBrush(Collection.BrushDefinitions, string.Empty, false, [])
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                Collection.BrushDefinitions.Add(_dlg.GetBrushDefinition());
                RebindDataContext();
            }
        }
        #endregion

        #region private void itemsMaterials_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        private void itemsMaterials_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (itemsMaterials.SelectedItem != null)
            {
                var _kvp = (KeyValuePair<int, BrushDefinition>)itemsMaterials.SelectedItem;
                if (_kvp.Value is SolidBrushDefinition)
                {
                    var _solid = _kvp.Value as SolidBrushDefinition;
                    var _opacity = _solid.Opacity;
                    var _dlg = new SolidColor(Collection.BrushDefinitions, _solid.BrushKey, _solid.Color, true)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _brushDef = _dlg.GetBrushDefinition();
                        _brushDef.Opacity = _opacity;
                        var _index = Collection.BrushDefinitions.IndexOf(_solid);
                        Collection.BrushDefinitions.Remove(_solid);
                        Collection.BrushDefinitions.Insert(_index, _brushDef);
                        RebindDataContext();
                    }
                }
                else if (_kvp.Value is LinearGradientBrushDefinition)
                {
                    var _linear = _kvp.Value as LinearGradientBrushDefinition;
                    var _opacity = _linear.Opacity;
                    var _dlg = new LinearGradient(Collection.BrushDefinitions, _linear.BrushKey, _linear.GradientStops.Clone(), _linear.Angle, true,
                        _linear.ColorInterpolationMode, _linear.SpreadMethod, _linear.Size)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _brushDef = _dlg.GetBrushDefinition();
                        _brushDef.Opacity = _opacity;
                        var _index = Collection.BrushDefinitions.IndexOf(_linear);
                        Collection.BrushDefinitions.Remove(_linear);
                        Collection.BrushDefinitions.Insert(_index, _brushDef);
                        RebindDataContext();
                    }
                }
                else if (_kvp.Value is StackedBrushDefinition)
                {
                    var _stacked = _kvp.Value as StackedBrushDefinition;
                    var _dlg = new StackedBrush(Collection.BrushDefinitions, _stacked.BrushKey, true, (new BrushCollection()).AddRange(_stacked.Components))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _brushDef = _dlg.GetBrushDefinition();
                        var _index = Collection.BrushDefinitions.IndexOf(_stacked);
                        Collection.BrushDefinitions.Remove(_stacked);
                        Collection.BrushDefinitions.Insert(_index, _brushDef);
                        RebindDataContext();
                    }
                }
                else
                {
                    DialogRenameBrush(_kvp.Value);
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cmRename
        private void cmRename_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem _mItem)
            {
                var _idx = Convert.ToInt32(_mItem.Tag);
                var _brush = Collection.BrushDefinitions[_idx];
                DialogRenameBrush(_brush);
            }
            e.Handled = true;
        }
        #endregion

        #region private void DialogRenameBrush(BrushDefinition _brush)
        private void DialogRenameBrush(BrushDefinition _brush)
        {
            if ((Collection.IResolveBitmapImageParent is Model3DPart _mdl) && !(_mdl is MetaModel))
            {
                var _dlg = new SelectRename(_brush.BrushKey, _mdl.VisualEffectMaterialKeys)
                {
                    Owner = Window.GetWindow(this)
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    _brush.BrushKey = _dlg.GetName();
                }
            }
            else
            {
                var _dlg = new FullRename(_brush.BrushKey)
                {
                    Owner = Window.GetWindow(this)
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    _brush.BrushKey = _dlg.GetName();
                }
            }

            // rebind
            RebindDataContext();
        }
        #endregion

        #region private void ctxRename_SubmenuOpened(object sender, RoutedEventArgs e)
        private void ctxRename_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem _ctx)
            {
                _ctx.Items.Clear();
                if ((Collection.IResolveBitmapImageParent is Model3DPart _mdl) && !(_mdl is MetaModel))
                {
                    foreach (var _key in _mdl.VisualEffectMaterialKeys)
                    {
                        _ctx.Items.Add(_key);
                    }
                }
            }
        }
        #endregion

        #region private void RenameMenu_Click(object sender, RoutedEventArgs e)
        private void RenameMenu_Click(object sender, RoutedEventArgs e)
        {
            // header of item being clicked
            var _newKey = (e.OriginalSource as MenuItem).Header.ToString();

            // source of event is the Rename menu with an index tag
            var _mItem = e.Source as MenuItem;

            // get index, get brush, change to new name
            var _index = Convert.ToInt32(_mItem.Tag);
            var _brush = Collection.BrushDefinitions[_index];
            _brush.BrushKey = _newKey;

            // rebind
            RebindDataContext();
        }
        #endregion
    }
}
