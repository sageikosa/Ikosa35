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
using Uzi.Ikosa.Objects;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using System.ComponentModel;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RelockingOpenerEditor.xaml
    /// </summary>
    public partial class RelockingOpenerEditor : UserControl
    {
        public RelockingOpenerEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == RelockingOpenerProperty)
            {
                cboOpenable.Items.Clear();
                cboOpenable.SelectedItem = null;
                foreach (var _open in AvailableOpenables)
                {
                    cboOpenable.Items.Add(_open);
                    if (_open.CoreObject == RelockingOpener.Openable)
                        cboOpenable.SelectedItem = _open;
                }
            }
            base.OnPropertyChanged(e);
        }

        #region public IEnumerable<LockGroup> AvailableLocks { get; }
        public IEnumerable<LockGroup> AvailableLocks
        {
            get
            {
                if (RelockingOpener != null)
                {
                    var _openable = RelockingOpener.Openable;
                    if (_openable != null)
                    {
                        yield return new LockGroup(null, false, true);

                        // find all lock mechanisms that are bound to the openable
                        foreach (var _iLock in from _l in RelockingOpener.GetLocated().Locator.Map.MapContext.AllOf<ILockMechanism>()
                                               where _l.LockGroup.Target.Openable == _openable
                                               select _l)
                        {
                            yield return _iLock.LockGroup;
                        }
                        foreach (var _iLock in from _o in RelockingOpener.GetLocated().Locator.Map.MapContext.AllOf<ICoreObject>()
                                               from _l in _o.AllConnected(null).OfType<ILockMechanism>()
                                               where _l.LockGroup.Target.Openable == _openable
                                               select _l)
                        {
                            yield return _iLock.LockGroup;
                        }
                    }
                }
                yield break;
            }
        }
        #endregion

        #region public IEnumerable<PositionedObject> AvailableOpenables { get; }
        public IEnumerable<PositionedObject> AvailableOpenables
        {
            get
            {
                if (RelockingOpener != null)
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
                        var _loc = RelockingOpener.GetLocated().Locator;
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
                        foreach (var _posObj in (from _l in (RelockingOpener.Setting as LocalMap).MapContext.AllTokensOf<Locator>()
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
            get => RelockingOpener.Openable;
            set => RelockingOpener.Openable = value;
        }

        public RelockingOpener RelockingOpener
        {
            get => (RelockingOpener)GetValue(RelockingOpenerProperty);
            set => SetValue(RelockingOpenerProperty, value);
        }

        public double OpenState
        {
            get => OpenableTarget.OpenState.Value;
            set => OpenableTarget.OpenState = new OpenStatus(value);
        }

        // Using a DependencyProperty as the backing store for RelockingOpener.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RelockingOpenerProperty =
            DependencyProperty.Register(nameof(RelockingOpener), typeof(RelockingOpener), typeof(RelockingOpenerEditor),
            new UIPropertyMetadata(null));

        private void cboOpenable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cboOpenable.SelectedItem != null) && (RelockingOpener.Openable != ((PositionedObject)(cboOpenable.SelectedItem)).CoreObject))
                RelockingOpener.Openable = ((PositionedObject)(cboOpenable.SelectedItem)).CoreObject as IOpenable;
            cboLock.Items.Clear();
            foreach (var _lock in AvailableLocks)
            {
                if (!cboLock.Items.Contains(_lock))
                {
                    cboLock.Items.Add(_lock);
                    if (_lock == RelockingOpener.LockGroup)
                        cboLock.SelectedItem = _lock;
                }
            }
        }

        private void cboLock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RelockingOpener.LockGroup != cboLock.SelectedItem)
            {
                var _lock = cboLock.SelectedItem as LockGroup;
                if (_lock.Target == null)
                {
                    OpenableTarget.AddAdjunct(new LockTarget(_lock));
                }
                RelockingOpener.LockGroup = _lock;
            }
        }
    }
}
