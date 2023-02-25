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
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ObjectTip.xaml
    /// </summary>
    public partial class ObjectTip : UserControl
    {
        public ObjectTip()
        {
            InitializeComponent();
        }

        public ObjectBase ObjectBase => DataContext as ObjectBase;

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var _editor = new ObjectEditor(ObjectBase)
            {
                Owner = Window.GetWindow(this)
            };
            _editor.ShowDialog();
        }


    }
}
