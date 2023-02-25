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
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using System.Diagnostics;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for AdditionalCharInfo.xaml
    /// </summary>
    public partial class AdditionalCharInfo : UserControl
    {
        public AdditionalCharInfo()
        {
            try { InitializeComponent(); } catch { }
        }

        public ActorModel ActorModel { get { return DataContext as ActorModel; } }

        public IEnumerable<ActionInfo> Actions
        {
            get { return (IEnumerable<ActionInfo>)GetValue(ActionsProperty); }
            set { SetValue(ActionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Actions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof(IEnumerable<ActionInfo>), typeof(AdditionalCharInfo),
            new UIPropertyMetadata(null, new PropertyChangedCallback(ActionListChanged)));

        private static void ActionListChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is AdditionalCharInfo _addCharInfo)
            {
                var _actions = args.NewValue as IEnumerable<ActionInfo>;
                _addCharInfo.slctFeats.Actions = _actions;
                _addCharInfo.itemsFeats.Items.Refresh();
                //if (_actions != null)
                //    _addCharInfo.itemsChoices.ItemsSource = _actions
                //        .Where(_a => (_a.TimeType == TimeType.Choice) && (_a.AimingModes.FirstOrDefault() is OptionAimInfo))
                //        .ToList();
                //else
                //    _addCharInfo.itemsChoices.ItemsSource = null;
            }
        }

        private void Choice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox _combo)
            {
                // always starts @ zero
                if (_combo.SelectedIndex > 0)
                {
                    var _opt = _combo.SelectedItem as OptionAimOption;
                    var _action = _combo.Tag as ActionInfo;
                    var _optAim = _action.FirstOptionAimInfo;
                    if (_action != null)
                    {
                        // build choice activity
                        var _activity = new ActivityInfo
                        {
                            ActionID = _action.ID,
                            ActionKey = _action.Key,
                            ActorID = ActorModel.CreatureLoginInfo.ID,
                            Targets = new[]
                            {
                                new OptionTargetInfo
                                {
                                    Key = _optAim.Key,
                                    OptionKey = _opt.Key
                                }
                            }
                        };

                        try
                        {
                            ActorModel.Proxies.IkosaProxy.Service.DoAction(_activity);
                            ActorModel.UpdateActions();
                            //RefreshActions();
                        }
                        catch (Exception _ex)
                        {
                            Debug.WriteLine(_ex);
                            MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            e.Handled = true;
        }
    }
}
