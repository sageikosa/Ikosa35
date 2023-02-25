using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Ikosa.Client.UI;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for ActorView.xaml
    /// </summary>
    public partial class ActorView : Window
    {
        public static RoutedCommand ShowToolWindow = new RoutedCommand();

        private ActorView(ActorModel actor)
        {
            InitializeComponent();

            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            dockLocale.Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            dockLocale.Resources.Add(@"slctItemListTemplate", ItemListTemplateSelector.GetDefault(dockLocale.Resources));
            dockLocale.Resources.Add(@"menuItemListTemplate", ItemListTemplateSelector.GetMenuDefault(dockLocale.Resources));

            _Actor = actor;
            _Actor.ObservableActor = new ObservableActor(_Actor, Dispatcher, dockLocale.Resources,
                () => Close(),
                () =>
                {
                    if (WindowState == WindowState.Minimized)
                        WindowState = WindowState.Normal;
                    Activate();
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                        new Action(() =>
                        {
                            var _setTo = Keyboard.Focus(viewMainSensor);
                            Debug.WriteLine(_setTo.GetType().Name);
                            CommandManager.InvalidateRequerySuggested();
                        }));
                });

            DataContext = _Actor;
            Title = $@"Character: {_Actor.CreatureLoginInfo.Name}";
            //if (_Actor.CreatureLoginInfo.Portrait != null)
            //    Icon = _Actor.CreatureLoginInfo.Portrait.GetSizedImage(32, 32);

            Closed += ActorView_Closed;
            SetKeyBindings();
        }

        #region KeyBindings in code since XAML can't seem to find the elements (XCeed NameScope issues?)
        private void SetOneKeyBinding(IInputElement target, ICommand command, Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            if (modifiers == ModifierKeys.None)
                dockLocale.InputBindings.Add(new KeyBinding { Command = command, Key = key, CommandTarget = target });
            else if (modifiers == ModifierKeys.Shift)
                dockLocale.InputBindings.Add(new KeyBinding { Command = command, Key = key, Modifiers = modifiers, CommandTarget = target });
            else
                dockLocale.InputBindings.Add(new KeyBinding(command, key, modifiers) { CommandTarget = target });
        }

        private void SetKeyBindings()
        {
            SetOneKeyBinding(mnuLocalAction.MoveActionPanel, MoveActionPanel.PickMove, Key.OemQuestion);
            SetOneKeyBinding(mnuLocalAction.MoveActionPanel, MoveActionPanel.PickStart, Key.OemQuestion, ModifierKeys.Shift);

            dockLocale.InputBindings.Add
                (new KeyBinding()
                {
                    Command = LocalActionPanel.GraspCommand,
                    Key = Key.G,
                    Modifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    CommandTarget = mnuLocalAction
                });
            dockLocale.InputBindings.Add
                (new KeyBinding()
                {
                    Command = LocalActionPanel.SearchCommand,
                    Key = Key.OemQuestion,
                    Modifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    CommandTarget = mnuLocalAction
                });
            SetOneKeyBinding(mnuLocalAction, LocalActionPanel.GraspCommand, Key.G, ModifierKeys.Control);
        }
        #endregion

        private void ActorView_Closed(object sender, EventArgs e)
        {
            try
            {
                Actor.ObservableActor = null;
                Closed -= ActorView_Closed;
                Dispatcher.InvokeShutdown();
            }
            catch
            {
            }
        }

        public static void StartActorView(ActorModel actor)
        {
            var _start = new Thread(() => DoStartActorView(actor));
            _start.SetApartmentState(ApartmentState.STA);
            _start.Start();
        }

        private static void DoStartActorView(ActorModel actor)
        {
            var _view = new ActorView(actor);
            _view.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        #region data
        private ActorModel _Actor;
        #endregion

        #region tool windows
        private void cdShowToolWindow_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cdShowToolWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is TabItem _tabItem)
            {
                tabControl.SelectedItem = _tabItem;
            }
            e.Handled = true;
        }
        #endregion

        public ActorModel Actor => _Actor;

        private void dockLocale_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(viewMainSensor);
            CommandManager.InvalidateRequerySuggested();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem == tcView)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                    new Action(() =>
                    {
                        Keyboard.Focus(viewMainSensor);
                        CommandManager.InvalidateRequerySuggested();
                    }));
            }
        }
    }
}
