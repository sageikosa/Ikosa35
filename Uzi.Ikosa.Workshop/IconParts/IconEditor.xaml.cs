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
using System.Windows.Shapes;
using Uzi.Core;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for IconEditor.xaml
    /// </summary>
    public partial class IconEditor : Window
    {
        public IconEditor(PresentableContext context)
        {
            InitializeComponent();
            DataContext = new IconCustomizationVM(context);
        }
    }
}
