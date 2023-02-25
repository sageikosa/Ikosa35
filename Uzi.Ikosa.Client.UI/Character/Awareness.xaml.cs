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
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Ikosa.Proxy;
using System.Windows.Controls.Primitives;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for Awareness.xaml
    /// </summary>
    public partial class Awareness : UserControl
    {
        public Awareness()
        {
            try { InitializeComponent(); } catch { }
            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            Resources.Add(@"slctItemListTemplate", ItemListTemplateSelector.GetDefault(Resources));
        }

        private void lstItems_Checked(object sender, RoutedEventArgs e)
        {
            ObservableActor.ResyncAwarenessQueue();
            e.Handled = true;
        }

        public ObservableActor ObservableActor => DataContext as ObservableActor;

        private static void RefreshList(DependencyObject dependency, DependencyPropertyChangedEventArgs args)
        {
            var _awareness = dependency as Awareness;
            if (_awareness != null)
            {
                _awareness.lstItems.Items.Refresh();
            }
        }

        #region public IEnumerable<AwarenessInfo> AwarenessInfos { get; }
        /// <summary>gets or sets the awarenesses</summary>
        public IEnumerable<AwarenessInfo> AwarenessInfos
        {
            get { return (IEnumerable<AwarenessInfo>)GetValue(AwarenessInfosProperty); }
            set { SetValue(AwarenessInfosProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AwarenessInfos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AwarenessInfosProperty =
            DependencyProperty.Register(@"AwarenessInfos", typeof(IEnumerable<AwarenessInfo>), typeof(Awareness), new PropertyMetadata(null));

        #endregion

        #region public IEnumerable<AwarenessInfo> SelectedAwarenesses { get; set; }

        public IEnumerable<AwarenessInfo> SelectedAwarenesses
        {
            get { return (IEnumerable<AwarenessInfo>)GetValue(SelectedAwarenessesProperty); }
            set { SetValue(SelectedAwarenessesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedAwarenesses.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAwarenessesProperty =
            DependencyProperty.Register("SelectedAwarenesses", typeof(IEnumerable<AwarenessInfo>),
                typeof(Awareness), new PropertyMetadata(null, new PropertyChangedCallback(RefreshList)));
        #endregion

        private void Selection_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _present = e.Parameter as PresentationInfo;
            if (_present != null)
            {
                e.CanExecute = AwarenessInfos.Any(_a => _a.ID == _present.PresentingIDs.FirstOrDefault());
            }
            e.Handled = true;
        }

        #region private void CheckBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        private void CheckBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // exclusively select item from awareness control
            var _checkBox = sender as CheckBox;
            if (_checkBox != null)
            {
                var _aware = _checkBox.DataContext as AwarenessInfo;
                if (_aware != null)
                {
                    ObservableActor.SelectAwareness(_aware.ID);
                    lstItems.Items.Refresh();
                }
            }
            e.Handled = true;
        }
        #endregion

        private void Awareness_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}