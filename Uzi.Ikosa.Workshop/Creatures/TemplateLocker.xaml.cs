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
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for TemplateLocker.xaml
    /// </summary>
    public partial class TemplateLocker : UserControl
    {
        public static RoutedCommand RemoveLevel = new RoutedCommand();
        public static RoutedCommand LockLevel = new RoutedCommand();
        public static RoutedCommand UnLockLevel = new RoutedCommand();

        public ItemsControl AdvancementLogItemsControl
        {
            get { return (ItemsControl)GetValue(AdvancementLogItemsControlProperty); }
            set { SetValue(AdvancementLogItemsControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdvancementLogItemsControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdvancementLogItemsControlProperty =
            DependencyProperty.Register(nameof(AdvancementLogItemsControl), typeof(ItemsControl),
            typeof(TemplateLocker), new UIPropertyMetadata(null));

        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RefreshCommandProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RefreshCommandProperty =
            DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand),
            typeof(TemplateLocker), new UIPropertyMetadata(null));

        public TemplateLocker()
        {
            InitializeComponent();
        }

        private void RefreshControls()
        {
            AdvancementLogItemsControl?.Items.Refresh();
            RefreshCommand?.Execute(null);
        }

        #region private void btnLockerClick(object sender, MouseButtonEventArgs e)
        private void btnLockerClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // lock/unlock clicked, attempt to make it so
                var _advItem = (sender as Grid)?.DataContext as AdvancementLogItem;
                if (_advItem.IsLocked)
                {
                    // unlock to before this
                    _advItem.Creature.AdvancementLog.UnlockBackTo(_advItem.StartLevel - 1);
                }
                else
                {
                    // lock up to this
                    _advItem.Creature.AdvancementLog.LockUpTo(_advItem.EndLevel);
                }
                RefreshControls();
            }
        }
        #endregion

        #region unlock level context menu handling
        private void cmdbndUnLockLevel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is AdvancementLogItem _log)
            {
                e.CanExecute = _log.IsLocked;
            }
            e.Handled = true;
        }

        private void cmdbndUnLockLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is AdvancementLogItem _log)
            {
                // unlock to before this
                _log.Creature.AdvancementLog.UnlockBackTo(_log.StartLevel - 1);
                RefreshControls();
            }
            e.Handled = true;
        }
        #endregion

        #region lock level context menu handling
        private void cmdbndLockLevel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is AdvancementLogItem _log)
            {
                e.CanExecute = _log.IsLockable && !_log.IsLocked;
            }
            e.Handled = true;
        }

        private void cmdbndLockLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is AdvancementLogItem _log)
            {
                // unlock to before this
                _log.Creature.AdvancementLog.LockUpTo(_log.EndLevel);
                RefreshControls();
            }
            e.Handled = true;

        }
        #endregion

        #region private void cmdbndRemoveLevel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndRemoveLevel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndRemoveLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndRemoveLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is AdvancementLogItem _log)
            {
                _log.Creature.IsInSystemEditMode = true;
                _log.Creature.AdvancementLog.RemoveTo(_log.StartLevel - 1);
                _log.Creature.IsInSystemEditMode = false;
                RefreshControls();
            }
            e.Handled = true;
        }
        #endregion

    }
}
