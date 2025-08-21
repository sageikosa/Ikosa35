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
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for OpenerCloserEditor.xaml
    /// </summary>
    public partial class OpenerCloserEditor : UserControl
    {
        public OpenerCloserEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == OpenerCloserProperty)
            {
                cboOpenable.Items.Clear();
                cboOpenable.SelectedItem = null;
                foreach (var _open in AvailableOpenables)
                {
                    cboOpenable.Items.Add(_open);
                    if (_open.CoreObject == OpenerCloser.Openable)
                    {
                        cboOpenable.SelectedItem = _open;
                    }
                }
            }
            base.OnPropertyChanged(e);
        }

        #region public IEnumerable<PositionedObject> AvailableOpenables { get; }
        public IEnumerable<PositionedObject> AvailableOpenables
        {
            get
            {
                if (OpenerCloser != null)
                {
                    if (OpenableTarget is ItemBase)
                    {
                        yield return new PositionedObject
                        {
                            CoreObject = OpenableTarget
                        };
                    }
                    else
                    {
                        var _loc = OpenerCloser.GetLocated().Locator;
                        var _rgn = _loc.GeometricRegion;

                        // openables on own top-level and connected objects
                        foreach (var _obj in _loc.AllConnectedOf<IOpenable>())
                        {
                            yield return new PositionedObject
                            {
                                CoreObject = _obj
                            };
                        }

                        // openables on all other top-level objects
                        foreach (var _posObj in (from _l in (OpenerCloser.Setting as LocalMap).MapContext.AllTokensOf<Locator>()
                                                 where _l != _loc
                                                 let _o = _l.ICore as ICoreObject
                                                 where !(_o is IItemBase) && (_o is IOpenable)
                                                 let _lRgn = _l.GeometricRegion
                                                 select new PositionedObject
                                                 {
                                                     CoreObject = _o,
                                                     Distance = Math.Round(_rgn.NearDistance(_lRgn), 2),
                                                     ZOffset = _rgn.LowerZ - _lRgn.LowerZ,
                                                     YOffset = _rgn.LowerY - _lRgn.LowerY,
                                                     XOffset = _rgn.LowerX - _lRgn.LowerX
                                                 }).OrderBy(_po => _po.Distance))
                        {
                            yield return _posObj;
                        }
                    }
                }
                yield break;
            }
        }
        #endregion

        public IOpenable OpenableTarget
        {
            get => OpenerCloser.Openable;
            set => OpenerCloser.Openable = value;
        }

        public OpenerCloser OpenerCloser
        {
            get => GetValue(OpenerCloserProperty) as OpenerCloser;
            set => SetValue(OpenerCloserProperty, value);
        }

        public double OpenState
        {
            get => OpenableTarget?.OpenState.Value ?? 0d;
            set => OpenableTarget.OpenState = OpenableTarget.GetOpenStatus(null, null, value);
        }

        // Using a DependencyProperty as the backing store for RelockingOpener.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenerCloserProperty =
            DependencyProperty.Register(nameof(OpenerCloser), typeof(OpenerCloser), typeof(OpenerCloserEditor),
            new UIPropertyMetadata(null));

        private void cboOpenable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cboOpenable.SelectedItem != null) && (OpenerCloser?.Openable != ((PositionedObject)(cboOpenable.SelectedItem)).CoreObject))
            {
                OpenerCloser.Openable = ((PositionedObject)(cboOpenable.SelectedItem)).CoreObject as IOpenable;
            }
        }
    }
}
