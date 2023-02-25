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
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Fidelity;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for DescriptionEditor.xaml
    /// </summary>
    public partial class DescriptionEditor : Window
    {
        public DescriptionEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(DescriptionEditor_DataContextChanged);
            foreach (var _dev in Campaign.SystemCampaign.Devotions)
                cbDevotion.Items.Add(_dev.Key);
        }

        void DescriptionEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Creature _critter)
            {
                foreach (Alignment _align in cbAlignment.Items)
                {
                    if (_align.Equals(_critter.Alignment))
                    {
                        cbAlignment.SelectedItem = _align;
                        break;
                    }
                }
                foreach (var _devotion in cbDevotion.Items)
                {
                    if (_devotion.Equals(_critter.Devotion.Name))
                    {
                        cbDevotion.SelectedItem = _devotion;
                        break;
                    }
                }
            }
        }

        private void txtDouble_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!double.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            _txt.ToolTip = null;
            _txt.Tag = null;
        }

        private void cbAlignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _newAlign = cbAlignment.SelectedItem as Alignment;
            if ((DataContext is Creature _critter) && (_newAlign != null))
            {
                var _current = _critter.Adjuncts.OfType<AlignedCreature>().FirstOrDefault();
                if ((_current == null) || ((_current.Alignment.Ethicality != _newAlign.Ethicality)
                    || (_current.Alignment.Orderliness != _newAlign.Orderliness)))
                {
                    var _set = new AlignedCreature(_newAlign);
                    _critter.AddAdjunct(_set);
                }
            }
        }

        private void cbDevotion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _devotion = cbDevotion.SelectedItem as string;
            if ((DataContext is Creature _critter) && (_devotion != null))
            {
                if (!_critter.Devotion.Name.Equals(_devotion))
                    _critter.Devotion = new Devotion(_devotion);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
