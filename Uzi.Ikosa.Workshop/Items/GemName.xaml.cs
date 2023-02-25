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
using System.Windows.Shapes;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for GemName.xaml
    /// </summary>
    public partial class GemName : Window
    {
        public GemName(GemMaterial material)
        {
            InitializeComponent();
            txtName.Text = material.DisplayName;
            txtTrueName.Text = material.Name;
        }

        public string UnknownName => txtName.Text;
        public string TrueName => txtTrueName.Text;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
