using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;

namespace CharacterModeler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            BasePartFactory.RegisterFactory();
            VisualizeBasePartFactory.RegisterFactory();
        }
    }
}
