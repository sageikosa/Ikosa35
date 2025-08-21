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
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for KeyholeEditor.xaml
    /// </summary>
    public partial class KeyholeEditor : UserControl
    {
        #region construction
        public KeyholeEditor(Keyhole keyhole) // Openable?
        {
            InitializeComponent();
            _Keyhole = keyhole;
            DataContext = keyhole;

            cboLock.Items.Clear();
            foreach (var _lock in AvailableLocks)
            {
                if (!cboLock.Items.Contains(_lock))
                {
                    cboLock.Items.Add(_lock);
                    if (_lock == Keyhole.LockGroup)
                    {
                        cboLock.SelectedItem = _lock;
                    }
                }
            }
        }
        #endregion

        private Keyhole _Keyhole;

        public Keyhole Keyhole => _Keyhole;
        public IOpenable Openable => _Keyhole.Openable;

        #region public IEnumerable<Lock> AvailableLocks { get; }
        public IEnumerable<LockGroup> AvailableLocks
        {
            get
            {
                if (Keyhole != null)
                {
                    if (Openable != null)
                    {
                        yield return new LockGroup(null, false, true);
                    }

                    var _khLoc = Keyhole.GetLocated();
                    if (_khLoc != null)
                    {
                        if (Openable is ItemBase)
                        {
                            foreach (var _iLock in from _l in _khLoc.Locator.ICoreAs<ILockMechanism>()
                                                   where (_l.LockGroup.Target.Openable == Openable)
                                                   select _l)
                            {
                                yield return _iLock.LockGroup;
                            }
                            foreach (var _iLock in from _o in _khLoc.Locator.ICoreAs<ICoreObject>()
                                                   from _l in _o.AllConnected(null).OfType<ILockMechanism>()
                                                   where (_l.LockGroup.Target.Openable == Openable)
                                                   select _l)
                            {
                                yield return _iLock.LockGroup;
                            }
                        }
                        else
                        {
                            foreach (var _iLock in from _l in _khLoc.Locator.Map.MapContext.AllOf<ILockMechanism>()
                                                   where !(_l.LockGroup.Target.Openable is ItemBase)
                                                   select _l)
                            {
                                yield return _iLock.LockGroup;
                            }
                            foreach (var _iLock in from _o in _khLoc.Locator.Map.MapContext.AllOf<ICoreObject>()
                                                   from _l in _o.AllConnected(null).OfType<ILockMechanism>()
                                                   where !(_l.LockGroup.Target.Openable is ItemBase)
                                                   select _l)
                            {
                                yield return _iLock.LockGroup;
                            }
                        }
                    }
                }
                yield break;
            }
        }
        #endregion

        private void cboLock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Keyhole.LockGroup != cboLock.SelectedItem)
            {
                var _lock = cboLock.SelectedItem as LockGroup;
                if (_lock.Target == null)
                {
                    Openable.AddAdjunct(new LockTarget(_lock));
                }
                Keyhole.LockGroup = _lock;
            }
        }

        private void btnKeySelect_Click(object sender, RoutedEventArgs e)
        {
            var _selector = new KeySelector(Keyhole)
            {
                Owner = Window.GetWindow(this)
            };
            _selector.ShowDialog();
        }

    }
}
