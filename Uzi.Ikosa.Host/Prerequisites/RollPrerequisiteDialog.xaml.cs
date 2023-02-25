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
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Host.Prerequisites
{
    /// <summary>
    /// Interaction logic for RollPrerequisiteDialog.xaml
    /// </summary>
    public partial class RollPrerequisiteDialog : Window
    {
        public RollPrerequisiteDialog(RollPrerequisite preReq)
        {
            InitializeComponent();
            DataContext = preReq;
            _RollVal = null;
        }

        public RollPrerequisite RollPrerequisite => DataContext as RollPrerequisite; 
        private int? _RollVal;

        #region private void txtRoll_TextChanged(object sender, RoutedEventArgs e)
        private void txtRoll_TextChanged(object sender, RoutedEventArgs e)
        {
            var _txt = sender as TextBox;

            var _roll = RollPrerequisite;
            if (_roll != null)
            {
                // validate value
                if (!int.TryParse(_txt.Text, out var _out))
                {
                    _txt.Tag = @"Invalid";
                    _RollVal = null;
                    return;
                }

                // set as prerequisite value
                _RollVal = _out;
                _txt.ToolTip = @"Manual";
                _txt.Tag = null;
            }
        }
        #endregion

        private void btnRoll_Click(object sender, RoutedEventArgs e)
        {
            txtValue.Text = RollPrerequisite.Roller.RollValue(Guid.Empty, RollPrerequisite.BindKey, RollPrerequisite.Name).ToString();
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = txtValue.Tag == null;
            e.Handled = true;
        }

        public int RollValue { get { return Convert.ToInt32(txtValue.Text); } }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
    }
}
