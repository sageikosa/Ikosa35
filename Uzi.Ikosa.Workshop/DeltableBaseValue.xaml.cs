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
using Uzi.Core;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for DeltableBaseValue.xaml
    /// </summary>
    public partial class DeltableBaseValue : Window
    {
        public DeltableBaseValue(Deltable deltable)
        {
            InitializeComponent();
            txtBaseValue.Text = deltable.BaseValue.ToString();
            _Deltable = deltable;
        }

        private Deltable _Deltable;

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            int _out = 0;
            if (int.TryParse(txtBaseValue.Text, out _out))
            {
                _Deltable.BaseValue = _out;
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
