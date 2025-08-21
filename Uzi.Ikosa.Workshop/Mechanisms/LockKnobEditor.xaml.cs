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
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LockKnobEditor.xaml
    /// </summary>
    public partial class LockKnobEditor : UserControl
    {
        #region construction
        public LockKnobEditor(LockKnob lockKnob) // Openable?
        {
            InitializeComponent();
            _LockKnob = lockKnob;
            DataContext = this;
            tipObject.DataContext = lockKnob;

            cboLock.Items.Clear();
            foreach (var _lock in AvailableLocks)
            {
                if (!cboLock.Items.Contains(_lock))
                {
                    cboLock.Items.Add(_lock);
                    if (_lock == LockKnob.LockGroup)
                    {
                        cboLock.SelectedItem = _lock;
                    }
                }
            }
        }
        #endregion

        private LockKnob _LockKnob;

        public LockKnob LockKnob => _LockKnob;
        public IOpenable Openable => _LockKnob.Openable;

        #region public IEnumerable<LockGroup> AvailableLocks { get; }
        public IEnumerable<LockGroup> AvailableLocks
        {
            get
            {
                if (LockKnob != null)
                {
                    if (Openable != null)
                    {
                        yield return new LockGroup(null, false, true);
                    }

                    if (Openable is ItemBase)
                    {
                        foreach (var _iLock in from _l in LockKnob.GetLocated().Locator.ICoreAs<ILockMechanism>()
                                               where (_l.LockGroup.Target.Openable == Openable)
                                               select _l)
                        {
                            yield return _iLock.LockGroup;
                        }
                        foreach (var _iLock in from _o in LockKnob.GetLocated().Locator.ICoreAs<ICoreObject>()
                                               from _l in _o.AllConnected(null).OfType<ILockMechanism>()
                                               where (_l.LockGroup.Target.Openable == Openable)
                                               select _l)
                        {
                            yield return _iLock.LockGroup;
                        }
                    }
                    else
                    {
                        foreach (var _iLock in from _l in LockKnob.GetLocated().Locator.Map.MapContext.AllOf<ILockMechanism>()
                                               where !(_l.LockGroup.Target.Openable is ItemBase)
                                               select _l)
                        {
                            yield return _iLock.LockGroup;
                        }
                        foreach (var _iLock in from _o in LockKnob.GetLocated().Locator.Map.MapContext.AllOf<ICoreObject>()
                                               from _l in _o.AllConnected(null).OfType<ILockMechanism>()
                                               where !(_l.LockGroup.Target.Openable is ItemBase)
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

        private void cboLock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LockKnob.LockGroup != cboLock.SelectedItem)
            {
                var _lock = cboLock.SelectedItem as LockGroup;
                if (_lock.Target == null)
                {
                    Openable.AddAdjunct(new LockTarget(_lock));
                }
                LockKnob.LockGroup = _lock;
            }
        }

        public double OpenState
        {
            get => _LockKnob.OpenState.Value;
            set => _LockKnob.OpenState = new OpenStatus(value);
        }
    }
}
