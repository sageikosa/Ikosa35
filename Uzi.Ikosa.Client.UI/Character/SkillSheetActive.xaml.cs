using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for SkillSheetActive.xaml
    /// </summary>
    public partial class SkillSheetActive : UserControl
    {
        public SkillSheetActive()
        {
            try { InitializeComponent(); } catch { }
        }

        public ActorModel ActorModel => DataContext as ActorModel;
    }
}
