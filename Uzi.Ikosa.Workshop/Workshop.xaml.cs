using System;
using System.Windows;
using System.IO;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using Uzi.Core.Packaging;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Core.Contracts;
using Newtonsoft.Json;
using System.Diagnostics;
using Uzi.Ikosa.Workshop.ModuleManagement;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using Uzi.Ikosa.UI.MVVM.Package;
using Uzi.Ikosa.UI;
using System.ComponentModel;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for Workshop.xaml</summary>
    public partial class Workshop : Window, INotifyPropertyChanged, IRefreshPackages
    {
        public Workshop()
        {
            InitializeComponent();
            BasePartFactory.RegisterFactory();
            VisualizeBasePartFactory.RegisterFactory();
            CoreBasePartFactory.RegisterFactory();
            IkosaBasePartFactory.RegisterFactory();
            //miFileOpen.Click += new RoutedEventHandler(miFileOpen_Click);
            DataContext = this;

            var _serializer = new JsonSerializer();
            using var _text = File.OpenText(@"config.json");
            using var _json = new JsonTextReader(_text);
            _Config = _serializer.Deserialize<IkosaPackageManagerConfig>(_json);
            PackageManager.Manager.SetPaths(
                _Config.Packages.Select(_p => _p.Path).ToList(),
                _Config.Campaigns.Select(_c => _c.Path).ToList());

            _OpenPackage = new RelayCommand<PackageFileVM>(cmdOpenPackage);
            _NewCampaign = new RelayCommand<PackageSetVM>(cmdNewCampaign);
            _NewModule = new RelayCommand<PackageCampaignVM>(cmdNewModule);
            _NewResources = new RelayCommand<object>(cmdNewResources);
        }

        private readonly IkosaPackageManagerConfig _Config;

        private readonly RelayCommand<PackageFileVM> _OpenPackage;
        private readonly RelayCommand<PackageSetVM> _NewCampaign;
        private readonly RelayCommand<PackageCampaignVM> _NewModule;
        private readonly RelayCommand<object> _NewResources;

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand<PackageFileVM> DoOpenPackage => _OpenPackage;
        public RelayCommand<PackageSetVM> DoNewCampaign => _NewCampaign;
        public RelayCommand<PackageCampaignVM> DoNewModule => _NewModule;
        public RelayCommand<object> DoNewResources => _NewResources;

        public void RefreshPackages()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageSources)));

        public IList<object> PackageSources
            => _Config.Campaigns
            .Select(_c => new PackageSetVM
            {
                PackageSetPathEntry = _c,
                IsNodeExpanded = true,
                CommandHost = this
            })
            .Cast<object>()
            .Union(
                _Config.Packages
                .Select(_p => new PackagePathVM
                {
                    PackagePathEntry = _p,
                    IsNodeExpanded = true,
                    CommandHost = this
                }))
            .ToList();

        private void cmdOpenPackage(PackageFileVM packageVM)
        {
            try
            {
                LoadStatus.StartLoadStatusWindow();
                var _explore = new PackageExplorer(packageVM, _Config);
                tabDocuments.Items.Add(_explore);
                _explore.IsSelected = true;
            }
            catch (Exception _except)
            {
                MessageBox.Show(_except.Message, @"Workshop Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                LoadStatus.StopLoadStatusWindow();
            }
        }

        private void cmdNewCampaign(PackageSetVM packageSetVM)
        {
            var _newCamp = new NewCampaignDialog(packageSetVM)
            {
                Owner = this
            };
            if (_newCamp.ShowDialog() ?? false)
            {
                var _campRoot = packageSetVM.PackageSetPathEntry.Path;
                if (_campRoot != null)
                {
                    try
                    {
                        var _createFolder = $@"{_campRoot}/{_newCamp.CampaignName}";
                        Directory.CreateDirectory(_createFolder);
                        RefreshPackages();
                    }
                    catch (Exception _except)
                    {
                        MessageBox.Show(_except.Message, @"Workshop Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
        }

        private void cmdNewModule(PackageCampaignVM packageCampaignVM)
        {
            var _explore = new PackageExplorer(packageCampaignVM, _Config);
            _explore.CorePackage.Add(new Module(@"Module", new Description(@"Module")));
            _explore.RefreshTree();
            tabDocuments.Items.Add(_explore);
            _explore.IsSelected = true;
        }

        private void cmdNewResources(object targetVM)
        {
            var _explore =
                (targetVM is PackageCampaignVM _camp)
                ? new PackageExplorer(_camp, _Config)
                : new PackageExplorer(targetVM as PackagePathVM, _Config);
            _explore.CorePackage.Add(new VisualResources(_explore.CorePackage, @"Resources"));
            _explore.RefreshTree();
            tabDocuments.Items.Add(_explore);
            _explore.IsSelected = true;
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            var _about = new AboutWorkshop(this)
            {
                Owner = this
            };
            _about.ShowDialog();
        }

        private void tvwPackages_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // select item is right button is clicked
            if (e.MouseDevice.Target is FrameworkElement)
            {
                var element = e.MouseDevice.Target as FrameworkElement;

                if (element.TemplatedParent is ContentPresenter pres)
                {

                    if (pres.TemplatedParent is not TreeViewItem viewItem)
                    {
                        viewItem = tvwPackages.ItemContainerGenerator.ContainerFromItem(pres.Content) as TreeViewItem;
                    }

                    if (viewItem != null)
                    {
                        viewItem.IsSelected = true;
                    }
                }
            }
        }
    }
}