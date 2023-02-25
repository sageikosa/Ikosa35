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
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for MechanismManager.xaml</summary>
    public partial class MechanismManager : UserControl
    {
        public static readonly RoutedCommand NewOpener = new();
        public static readonly RoutedCommand NewRelockingOpener = new();
        public static readonly RoutedCommand NewLockKnob = new();
        public static readonly RoutedCommand NewKeyhole = new();
        public static readonly RoutedCommand NewBolt = new();
        public static readonly RoutedCommand NewHasp = new();
        public static readonly RoutedCommand NewOpenTrigger = new();
        public static readonly RoutedCommand NewOpenCloseTriggerable = new();

        public MechanismManager()
        {
            InitializeComponent();
        }

        public PresentableContext PresentableContext
            => DataContext as PresentableContext;

        public IOpenable Openable
            => (PresentableContext?.CoreObject as IOpenable) ?? Portal;

        public PortalBase Portal
            => (PresentableContext?.CoreObject as PortalledObjectBase)
            ?.Adjuncts.OfType<ObjectBound>().FirstOrDefault()
            ?.Anchorage as PortalBase;

        #region private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region New Opener
        private void cmdbndNewOpener_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _opener = new OpenerCloser(@"Opener/Closer", SteelMaterial.Static, 20);

            // bind the opener to the portalled object base
            _opener.BindToObject(e.Parameter as IAnchorage);
            _opener.Openable = Openable;
        }
        #endregion

        #region New Bolt
        private void cmdbndNewBolt_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _target = e.Parameter as IAnchorage;
            var _bolt = new ThrowBolt(@"Throw Bolt", SteelMaterial.Static, 20, Openable, _target, false);
        }
        #endregion

        #region New Hasp
        private void cmdbndNewHasp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // add hasp
            var _target = e.Parameter as IAnchorage;
            var _hasp = new Hasp(@"Hasp", SteelMaterial.Static, 20, Openable, _target);

            // add new padlock
            var _padlock = new PadLock(@"Padlock", SteelMaterial.Static, 20, 20, new Guid[] { });
            _padlock.BindToObject(_hasp);
        }
        #endregion

        #region New LockKnob
        private void cmdbndNewLockKnob_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _target = e.Parameter as IAnchorage;

            var _lockKnob = new LockKnob(@"Knob", SteelMaterial.Static, 20);
            _lockKnob.BindToObject(_target);
            _lockKnob.Openable = Openable;
        }
        #endregion

        #region New Keyhole
        private void cmdbndNewKeyhole_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _target = e.Parameter as IAnchorage;

            // define new key guid
            var _guid = Guid.NewGuid();
            Openable.GetLocated().Locator.Map.NamedKeyGuids.Add(_guid, @"New Key");

            var _keyHole = new Keyhole(@"Keyhole", SteelMaterial.Static, 20, 20, new Guid[] { _guid });
            _keyHole.BindToObject(_target);
            _keyHole.Openable = Openable;
        }
        #endregion

        #region New RelockingOpener
        private void cmdbndNewRelockingOpener_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _relock = new RelockingOpener(@"Relock", SteelMaterial.Static, 20);
            _relock.BindToObject(e.Parameter as IAnchorage);
            _relock.Openable = Openable;
        }
        #endregion

        #region New OpenTrigger
        private void cmdbndNewOpenTrigger_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _openTrigger = new OpenableTriggerMechanism(@"Open/Close Trigger", SteelMaterial.Static, 20,
                PostTriggerState.AutoReset, true, false, Openable);

            // bind to portalled object base
            _openTrigger.BindToObject(e.Parameter as IAnchorage);
        }
        #endregion

        #region New OpenCloseTriggerable
        private void cmdbndNewOpenCloseTriggerable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _openCloseTriggerable = new OpenCloseTriggerable(@"Trigger Response", SteelMaterial.Static, 20,
                 OpenCloseTriggerOperation.Open, Openable);

            // bind to portalled object base
            _openCloseTriggerable.BindToObject(e.Parameter as IAnchorage);
        }
        #endregion

        #region Delete Mechanism
        private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstMechanism != null) && (lstMechanism.SelectedItem != null);
            e.Handled = true;
        }

        private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (lstMechanism.SelectedItem is ObjectBase _obj)
            {
                _obj.DoDestruction();
                ctrlContent.Content = null;
            }
        }
        #endregion

        #region private void lstMechanism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void lstMechanism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMechanism.SelectedItem is RelockingOpener _relock)
            {
                ctrlContent.Content = new RelockingOpenerEditor { RelockingOpener = _relock };
            }
            else if (lstMechanism.SelectedItem is OpenerCloser _openClose)
            {
                ctrlContent.Content = new OpenerCloserEditor { OpenerCloser = _openClose };
            }
            else if (lstMechanism.SelectedItem is ThrowBolt _bolt)
            {
                ctrlContent.Content = new ThrowBoltEditor(_bolt);
            }
            else if (lstMechanism.SelectedItem is LockKnob _knob)
            {
                ctrlContent.Content = new LockKnobEditor(_knob);
            }
            else if (lstMechanism.SelectedItem is Keyhole _keyhole)
            {
                ctrlContent.Content = new KeyholeEditor(_keyhole);
            }
            else if (lstMechanism.SelectedItem is OpenableTriggerMechanism _openTrigger)
            {
                ctrlContent.Content = new OpenableTriggerMechanismEditor { OpenableTriggerMechanism = _openTrigger };
            }
            else if (lstMechanism.SelectedItem is OpenCloseTriggerable _openCloseTriggerable)
            {
                ctrlContent.Content = new OpenCloseTriggerableEditor { OpenCloseTriggerable = _openCloseTriggerable };
            }
            else
            {
                ctrlContent.Content = null;
            }
            // TODO: hasp (with padlock)...
        }
        #endregion

        private void cmdbndOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: dialog box full editor (instead of in-place editors)
        }
    }
}
