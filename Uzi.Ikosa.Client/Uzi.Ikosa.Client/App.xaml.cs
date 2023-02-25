using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                TreeViewItem.PreviewMouseDownEvent,
                new RoutedEventHandler(TreeViewItem_PreviewMouseDownEvent));
            base.OnStartup(e);
        }
        private void TreeViewItem_PreviewMouseDownEvent(object sender, RoutedEventArgs e)
        {
            (sender as TreeViewItem).IsSelected = true;
        }
    }
}
