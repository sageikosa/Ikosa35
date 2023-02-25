using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for Advancement.xaml
    /// </summary>
    public partial class Advancement : UserControl
    {
        public Advancement()
        {
            try { InitializeComponent(); } catch { }
        }

        private ActorModel Actor
            => DataContext as ActorModel;

        private AdvancementVM AdvancementVM
            => Actor?.AdvancementVM;
    }
}
