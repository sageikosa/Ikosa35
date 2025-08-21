using System;
using System.Linq;
using System.Windows;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using System.Windows.Input;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Adjuncts;
using System.Windows.Controls;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ObjectEditorWindow.xaml
    /// </summary>
    public partial class ObjectEditorWindow : Window, IHostTabControl
    {
        #region CoreObject constructor
        public ObjectEditorWindow(PresentableContext presentContext)
        {
            InitializeComponent();
            void _addLocator<Thing>(PresentableThingVM<Thing> myContext)
                where Thing : class, ICoreObject
            {
                var _loc = myContext?.Thing?.GetLocated()?.Locator;
                if (_loc != null)
                {
                    var _editor = new LocatorEditor(_loc, this);
                    tabHost.Items.Add(_editor);
                }
            }
            if (presentContext is PresentableCreatureVM _critter)
            {
                tabHost.Items.Add(new CreatureEditor(_critter, this) { IsSelected = true });
                _addLocator(_critter);
            }
            else if (presentContext is PresentableContainerObjectVM _cont)
            {
                tabHost.Items.Add(new ContainerEditorTab(_cont, this) { IsSelected = true });
                _addLocator(_cont);
            }
            else if (presentContext is PresentableContainerItemVM _contItem)
            {
                tabHost.Items.Add(new ContainerItemBaseTab(_contItem, this) { IsSelected = true });
            }
            else if (presentContext is PresentableSlottedContainerItemVM _slotCont)
            {
                tabHost.Items.Add(new SlottedContainerItemBaseTab(_slotCont, this) { IsSelected = true });
            }
            else if (presentContext is KeyRingVM _keyRing)
            {
                tabHost.Items.Add(new KeyRingTab(_keyRing, this) { IsSelected = true });
            }
            else if (presentContext is PresentablePortalVM _port)
            {
                tabHost.Items.Add(new PortalEditor(_port, this) { IsSelected = true });
                _addLocator(_port);
            }
            else if (presentContext is PresentableCloseableContainerVM _closeCont)
            {
                tabHost.Items.Add(new CloseableContainerTab(_closeCont, this) { IsSelected = true });
                _addLocator(_closeCont);
            }
            else if (presentContext is PresentableMechanismMountVM _mount)
            {
                tabHost.Items.Add(new MechanismMountEditor(_mount, this) { IsSelected = true });
                _addLocator(_mount);
            }
            else if (presentContext is PresentableThingVM<MeleeTriggerable> _mTrig)
            {
                tabHost.Items.Add(new MeleeTriggerableAttackEditor(this) { MeleeTriggerable = _mTrig.Thing, IsSelected = true });
            }
            else if (presentContext is PresentableThingVM<RangedTriggerable> _rTrig)
            {
                tabHost.Items.Add(new RangedTriggerableAttackEditor(this) { RangedTriggerable = _rTrig.Thing, IsSelected = true });
            }
            //else if (presentContext is Keyhole _keyhole)
            //{
            //    // TODO:
            //}
            //else if (presentContext is LockActivationMechanism _lockAct)
            //{
            //    // TODO:
            //}
            //else if (presentContext is LockKnob _lockKnob)
            //{
            //    // TODO:
            //}
            //else if (presentContext is OpenableTriggerMechanism _openTrigger)
            //{
            //    // TODO:
            //}
            //else if (presentContext is OpenCloseTriggerable _openCloseTriggerable)
            //{
            //    // TODO:
            //}
            //else if (presentContext is OpenerCloser _openerCloser)
            //{
            //    // TODO:
            //}
            //else if (presentContext is RelockingOpener _relock)
            //{
            //    // TODO:
            //}
            //else if (presentContext is SurfaceTriggerMechanism _surfaceTrigger)
            //{
            //    // TODO:
            //}
            //else if (presentContext is ProximityTriggerMechanism _proximityTrigger)
            //{
            //    // TODO:
            //}
            //else if (presentContext is SwitchActivationMechanism _switchAct)
            //{
            //    // TODO:
            //}
            //else if (presentContext is ThrowBolt _bolt)
            //{
            //    // TODO:
            //}
            // TODO: ammo container
        }
        #endregion

        #region Locator constructor
        public ObjectEditorWindow(Locator locator)
        {
            InitializeComponent();
            var _editor = new LocatorEditor(locator, this);
            tabHost.Items.Add(_editor);
            _editor.IsSelected = true;
        }
        #endregion

        #region Room constructor
        public ObjectEditorWindow(Room room)
        {
            InitializeComponent();
            var _editor = new RoomEditor(room, this);
            SizeToContent = SizeToContent.WidthAndHeight;
            MaxHeight = 400;
            ResizeMode = ResizeMode.NoResize;
            tabHost.Items.Add(_editor);
            _editor.IsSelected = true;
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            foreach (var _item in tabHost.Items.OfType<IHostedTabItem>())
            {
                _item.CloseTabItem();
            }

            Close();
        }

        #region IHostTabControl Members
        public void RemoveTabItem(IHostedTabItem item)
        {
            btnClose_Click(this, new RoutedEventArgs());
        }

        public Window GetWindow() => Window.GetWindow(this);
        #endregion

        private void cbOpenItem_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is PresentableContainerItemVM)
                || (e.Parameter is PresentableSlottedContainerItemVM)
                || (e.Parameter is KeyRingVM)
                || typeof(PresentableAmmunitionBundle<,,>).IsAssignableFrom(e.Parameter.GetType());
            e.Handled = true;
        }

        private void cbOpenItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _edit = new ObjectEditorWindow(e.Parameter as PresentableContext)
            {
                Title = @"Edit Object",
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _edit.ShowDialog();
        }

        public void FindOrOpen<TabType>(Func<TabType, bool> match, Func<TabType> generate) where TabType : TabItem
        {
        }
    }
}
