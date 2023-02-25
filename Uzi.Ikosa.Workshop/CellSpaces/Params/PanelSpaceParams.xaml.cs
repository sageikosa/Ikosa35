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
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PanelSpaceParams.xaml
    /// </summary>
    public partial class PanelSpaceParams : UserControl, IParamControl
    {
        public PanelSpaceParams()
        {
            _Params = new PanelParamsVM(new PanelParams());
            InitializeComponent();
            this.DataContext = _Params;
            Init();
        }

        public PanelSpaceParams(uint paramData)
        {
            _Params = new PanelParamsVM(new PanelParams(paramData));
            InitializeComponent();
            this.DataContext = _Params;
            Init();
        }

        private void Init()
        {
            var _panelTypes = new[] 
            { 
                PanelType.NoPanel, 
                PanelType.Panel1, 
                PanelType.Panel2, 
                PanelType.Panel3, 
                PanelType.Corner, 
                PanelType.LFrame, 
                PanelType.MaskedCorner, 
                PanelType.MaskedLFrame 
            };
            this.Resources.Add(@"listPanelTypes", _panelTypes);
        }

        private PanelParamsVM _Params;

        public uint ParamData { get { return _Params.Params.Value; } }

        // TODO: interior binding!!!
    }
}