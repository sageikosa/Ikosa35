using System;
using System.Collections.Generic;
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
using System.ComponentModel;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>

    public partial class Start : System.Windows.Controls.Page
    {
        public Start()
        {
            InitializeComponent();
            _Creator = new CreateCharacter();
            frmCreate.Content = _Creator;
        }

        private CreateCharacter _Creator;
        public CreateCharacter Creator { get { return _Creator; } }
    }
}