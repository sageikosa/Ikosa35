using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SurfaceTriggerMechanismEditor.xaml
    /// </summary>
    public partial class SurfaceTriggerMechanismEditor : UserControl
    {
        public SurfaceTriggerMechanismEditor()
        {
            InitializeComponent();

            // control is the view model
            DataContext = this;
        }

        public SurfaceTriggerMechanism SurfaceTriggerMechanism
        {
            get { return (SurfaceTriggerMechanism)GetValue(SurfaceTriggerMechanismProperty); }
            set { SetValue(SurfaceTriggerMechanismProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SurfaceTriggerMechanismProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SurfaceTriggerMechanismProperty =
            DependencyProperty.Register(
                nameof(SurfaceTriggerMechanism),
                typeof(SurfaceTriggerMechanism),
                typeof(SurfaceTriggerMechanismEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnSurfaceTriggerMechanismChanged)));

        private static void OnSurfaceTriggerMechanismChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is SurfaceTriggerMechanismEditor _surfTriggerEdit)
            {
                // ???
            }
        }

        public bool SurfaceZLow
        {
            get => SurfaceTriggerMechanism?.Surfaces.Contains(AnchorFace.ZLow) ?? false;
            set
            {
                if (SurfaceTriggerMechanism != null)
                {
                    SurfaceTriggerMechanism.Surfaces = value
                        ? SurfaceTriggerMechanism.Surfaces.Add(AnchorFace.ZLow)
                        : SurfaceTriggerMechanism.Surfaces.Remove(AnchorFace.ZLow);
                }
            }
        }

        public bool SurfaceZHigh
        {
            get => SurfaceTriggerMechanism?.Surfaces.Contains(AnchorFace.ZHigh) ?? false;
            set
            {
                if (SurfaceTriggerMechanism != null)
                {
                    SurfaceTriggerMechanism.Surfaces = value
                        ? SurfaceTriggerMechanism.Surfaces.Add(AnchorFace.ZHigh)
                        : SurfaceTriggerMechanism.Surfaces.Remove(AnchorFace.ZHigh);
                }
            }
        }

        public bool SurfaceYLow
        {
            get => SurfaceTriggerMechanism?.Surfaces.Contains(AnchorFace.YLow) ?? false;
            set
            {
                if (SurfaceTriggerMechanism != null)
                {
                    SurfaceTriggerMechanism.Surfaces = value
                        ? SurfaceTriggerMechanism.Surfaces.Add(AnchorFace.YLow)
                        : SurfaceTriggerMechanism.Surfaces.Remove(AnchorFace.YLow);
                }
            }
        }

        public bool SurfaceYHigh
        {
            get => SurfaceTriggerMechanism?.Surfaces.Contains(AnchorFace.YHigh) ?? false;
            set
            {
                if (SurfaceTriggerMechanism != null)
                {
                    SurfaceTriggerMechanism.Surfaces = value
                        ? SurfaceTriggerMechanism.Surfaces.Add(AnchorFace.YHigh)
                        : SurfaceTriggerMechanism.Surfaces.Remove(AnchorFace.YHigh);
                }
            }
        }

        public bool SurfaceXLow
        {
            get => SurfaceTriggerMechanism?.Surfaces.Contains(AnchorFace.XLow) ?? false;
            set
            {
                if (SurfaceTriggerMechanism != null)
                {
                    SurfaceTriggerMechanism.Surfaces = value
                        ? SurfaceTriggerMechanism.Surfaces.Add(AnchorFace.XLow)
                        : SurfaceTriggerMechanism.Surfaces.Remove(AnchorFace.XLow);
                }
            }
        }

        public bool SurfaceXHigh
        {
            get => SurfaceTriggerMechanism?.Surfaces.Contains(AnchorFace.XHigh) ?? false;
            set
            {
                if (SurfaceTriggerMechanism != null)
                {
                    SurfaceTriggerMechanism.Surfaces = value
                        ? SurfaceTriggerMechanism.Surfaces.Add(AnchorFace.XHigh)
                        : SurfaceTriggerMechanism.Surfaces.Remove(AnchorFace.XHigh);
                }
            }
        }

        private void btnTargets_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new TriggerTargetsEditor { TriggerMechanism = SurfaceTriggerMechanism },
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                Title = SurfaceTriggerMechanism.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
        }
    }
}
