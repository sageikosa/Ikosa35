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
using System.IO.Packaging;
using System.IO;
using System.Diagnostics;
using Uzi.Ikosa.Tactical;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.Services;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using Uzi.Visualize;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.UI.MVVM.Package;
using Uzi.Ikosa.Workshop.ModuleManagement;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PackageExplorer.xaml
    /// </summary>
    public partial class PackageExplorer : TabItem, IHostTabControl
    {
        #region construction
        /// <summary>Open an existing package</summary>
        public PackageExplorer(PackageFileVM packageFile, IkosaPackageManagerConfig packageConfig)
        {
            _PackageFileVM = packageFile;

            InitializeComponent();
            tvwPackageParts.ItemContainerStyleSelector = new ContextMenuSelector(this);
            var _fInfo = new FileInfo(_PackageFileVM.PackageFileEntry.Path);
            _Package = new CorePackage(_fInfo, Package.Open(_PackageFileVM.PackageFileEntry.Path, FileMode.Open, FileAccess.Read, FileShare.Read));

            // open the module when loaded in package explorer
            _Package.Relationships.OfType<Module>().FirstOrDefault()?.Open();
            _Config = packageConfig;
            DataContext = this;
        }

        /// <summary>Construct a new package</summary>
        public PackageExplorer(IPackageFilesFolder fileFolder, IkosaPackageManagerConfig packageConfig)
        {
            InitializeComponent();
            tvwPackageParts.ItemContainerStyleSelector = new ContextMenuSelector(this);
            _Package = new CorePackage();
            tvwPackageParts.ItemsSource = _Package.Relationships;
            _PackagesFolder = fileFolder;
            _Config = packageConfig;
            DataContext = this;
        }
        #endregion

        #region Routed Commands
        public static readonly RoutedCommand OpenItem = new RoutedCommand();

        public static readonly RoutedCommand NewCellSpace = new RoutedCommand();
        public static readonly RoutedCommand NewWedgeSpace = new RoutedCommand();
        public static readonly RoutedCommand NewCornerSpace = new RoutedCommand();
        public static readonly RoutedCommand NewSliverSpace = new RoutedCommand();
        public static readonly RoutedCommand NewSlopeSpace = new RoutedCommand();
        public static readonly RoutedCommand NewStairsSpace = new RoutedCommand();
        public static readonly RoutedCommand NewLFrameSpace = new RoutedCommand();
        public static readonly RoutedCommand NewBorderSpace = new RoutedCommand();
        public static readonly RoutedCommand NewCylinderSpace = new RoutedCommand();
        public static readonly RoutedCommand NewSmallCylinderSpace = new RoutedCommand();

        public static readonly RoutedCommand NewNormalPanel = new RoutedCommand();
        public static readonly RoutedCommand NewCornerPanel = new RoutedCommand();
        public static readonly RoutedCommand NewLFramePanel = new RoutedCommand();
        public static readonly RoutedCommand NewSlopeComposite = new RoutedCommand();

        public static readonly RoutedCommand NewModel = new RoutedCommand();
        public static readonly RoutedCommand NewMetaModel = new RoutedCommand();
        public static readonly RoutedCommand NewFragment = new RoutedCommand();

        public static readonly RoutedCommand NewImageResourcesBefore = new RoutedCommand();
        public static readonly RoutedCommand NewImageResourcesAfter = new RoutedCommand();
        public static readonly RoutedCommand NewIconResourcesBefore = new RoutedCommand();
        public static readonly RoutedCommand NewIconResourcesAfter = new RoutedCommand();
        public static readonly RoutedCommand NewModelResourcesBefore = new RoutedCommand();
        public static readonly RoutedCommand NewModelResourcesAfter = new RoutedCommand();
        public static readonly RoutedCommand NewFragmentResourcesBefore = new RoutedCommand();
        public static readonly RoutedCommand NewFragmentResourcesAfter = new RoutedCommand();
        public static readonly RoutedCommand NewBrushResourcesBefore = new RoutedCommand();
        public static readonly RoutedCommand NewBrushResourcesAfter = new RoutedCommand();
        public static readonly RoutedCommand NewBrushSetResourcesBefore = new RoutedCommand();
        public static readonly RoutedCommand NewBrushSetResourcesAfter = new RoutedCommand();
        #endregion

        private readonly IkosaPackageManagerConfig _Config;
        private readonly PackageFileVM _PackageFileVM;
        private readonly IPackageFilesFolder _PackagesFolder;
        private readonly CorePackage _Package;

        public CorePackage CorePackage => _Package;
        public IPackageFilesFolder Folder => _PackageFileVM?.Folder ?? _PackagesFolder;

        public void FindOrOpen<TabType>(Func<TabType, bool> match, Func<TabType> generate) where TabType : TabItem
        {
            var _exist = trayItems.Items.OfType<TabType>().FirstOrDefault(_tt => match(_tt));
            if (_exist != null)
            {
                _exist.IsSelected = true;
            }
            else
            {
                var _create = generate();
                trayItems.Items.Add(_create);
                _create.IsSelected = true;
            }
        }

        public Window GetWindow() => Window.GetWindow(this);

        #region cbNew
        private void cbNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is BrushCollectionPart)
            {
                #region images attached to material collection
                var _collection = e.Parameter as BrushCollectionPart;
                var _opener = new System.Windows.Forms.OpenFileDialog()
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = @"All Files|*.gif;*.jpg;*.jpeg;*.bmp;*.png|GIF Images|*.gif|JPEG Images|*.jpg;*.jpeg|Bitmap Images|*.bmp|PNG Images|*.png",
                    Title = @"Import Image...",
                    ValidateNames = true
                };
                System.Windows.Forms.DialogResult _rslt = _opener.ShowDialog();

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);
                    using var _fStream = _fInfo.OpenRead();
                    // copy to stream and initialize image part
                    var _memStream = new MemoryStream((int)_fStream.Length);
                    StreamHelper.CopyStream(_fStream, _memStream);
                    var _bip = new BitmapImagePart(_collection, _memStream, _fInfo.Name);

                    // add image part
                    _collection.AddImage(_bip);
                }
                #endregion
            }
            else if (e.Parameter is Model3DPart)
            {
                #region images attached to Model3D
                var _model3D = e.Parameter as Model3DPart;
                var _opener = new System.Windows.Forms.OpenFileDialog()
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = @"All Files|*.gif;*.jpg;*.jpeg;*.bmp;*.png|GIF Images|*.gif|JPEG Images|*.jpg;*.jpeg|Bitmap Images|*.bmp|PNG Images|*.png",
                    Title = @"Import Image...",
                    ValidateNames = true
                };
                System.Windows.Forms.DialogResult _rslt = _opener.ShowDialog();

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);
                    using var _fStream = _fInfo.OpenRead();
                    // copy to stream and initialize image part
                    var _memStream = new MemoryStream((int)_fStream.Length);
                    StreamHelper.CopyStream(_fStream, _memStream);
                    var _bip = new BitmapImagePart(_model3D, _memStream, _fInfo.Name);

                    // add image part
                    _model3D.AddImage(_bip);
                }
                #endregion
            }
            else if (e.Parameter is RoomSet)
            {
                #region rooms
                var _map = (e.Parameter as RoomSet).Map;
                if (_map != null)
                {
                    var _dlg = new NewRoom()
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        _map.Rooms.Add(new Room(@"New Room", new CellLocation(0, 0, 0), new GeometricSize(1, 1, 1), _map, false, _dlg.IsEnclosed));
                    }
                }
                #endregion
            }
            else if (e.Parameter is BackgroundCellGroupSet)
            {
                #region background cell groups
                var _map = (e.Parameter as BackgroundCellGroupSet).Map;
                if (_map != null)
                {
                    var _window = new BackgroundCellGroupCreate(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    var _result = _window.ShowDialog();
                    if (_result.HasValue && _result.Value)
                    {
                        var _newBackground = _window.CreateGroup();
                        var _index = _window.TargetIndex;
                        if (_newBackground != null)
                        {
                            if (_index == -1)
                                _map.Backgrounds.AddBase(_newBackground);
                            else
                                _map.Backgrounds.AddOverlay(_newBackground, _index);
                        }
                    }
                }
                #endregion
            }
            else if (e.Parameter is MapContext)
            {
                #region locators
                var _map = (e.Parameter as MapContext).Map;
                if (_map != null)
                {
                    var _window = new LocatorCreate(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    var _result = _window.ShowDialog();
                    if (_result ?? false)
                    {
                        _window.GetLocator(_map.MapContext);
                    }
                }
                #endregion
            }
            else if (e.Parameter is PartsFolder)
            {
                var _folder = e.Parameter as PartsFolder;
                if (_folder.PartType.Equals(typeof(Model3DPart)))
                {
                    throw new NotImplementedException(@"Code path removed");
                }
                else if (_folder.PartType.Equals(typeof(BitmapImagePart)))
                {
                    #region images in folder
                    GetImageFile(out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        var _fName = _opener.FileName;
                        var _fInfo = new FileInfo(_fName);
                        using var _fStream = _fInfo.OpenRead();
                        // copy to stream and initialize image part
                        var _memStream = new MemoryStream((int)_fStream.Length);
                        StreamHelper.CopyStream(_fStream, _memStream);
                        _fStream.Close();

                        if (_folder.Parent is VisualResources _resourceManager)
                        {
                            var _bip = new BitmapImagePart(_resourceManager, _memStream, _fInfo.Name);

                            // add image part
                            _resourceManager.AddPart(_bip);
                        }
                    }
                    #endregion
                }
                else if (_folder.PartType.Equals(typeof(IconPart)))
                {
                    #region icons in folder
                    GetIconFile(@"Import Visual Icons...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        foreach (var _fName in _opener.FileNames)
                        {
                            var _fInfo = new FileInfo(_fName);
                            using var _fStream = _fInfo.OpenRead();
                            // initialize icon part
                            if (_folder.Parent is VisualResources _resourceManager)
                            {
                                var _iPart = new IconPart(_resourceManager, _fInfo);
                                if (_iPart.BindableName.EndsWith(@".xaml", StringComparison.OrdinalIgnoreCase))
                                    _iPart.BindableName = _iPart.BindableName.Substring(0, _iPart.BindableName.Length - 5);

                                // add image part
                                _resourceManager.AddPart(_iPart);
                            }
                        }
                    }
                    #endregion
                }
                else if (_folder.PartType.Equals(typeof(BrushCollectionPart)))
                {
                    #region 3D material collection
                    var _window = new BrushCollectionPartName()
                    {
                        Owner = Window.GetWindow(this)
                    };
                    var _result = _window.ShowDialog();
                    if (_result.HasValue && _result.Value)
                    {
                        if (_folder.Parent is VisualResources)
                        {
                            var _mgr = _folder.Parent as VisualResources;
                            var _newPart = new BrushCollectionPart(_mgr, _window.NewName);
                            _mgr.AddPart(_newPart);
                        }
                    }
                    #endregion
                }
                else if (_folder.PartType.Equals(typeof(MetaModelFragment)))
                {
                    #region new Fragment
                    GetFragmentFile(@"Import Fragment File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        var _fName = _opener.FileName;
                        var _fInfo = new FileInfo(_fName);

                        // add model part
                        if (_folder.Parent is VisualResources _resourceManager)
                        {
                            var _mPart = new MetaModelFragment(_resourceManager, _fInfo);
                            _resourceManager.AddPart(_mPart);
                        }
                    }
                    #endregion
                }
                else if (_folder.PartType.Equals(typeof(Uzi.Ikosa.Tactical.AmbientLight)))
                {
                    #region ambient lights
                    if (_folder.Parent is LocalMap _map)
                    {
                        var _window = new AmbientLightCreate(_map)
                        {
                            Owner = Window.GetWindow(this)
                        };
                        var _result = _window.ShowDialog();
                        if (_result.HasValue && _result.Value)
                        {
                            var _newAmbient = _window.CreateAmbientLight();
                            if (_newAmbient != null)
                            {
                                _map.AmbientLights.Add(_newAmbient.Name, _newAmbient);
                                _folder.ContentsChanged();
                            }
                        }

                    }
                    #endregion
                }
                else if (_folder.PartType.Equals(typeof(CellMaterial)))
                {
                    #region cell materials
                    if (_folder.Parent is LocalMap _map)
                    {
                        var _window = new CellMaterialCreate(_map)
                        {
                            Owner = Window.GetWindow(this)
                        };
                        var _result = _window.ShowDialog();
                        if (_result.HasValue && _result.Value)
                        {
                            var _newCM = _window.CreatedMaterial();
                            if (_newCM != null)
                            {
                                _map.CellMaterials.Add(_newCM.Name, _newCM);
                                _folder.ContentsChanged();
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        private static void GetIconFile(string prompt, out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt)
        {
            _opener = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = @"Icon Files|*.xaml",
                Title = prompt,
                ValidateNames = true,
                Multiselect = true
            };
            _rslt = _opener.ShowDialog();
        }

        private static void GetImageFile(out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt)
        {
            _opener = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = @"All Files|*.gif;*.jpg;*.jpeg;*.png|GIF Images|*.gif|JPEG Images|*.jpg;*.jpeg|PNG Images|*.png",
                Title = @"Import Image...",
                ValidateNames = true
            };
            _rslt = _opener.ShowDialog();
        }

        private static void GetFragmentFile(string prompt, out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt)
        {
            _opener = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = @"All Fragment Files|*.xaml;*.frag|Fragment Files|*.frag|XAML Files|*.xaml",
                Title = prompt,
                ValidateNames = true
            };
            _rslt = _opener.ShowDialog();
        }

        private static void GetModelFile(string prompt, out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt)
        {
            _opener = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = @"Model Files|*.xaml",
                Title = prompt,
                ValidateNames = true
            };
            _rslt = _opener.ShowDialog();
        }
        #endregion

        #region cbOpen
        private void cbOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is MetaModel _meta)
            {
                FindOrOpen(_mmt => _mmt.MetaModel == _meta,
                    () => new MetaModelEditorTab(_meta, this));
            }
            else if (e.Parameter is Model3DPart _model)
            {
                FindOrOpen(_mv => _mv.Model3DPart == _model,
                    () => new ModelViewer(_model, this));
            }
            else if (e.Parameter is BitmapImagePart _image)
            {
                FindOrOpen(_iv => _iv.ImagePart == _image,
                    () => new ImageViewer(_image, this));
            }
            else if (e.Parameter is IconPart _icon)
            {
                FindOrOpen(_iv => _iv.IconPart == _icon,
                    () => new IconViewerTab(_icon, this));
            }
            else if (e.Parameter is BrushCollectionPart)
            {
                #region open tile set
                var _coll = e.Parameter as BrushCollectionPart;
                var _exist = trayItems.Items.OfType<BrushCollectionPartEditor>().FirstOrDefault(_tse => _tse.Collection == _coll);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new BrushCollectionPartEditor(_coll, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is Tactical.AmbientLight)
            {
                #region ambient light
                var _ambient = e.Parameter as Uzi.Ikosa.Tactical.AmbientLight;
                var _exist = trayItems.Items.OfType<AmbientLightEditor>().FirstOrDefault(_le => _le.Light == _ambient);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new AmbientLightEditor(_ambient, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }

                #endregion
            }
            else if (e.Parameter is CellMaterial)
            {
                #region cell materials
                var _cMat = e.Parameter as CellMaterial;
                var _exist = trayItems.Items.OfType<CellMaterialEditor>().FirstOrDefault(_cme => _cme.CellMaterial == _cMat);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new CellMaterialEditor(_cMat, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is BackgroundCellGroup)
            {
                #region background cell groups
                var _bgCellGrp = e.Parameter as BackgroundCellGroup;
                var _exist = trayItems.Items.OfType<BackgroundCellGroupEditor>().FirstOrDefault(_bgcge => _bgcge.CellGroup == _bgCellGrp);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new BackgroundCellGroupEditor(_bgCellGrp, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is Room)
            {
                #region rooms
                var _room = e.Parameter as Room;
                var _exist = trayItems.Items.OfType<RoomEditor>().FirstOrDefault(_re => _re.Room == _room);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new RoomEditor(_room, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is Locator)
            {
                #region locators
                var _loc = e.Parameter as Locator;
                var _exist = trayItems.Items.OfType<LocatorEditor>().FirstOrDefault(_le => _le.Locator == _loc);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new LocatorEditor(_loc, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is Creature)
            {
                #region creature
                var _critter = e.Parameter as Creature;
                var _vm = (_critter.Setting is LocalMap _map)
                    ? new PresentableCreatureVM { Thing = _critter, VisualResources = _map.Resources }
                    : new PresentableCreatureVM { Thing = _critter };
                var _exist = trayItems.Items.OfType<CreatureEditor>().FirstOrDefault(_ce => _ce.Creature == _critter);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new CreatureEditor(_vm, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is PortalBase)
            {
                #region portal base
                var _port = e.Parameter as PortalBase;
                var _vm = (_port.Setting is LocalMap _map)
                    ? new PresentablePortalVM { Thing = _port, VisualResources = _map.Resources }
                    : new PresentablePortalVM { Thing = _port };
                var _exist = trayItems.Items.OfType<PortalEditor>().FirstOrDefault(_pe => _pe.Portal == _port);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new PortalEditor(_vm, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is ContainerObject)
            {
                #region container
                var _cont = e.Parameter as ContainerObject;
                var _exist = trayItems.Items.OfType<ContainerEditorTab>().FirstOrDefault(_ce => _ce.Container == _cont);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new ContainerEditorTab(_cont.GetPresentableObjectVM((_cont.Setting as LocalMap)?.Resources, null) as PresentableContainerObjectVM, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is CloseableContainerObject _closeCont)
            {
                FindOrOpen(_ce => _ce.CloseableContainerObject == _closeCont,
                    () => new CloseableContainerTab(_closeCont.GetPresentableObjectVM((_closeCont.Setting as LocalMap)?.Resources, null) as PresentableCloseableContainerVM, this)); ;
            }
            else if (e.Parameter is ContainerItemBase _cont)
            {
                // TODO: inject possessor vm
                FindOrOpen(_ce => _ce.ContainerItem == _cont,
                    () => new ContainerItemBaseTab(_cont.GetPresentableObjectVM((_cont.Setting as LocalMap)?.Resources, null) as PresentableContainerItemVM, this));
            }
            else if (e.Parameter is SlottedContainerItemBase _slotCont)
            {
                // TODO: inject possessor vm
                FindOrOpen(_ce => _ce.ContainerItem == _slotCont,
                    () => new SlottedContainerItemBaseTab(_slotCont.GetPresentableObjectVM((_slotCont.Setting as LocalMap)?.Resources, null) as PresentableSlottedContainerItemVM, this));
            }
            else if (e.Parameter is PartsFolder _folder)
            {
                // preview folder...images and icons
                if (_folder.PartType == typeof(IconPart))
                {
                    FindOrOpen(_tab => _tab.PartsFolder == _folder,
                        () => new IconFolderPreviewTab(_folder, this));
                }
                else if (_folder.PartType == typeof(ModuleNode))
                {
                    FindOrOpen(_tab => _tab.PartsFolder == _folder,
                        () => new ComponentsFolderTab(_folder, this));
                }
                else if (_folder.PartType == typeof(TeamGroupSummary))
                {
                    FindOrOpen(_tab => _tab.TeamGroupSummaryFolder.PartsFolder == _folder,
                        () => new TeamGroupSummaryTab(_folder, this));
                }
                else if (_folder.PartType == typeof(Variable))
                {
                    FindOrOpen(_tab => _tab.VariableFolder.PartsFolder == _folder,
                        () => new VariableTab(_folder, this));
                }
                else if (_folder.PartType == typeof(InfoKey))
                {
                    FindOrOpen(_tab => _tab.InfoKeyFolder.PartsFolder == _folder,
                        () => new InfoKeyTab(_folder, this));
                }
                else if (_folder.PartType == typeof(ItemElement))
                {
                    FindOrOpen(_tab => _tab.ItemElementFolder.PartsFolder == _folder,
                        () => new ItemElementTab(_folder, this));
                }
            }
            else if (e.Parameter is NamedKeysPart _nkp)
            {
                FindOrOpen(_tab => _tab.NamedKeysPart.Part == _nkp,
                    () => new NamedKeyTab(_nkp, this));
            }
            else if (e.Parameter is CorePackagePartsFolder _coreFolder)
            {
                // rename
                var _rename = new IkosaFolderRename(_coreFolder)
                {
                    Owner = Window.GetWindow(this)
                };
                if (_rename.ShowDialog() ?? false)
                {
                    _coreFolder.BindableName = _rename.FolderName;
                }
            }
            else if (e.Parameter is MetaModelFragment _frag)
            {
                // rename
                var _rename = new FragmentRename(_frag)
                {
                    Owner = Window.GetWindow(this)
                };
                if ((_rename.ShowDialog() ?? false) && !string.IsNullOrWhiteSpace(_rename.FragmentName))
                {
                    _frag.BindableName = _rename.FragmentName;
                }
            }
            else if (e.Parameter is LocalMap)
            {
                #region LocalMap
                var _preview = new Locale.Preview();
                _preview.LoadMap(e.Parameter as LocalMap);
                _preview.Show();
                _preview.Closed += new EventHandler(_preview_Closed);
                _Previews.Add(_preview);
                #endregion
            }
            else if (e.Parameter is ImportedLight)
            {
                #region imported light
                var _light = e.Parameter as ImportedLight;
                var _exist = trayItems.Items.OfType<ImportedLightEditor>().FirstOrDefault(_le => _le.ImportedLight == _light);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new ImportedLightEditor(_light, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is StructuralLight)
            {
                #region structural light
                var _light = e.Parameter as StructuralLight;
                var _exist = trayItems.Items.OfType<StructuralLightEditor>().FirstOrDefault(_le => _le.StructuralLight == _light);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new StructuralLightEditor(_light, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is LocalLinkSet)
            {
                #region localLinkSet
                var _set = e.Parameter as LocalLinkSet;
                var _exist = trayItems.Items.OfType<LocalLinkSetViewer>().FirstOrDefault(_le => _le.DataContext.Equals(_set));
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new LocalLinkSetViewer(_set, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is UserDefinitionsPart)
            {
                #region UserDefinitionsPart
                var _users = e.Parameter as UserDefinitionsPart;
                var _exist = trayItems.Items.OfType<UsersTab>().FirstOrDefault(_ut => _ut.DataContext.Equals(_users));
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new UsersTab(_users, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is PanelCellSpace)
            {
                #region PanelCellSpace
                var _panelCell = e.Parameter as PanelCellSpace;
                var _exist = trayItems.Items.OfType<PanelSpaceEditorTab>().FirstOrDefault(_pset => _pset.DataContext.Equals(_panelCell));
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new PanelSpaceEditorTab(_panelCell, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is SlopeCellSpace)
            {
                #region SlopeCellSpace
                var _slopeCell = e.Parameter as SlopeCellSpace;
                var _exist = trayItems.Items.OfType<SlopeEditorTab>().FirstOrDefault(_pset => _pset.DataContext.Equals(_slopeCell));
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new SlopeEditorTab(_slopeCell, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is SliverCellSpace)
            {
                #region SliverCellSpace
                var _sliverCell = e.Parameter as SliverCellSpace;
                var _exist = trayItems.Items.OfType<SliverEditorTab>().FirstOrDefault(_pset => _pset.DataContext.Equals(_sliverCell));
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _editor = new SliverEditorTab(_sliverCell, this);
                    trayItems.Items.Add(_editor);
                    _editor.IsSelected = true;
                }
                #endregion
            }
            else if (e.Parameter is WedgeCellSpace)
            {
            }
            else if (e.Parameter is Stairs)
            {
            }
            else if (e.Parameter is LFrame)
            {
            }
            else if (e.Parameter is CellSpace _cSpace)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_cSpace),
                    () => new CellSpaceEditorTab(_cSpace, this));
            }
            else if (e.Parameter is ImagesResourceReference _imgRsrcRef)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_imgRsrcRef),
                    () => new ImagesResourceReferenceTab(_imgRsrcRef, this, _Config));
            }
            else if (e.Parameter is IconsResourceReference _icoRsrcRef)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_icoRsrcRef),
                    () => new IconsResourceReferenceTab(_icoRsrcRef, this, _Config));
            }
            else if (e.Parameter is ModelsResourceReference _mdlRsrcRef)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_mdlRsrcRef),
                    () => new ModelsResourceReferenceTab(_mdlRsrcRef, this, _Config));
            }
            else if (e.Parameter is FragmentsResourceReference _fragRsrcRef)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_fragRsrcRef),
                    () => new FragmentsResourceReferenceTab(_fragRsrcRef, this, _Config));
            }
            else if (e.Parameter is BrushesResourceReference _brushRsrcRef)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_brushRsrcRef),
                    () => new BrushesResourceReferenceTab(_brushRsrcRef, this, _Config));
            }
            else if (e.Parameter is BrushSetsResourceReference _bSetRsrcRef)
            {
                FindOrOpen(_pset => _pset.DataContext.Equals(_bSetRsrcRef),
                    () => new BrushSetsResourceReferenceTab(_bSetRsrcRef, this, _Config));
            }
        }
        #endregion

        #region copy: Meta-Model duplicate
        private void cbCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is MetaModel)
            {
                var _meta = (e.Parameter as MetaModel);
                e.CanExecute = (_meta?.NameManager is VisualResources)
                    && (_meta?.NameManager.CanUseName($@"{_meta.Name} - Copy", typeof(MetaModel)) ?? false);
                e.Handled = true;
            }
        }

        private void cbCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is MetaModel)
            {
                #region open model
                // cast stuff (had to succees in CanExecute!)
                var _meta = e.Parameter as MetaModel;
                var _mgr = _meta.NameManager as VisualResources;

                // create and add
                _meta = new MetaModel(_mgr, _meta, $@"{_meta.Name} - Copy");
                _mgr.AddPart(_meta);

                // now open
                var _exist = trayItems.Items.OfType<MetaModelEditorTab>()
                    .FirstOrDefault(_mv => _mv.MetaModel == _meta);
                if (_exist != null)
                {
                    _exist.IsSelected = true;
                }
                else
                {
                    var _viewer = new MetaModelEditorTab(_meta, this);
                    trayItems.Items.Add(_viewer);
                    _viewer.IsSelected = true;
                }
                #endregion
            }
        }
        #endregion

        #region paste: tbd
        private void cbPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void cbPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region cbDelete
        private void cbDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // cannot delete root
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (trayItems.Items.OfType<IPackageItem>().FirstOrDefault(_pi => _pi.PackageItem == e.Parameter) is TabItem _tabItem)
            {
                trayItems.Items.Remove(_tabItem);
            }

            if (e.Parameter is Model3DPart)
            {
                #region Remove Model
                var _m3dPart = e.Parameter as Model3DPart;
                if (_m3dPart.NameManager != null)
                {
                    if (_m3dPart.NameManager is VisualResources)
                    {
                        (_m3dPart.NameManager as VisualResources).RemovePart(_m3dPart);
                    }
                    else if (_m3dPart.NameManager is CorePackage)
                    {
                        (_m3dPart.NameManager as CorePackage).Remove(_m3dPart);
                    }
                    else if (_m3dPart.NameManager is CorePackagePartsFolder)
                    {
                        (_m3dPart.NameManager as CorePackagePartsFolder).Remove(_m3dPart);
                    }
                }
                #endregion
            }
            else if (e.Parameter is MetaModelFragment)
            {
                #region Remove fragment
                var _fragPart = e.Parameter as MetaModelFragment;
                if (_fragPart.NameManager != null)
                {
                    if (_fragPart.NameManager is VisualResources)
                    {
                        (_fragPart.NameManager as VisualResources).RemovePart(_fragPart);
                    }
                    else if (_fragPart.NameManager is MetaModel)
                    {
                        (_fragPart.NameManager as MetaModel).RemoveFragment(_fragPart);
                    }
                }
                #endregion
            }
            else if (e.Parameter is BitmapImagePart)
            {
                #region Remove image
                var _bmiPart = e.Parameter as BitmapImagePart;
                if (_bmiPart.NameManager != null)
                {
                    if (_bmiPart.NameManager is VisualResources)
                    {
                        (_bmiPart.NameManager as VisualResources).RemovePart(_bmiPart);
                    }
                    else if (_bmiPart.NameManager is BrushCollectionPart)
                    {
                        (_bmiPart.NameManager as BrushCollectionPart).RemoveImage(_bmiPart);
                    }
                    else if (_bmiPart.NameManager is Model3DPart)
                    {
                        (_bmiPart.NameManager as Model3DPart).RemoveImage(_bmiPart);
                    }
                    else if (_bmiPart.NameManager is CorePackage)
                    {
                        (_bmiPart.NameManager as CorePackage).Remove(_bmiPart);
                    }
                    else if (_bmiPart.NameManager is CorePackagePartsFolder)
                    {
                        (_bmiPart.NameManager as CorePackagePartsFolder).Remove(_bmiPart);
                    }
                }
                #endregion
            }
            else if (e.Parameter is IconPart)
            {
                #region Remove icon
                var _icoPart = e.Parameter as IconPart;
                if (_icoPart.NameManager != null)
                {
                    if (_icoPart.NameManager is VisualResources)
                    {
                        (_icoPart.NameManager as VisualResources).RemovePart(_icoPart);
                    }
                    else if (_icoPart.NameManager is CorePackage)
                    {
                        (_icoPart.NameManager as CorePackage).Remove(_icoPart);
                    }
                    else if (_icoPart.NameManager is CorePackagePartsFolder)
                    {
                        (_icoPart.NameManager as CorePackagePartsFolder).Remove(_icoPart);
                    }
                }
                #endregion
            }
            else if (e.Parameter is BrushCollectionPart)
            {
                #region remove tileset
                var _matCollPart = e.Parameter as BrushCollectionPart;
                if (_matCollPart.NameManager != null)
                {
                    if (_matCollPart.NameManager is VisualResources)
                    {
                        (_matCollPart.NameManager as VisualResources).RemovePart(_matCollPart);
                    }
                    else if (_matCollPart.NameManager is CorePackage)
                    {
                        (_matCollPart.NameManager as CorePackage).Remove(_matCollPart);
                    }
                    else if (_matCollPart.NameManager is CorePackagePartsFolder)
                    {
                        (_matCollPart.NameManager as CorePackagePartsFolder).Remove(_matCollPart);
                    }
                }
                #endregion
            }
            else if (e.Parameter is Uzi.Ikosa.Tactical.AmbientLight)
            {
                #region remove ambient
                var _ambient = e.Parameter as Uzi.Ikosa.Tactical.AmbientLight;
                _ambient.Map.AmbientLights.Remove(_ambient.Name);
                foreach (var _group in _ambient.Map.Backgrounds.All().Where(_bg => _bg.Light == _ambient))
                {
                    _group.Light = null;
                }
                tvwPackageParts.Items.Refresh();
                #endregion
            }
            else if (e.Parameter is CellMaterial)
            {
                #region remove cell material
                var _cm = e.Parameter as CellMaterial;
                _cm.LocalMap.CellMaterials.Remove(_cm.Name);
                tvwPackageParts.Items.Refresh();
                #endregion
            }
            else if (e.Parameter is CellSpace)
            {
                var _space = e.Parameter as CellSpace;
                _space.Map.CellSpaces.Remove(_space);
                tvwPackageParts.Items.Refresh();
            }
            else if (e.Parameter is BasePanel)
            {
                var _panel = e.Parameter as BasePanel;
                _panel.Map.Panels.Remove(_panel);
                tvwPackageParts.Items.Refresh();
            }
            else if (e.Parameter is BackgroundCellGroup)
            {
                #region remove background cell group
                var _bcg = e.Parameter as BackgroundCellGroup;
                _bcg.Map.Backgrounds.Remove(_bcg);
                #endregion
            }
            else if (e.Parameter is Room)
            {
                var _room = e.Parameter as Room;
                _room.Map.Rooms.Remove(_room);
            }
            else if (e.Parameter is Locator)
            {
                var _loc = e.Parameter as Locator;
                _loc.MapContext.Remove(_loc);
            }
            else if (e.Parameter is PortalBase)
            {
                var _port = e.Parameter as PortalBase;
                var _loc = _port.GetLocated().Locator;
                _loc.MapContext.Remove(_loc);
            }
            else if (e.Parameter is LocalMap)
            {
                #region remove local map
                var _map = e.Parameter as LocalMap;
                if (_map.NameManager != null)
                {
                    if (_map.NameManager is CorePackage)
                    {
                        (_map.NameManager as CorePackage).Remove(_map);
                    }
                    else if (_map.NameManager is CorePackagePartsFolder)
                    {
                        (_map.NameManager as CorePackagePartsFolder).Remove(_map);
                    }
                }
                #endregion
            }
            else if (e.Parameter is CorePackagePartsFolder)
            {
                #region remove folder
                var _folder = e.Parameter as CorePackagePartsFolder;
                if (_folder.NameManager != null)
                {
                    if (_folder.NameManager is CorePackage)
                    {
                        (_folder.NameManager as CorePackage).Remove(_folder);
                    }
                    else if (_folder.NameManager is CorePackagePartsFolder)
                    {
                        (_folder.NameManager as CorePackagePartsFolder).Remove(_folder);
                    }
                }
                #endregion
            }
            else if (e.Parameter is ImagesResourceReference)
            {
                var _imgRef = e.Parameter as ImagesResourceReference;
                _imgRef.Parent.RemoveImageResourceReference(_imgRef);
            }
            else if (e.Parameter is IconsResourceReference)
            {
                var _imgRef = e.Parameter as IconsResourceReference;
                _imgRef.Parent.RemoveIconResourceReference(_imgRef);
            }
            else if (e.Parameter is ModelsResourceReference)
            {
                var _mdlRef = e.Parameter as ModelsResourceReference;
                _mdlRef.Parent.RemoveModelResourceReference(_mdlRef);
            }
            else if (e.Parameter is FragmentsResourceReference)
            {
                var _fragRef = e.Parameter as FragmentsResourceReference;
                _fragRef.Parent.RemoveFragmentResourceReference(_fragRef);
            }
            else if (e.Parameter is BrushesResourceReference)
            {
                var _brshRef = e.Parameter as BrushesResourceReference;
                _brshRef.Parent.RemoveBrushResourceReference(_brshRef);
            }
            else if (e.Parameter is BrushSetsResourceReference)
            {
                var _bSetRef = e.Parameter as BrushSetsResourceReference;
                _bSetRef.Parent.RemoveBrushSetResourceReference(_bSetRef);
            }
        }
        #endregion

        private readonly List<Locale.Preview> _Previews = new List<Locale.Preview>();

        private void _preview_Closed(object sender, EventArgs e)
        {
            if (sender is Locale.Preview _pre)
            {
                _pre.Closed -= new EventHandler(_preview_Closed);
                _Previews.Remove(_pre);
            }
        }

        #region private void tvwPackageParts_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        private void tvwPackageParts_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // select item is right button is clicked
            if (e.MouseDevice.Target is FrameworkElement)
            {
                var element = e.MouseDevice.Target as FrameworkElement;

                if (element.TemplatedParent is ContentPresenter pres)
                {

                    if (pres.TemplatedParent is not TreeViewItem viewItem)
                    {
                        viewItem = tvwPackageParts.ItemContainerGenerator.ContainerFromItem(pres.Content) as
                        TreeViewItem;
                    }

                    if (viewItem != null)
                    {
                        viewItem.IsSelected = true;
                    }
                }
            }
        }
        #endregion

        #region private void btnClose_Click(object sender, RoutedEventArgs e)
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            switch (MessageBox.Show(@"Save package?", @"Closing Package...", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    if (_Package.Package == null)
                    {
                        if (_PackagesFolder != null)
                        {
                            var _saveAs = new SavePackageAsDialog(_PackagesFolder)
                            {
                                Owner = _PackagesFolder.RefreshHost as Window
                            };
                            if (_saveAs.ShowDialog() ?? false)
                            {
                                LoadStatus.StartLoadStatusWindow();
                                try
                                {
                                    _Package.SaveAs($@"{_PackagesFolder.PackagePathEntry.Path}/{_saveAs.PackageName}.ikosa");
                                    _PackagesFolder.RefreshHost.RefreshPackages();
                                }
                                finally
                                {
                                    LoadStatus.StopLoadStatusWindow();
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        LoadStatus.StartLoadStatusWindow();
                        try
                        {
                            _Package.Save();
                        }
                        finally
                        {
                            LoadStatus.StopLoadStatusWindow();
                        }
                    }
                    break;

                case MessageBoxResult.No:
                    break;

                case MessageBoxResult.Cancel:
                    return;
            }

            try
            {
                _Package.Close();
                (Parent as TabControl).Items.Remove(this);
                foreach (var _preview in _Previews.ToList())
                    _preview.Close();
            }
            catch { }
        }
        #endregion

        private void btnRefreshPackage_Click(object sender, RoutedEventArgs e)
        {
            RefreshTree();
        }

        public void RefreshTree()
        {
            tvwPackageParts.ItemsSource = null;
            tvwPackageParts.ItemsSource = _Package.Relationships;
        }

        #region Package Saving
        private void btnSavePackageAs_Click(object sender, RoutedEventArgs e)
        {
            if (Folder != null)
            {
                var _saveAs = new SavePackageAsDialog(Folder)
                {
                    Owner = Folder.RefreshHost as Window
                };
                if (_saveAs.ShowDialog() ?? false)
                {
                    try
                    {
                        LoadStatus.StartLoadStatusWindow();
                        try
                        {
                            _Package.SaveAs($@"{Folder.PackagePathEntry.Path}/{_saveAs.PackageName}.ikosa");
                            Folder.RefreshHost.RefreshPackages();
                        }
                        finally
                        {
                            LoadStatus.StopLoadStatusWindow();
                        }
                    }
                    catch (Exception _except)
                    {
                        MessageBox.Show(_except.Message, @"Workshop", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnSavePackage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Package.Package == null)
                {
                    if (_PackagesFolder != null)
                    {
                        var _saveAs = new SavePackageAsDialog(_PackagesFolder)
                        {
                            Owner = _PackagesFolder.RefreshHost as Window
                        };
                        if (_saveAs.ShowDialog() ?? false)
                        {
                            LoadStatus.StartLoadStatusWindow();
                            try
                            {
                                _Package.SaveAs($@"{_PackagesFolder.PackagePathEntry.Path}/{_saveAs.PackageName}.ikosa");
                                _PackagesFolder.RefreshHost.RefreshPackages();
                            }
                            finally
                            {
                                LoadStatus.StopLoadStatusWindow();
                            }
                        }
                    }
                }
                else
                {
                    LoadStatus.StartLoadStatusWindow();
                    try
                    {
                        BasePartHelper.LoadMessage?.Invoke(@"Saving...");
                        _Package.Save();
                    }
                    finally
                    {
                        LoadStatus.StopLoadStatusWindow();
                    }
                }
            }
            catch (Exception _except)
            {
                MessageBox.Show(_except.Message, @"Workshop", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region private void mnuItemNew_Click(object sender, RoutedEventArgs e)
        private void mnuItemNew_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem _menu)
            {
                if (_menu.Tag.ToString().Equals(@"LocalMap"))
                {
                    // TODO: add some defaults?
                    _Package.Add(new LocalMap()
                    {
                        BindableName = string.Format(@"Map_{0}", _Package.Relationships.Count() + 1)
                    });
                }
                else if (_menu.Tag.ToString().Equals(@"Module"))
                {
                    _Package.Add(new Module(@"Module1", new Core.Contracts.Description(@"Module1")));
                }
                // TODO: resource manager
                else if (_menu.Tag.ToString().Equals(@"MetaModel"))
                {
                    #region meta-model
                    GetModelFile(@"Import Meta-Model File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        var _fName = _opener.FileName;
                        var _fInfo = new FileInfo(_fName);

                        // add model part
                        var _mPart = new MetaModel(_Package, _fInfo);
                        _Package.Add(_mPart);
                    }
                    #endregion
                }
                else if (_menu.Tag.ToString().Equals(@"Model"))
                {
                    #region model
                    GetModelFile(@"Import Model File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        var _fName = _opener.FileName;
                        var _fInfo = new FileInfo(_fName);

                        // add model part
                        var _mPart = new Model3DPart(_Package, _fInfo);
                        _Package.Add(_mPart);
                    }
                    #endregion
                }
                else if (_menu.Tag.ToString().Equals(@"Image"))
                {
                    #region image
                    GetImageFile(out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        var _fName = _opener.FileName;
                        var _fInfo = new FileInfo(_fName);
                        using var _fStream = _fInfo.OpenRead();
                        // copy to stream and initialize image part
                        var _memStream = new MemoryStream((int)_fStream.Length);
                        StreamHelper.CopyStream(_fStream, _memStream);
                        _fStream.Close();

                        var _bip = new BitmapImagePart(_Package, _memStream, _fInfo.Name);

                        // add image part
                        _Package.Add(_bip);
                    }
                    #endregion
                }
                else if (_menu.Tag.ToString().Equals(@"Icon"))
                {
                    #region icon
                    GetIconFile(@"Import Visual Icons...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                    // process results
                    if (_rslt == System.Windows.Forms.DialogResult.OK)
                    {
                        // open file
                        foreach (var _fName in _opener.FileNames)
                        {
                            var _fInfo = new FileInfo(_fName);
                            using var _fStream = _fInfo.OpenRead();
                            var _ip = new IconPart(_Package, _fInfo);
                            if (_ip.BindableName.EndsWith(@".xaml", StringComparison.OrdinalIgnoreCase))
                                _ip.BindableName = _ip.BindableName.Substring(0, _ip.BindableName.Length - 5);

                            // add icon part
                            _Package.Add(_ip);
                        }
                    }
                    #endregion
                }
                else if (_menu.Tag.ToString().Equals(@"MaterialCollection"))
                {
                    var _window = new BrushCollectionPartName()
                    {
                        Owner = Window.GetWindow(this)
                    };
                    var _result = _window.ShowDialog();
                    if (_result.HasValue && _result.Value)
                    {
                        var _newPart = new BrushCollectionPart(_Package, _window.NewName);
                        _Package.Add(_newPart);
                    }
                }
                else if (_menu.Tag.ToString().Equals(@"ResourceManager"))
                {
                    _Package.Add(new VisualResources(_Package, string.Format(@"Resources_{0}", _Package.Relationships.Count() + 1)));
                }
                else if (_menu.Tag.ToString().Equals(@"Object"))
                {
                    // TODO:
                }
            }
            RefreshTree();
        }
        #endregion

        #region private void miIkosaFolderNew_Click(object sender, RoutedEventArgs e)
        private void miIkosaFolderNew_Click(object sender, RoutedEventArgs e)
        {
            var _folder = (sender as MenuItem).Tag as CorePackagePartsFolder;
            var _tag = (e.Source as MenuItem).Tag.ToString();
            if (_tag.Equals(@"LocalMap"))
            {
                _folder.Add(new LocalMap() { BindableName = string.Format(@"Map_{0}", _folder.Relationships.Count() + 1) });
            }
            else if (_tag.Equals(@"Model"))
            {
                #region New Model
                GetModelFile(@"Import Model File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);

                    // add model part
                    var _mPart = new Model3DPart(_folder, _fInfo);
                    _folder.Add(_mPart);
                }
                #endregion
            }
            else if (_tag.Equals(@"MetaModel"))
            {
                #region new Meta Model
                GetModelFile(@"Import Meta-Model File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);

                    // add model part
                    var _mPart = new MetaModel(_folder, _fInfo);
                    _folder.Add(_mPart);
                }
                #endregion
            }
            else if (_tag.Equals(@"Fragment"))
            {
                #region new Fragment
                GetFragmentFile(@"Import Fragment File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);

                    // add model part
                    var _mPart = new MetaModelFragment(_folder, _fInfo);
                    _folder.Add(_mPart);
                }
                #endregion
            }
            else if (_tag.Equals(@"Image"))
            {
                #region new image
                GetImageFile(out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);
                    using var _fStream = _fInfo.OpenRead();
                    // copy to stream and initialize image part
                    var _memStream = new MemoryStream((int)_fStream.Length);
                    StreamHelper.CopyStream(_fStream, _memStream);
                    _fStream.Close();
                    var _bip = new BitmapImagePart(_folder, _memStream, _fInfo.Name);

                    // add image part
                    _folder.Add(_bip);
                }
                #endregion
            }
            else if (_tag.Equals(@"Icon"))
            {
                #region new Icon
                GetModelFile(@"Import Visual Icons...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    foreach (var _fName in _opener.FileNames)
                    {
                        var _fInfo = new FileInfo(_fName);

                        // add icon part
                        var _iPart = new IconPart(_folder, _fInfo);
                        if (_iPart.BindableName.EndsWith(@".xaml", StringComparison.OrdinalIgnoreCase))
                            _iPart.BindableName = _iPart.BindableName.Substring(0, _iPart.BindableName.Length - 5);
                        _folder.Add(_iPart);
                    }
                }
                #endregion
            }
            else if (_tag.Equals(@"MaterialCollection"))
            {
                var _window = new BrushCollectionPartName()
                {
                    Owner = Window.GetWindow(this)
                };
                var _result = _window.ShowDialog();
                if (_result.HasValue && _result.Value)
                {
                    var _newPart = new BrushCollectionPart(_folder, _window.NewName);
                    _folder.Add(_newPart);
                }
            }
            else if (_tag.Equals(@"Object"))
            {
                // TODO:
            }
            else if (_tag.Equals(@"ResourceManager"))
            {
                _folder.Add(new VisualResources(_folder, string.Format(@"Resources_{0}", _folder.Relationships.Count() + 1)));
            }
        }
        #endregion

        #region cbNewCellSpace
        private void cbNewCellSpace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter.ToString() != @"Border");
            e.Handled = true;
        }

        private void cbNewCellSpace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _parts)
            {
                var _map = _parts.Parent as LocalMap;
                if (e.Command == PackageExplorer.NewCellSpace)
                {
                    var _dlg = new NewCellSpace(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetCellSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewWedgeSpace)
                {
                    var _dlg = new NewWedgeCorner(_map, @"New Wedge")
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetWedgeCellSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewCornerSpace)
                {
                    var _dlg = new NewWedgeCorner(_map, @"New WeCornerdge")
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetCornerCellSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewSliverSpace)
                {
                    var _dlg = new NewSliver(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetSliverCellSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewSlopeSpace)
                {
                    var _dlg = new NewSlope(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetSlopeCellSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewCylinderSpace)
                {
                    var _dlg = new NewCylinder(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetCylinderSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewSmallCylinderSpace)
                {
                    var _dlg = new NewSmallCylinder(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetSmallCylinderSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewStairsSpace)
                {
                    var _dlg = new NewStairs(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetStairs(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewLFrameSpace)
                {
                    var _dlg = new NewLFrame(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetLFrame(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewBorderSpace)
                {
                    // Panel Space
                    var _dlg = new NewPanelSpace(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _cSpace = _dlg.GetPanelCellSpace(null);
                        _map.CellSpaces.Add(_cSpace);
                        _parts.ContentsChanged();
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region IHostTabControl Members
        public void RemoveTabItem(IHostedTabItem item)
        {
            if (trayItems.Items.Contains(item))
            {
                item.CloseTabItem();
                trayItems.Items.Remove(item);
            }
        }
        #endregion

        #region cbNewFragment
        private void cbNewFragment_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNewFragment_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is MetaModel)
            {
                var _metaModel = e.Parameter as MetaModel;
                var _opener = new System.Windows.Forms.OpenFileDialog()
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = @"All Files|*.xaml;*.xml;*.frag|XAML Fragments|*.xaml|XML Fragments|*.xml|Fragments|*.frag",
                    Title = @"Import Fragment...",
                    ValidateNames = true
                };
                System.Windows.Forms.DialogResult _rslt = _opener.ShowDialog();

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);
                    var _fragPart = new MetaModelFragment(_metaModel, _fInfo);

                    // add image part
                    _metaModel.AddFragment(_fragPart);
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbNewModel
        private void cbNewModel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNewModel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _folder = e.Parameter as PartsFolder;
            if (_folder.PartType.Equals(typeof(Model3DPart)))
            {
                #region Models
                GetModelFile(@"Import Model File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);

                    // add model part
                    if (_folder.Parent is VisualResources _mgr)
                    {
                        var _mPart = new Model3DPart(_mgr, _fInfo);
                        _mgr.AddPart(_mPart);
                    }
                }
                #endregion
            }

        }
        #endregion

        #region cbNewMetaModel
        private void cbNewMetaModel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNewMetaModel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _folder = e.Parameter as PartsFolder;
            if (_folder.PartType.Equals(typeof(Model3DPart)))
            {
                #region Models
                GetModelFile(@"Import Meta-Model File...", out System.Windows.Forms.OpenFileDialog _opener, out System.Windows.Forms.DialogResult _rslt);

                // process results
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    // open file
                    var _fName = _opener.FileName;
                    var _fInfo = new FileInfo(_fName);

                    // add model part
                    if (_folder.Parent is VisualResources _mgr)
                    {
                        var _mPart = new MetaModel(_mgr, _fInfo);
                        _mgr.AddPart(_mPart);
                    }
                }
                #endregion
            }

        }
        #endregion

        #region cbNewPanel
        private void cbNewPanelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNewPanelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _parts)
            {
                var _map = _parts.Parent as LocalMap;
                if (e.Command == PackageExplorer.NewNormalPanel)
                {
                    var _dlg = new NewNormalPanel(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _panel = _dlg.GetNormalPanel();
                        _map.Panels.Add(_panel);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewCornerPanel)
                {
                    var _dlg = new NewCornerPanel(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _panel = _dlg.GetCornerPanel();
                        _map.Panels.Add(_panel);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewLFramePanel)
                {
                    var _dlg = new NewLFramePanel(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _panel = _dlg.GetLFramePanel();
                        _map.Panels.Add(_panel);
                        _parts.ContentsChanged();
                    }
                }
                else if (e.Command == PackageExplorer.NewSlopeComposite)
                {
                    var _dlg = new NewSlopeComposite(_map)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _panel = _dlg.GetSlopeComposite();
                        _map.Panels.Add(_panel);
                        _parts.ContentsChanged();
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        private void cbNewResources_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        #region resource references add

        #region private void cbNewImageResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewImageResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.ImageResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.InsertImageResourceReference(0, new ImagesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is ImagesResourceReference _imgRef)
                {
                    var _refMgr = _imgRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.ImageResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            _refMgr.InsertImageResourceReference(_refMgr.GetIndex(_imgRef), new ImagesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewImageResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewImageResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.ImageResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.AddImageResourceReference(new ImagesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is ImagesResourceReference _imgRef)
                {
                    var _refMgr = _imgRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.ImageResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            var _newImgRef = new ImagesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty);
                            var _index = _refMgr.GetIndex(_imgRef);
                            if (_index < _refMgr.ImageResourceReferenceCount - 1)
                                _refMgr.InsertImageResourceReference(_index + 1, _newImgRef);
                            else
                                _refMgr.AddImageResourceReference(_newImgRef);
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewIconResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewIconResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.IconResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.InsertIconResourceReference(0, new IconsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is ImagesResourceReference _imgRef)
                {
                    var _refMgr = _imgRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.IconResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            _refMgr.InsertIconResourceReference(_refMgr.GetIndex(_imgRef), new IconsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewIconResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewIconResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.IconResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.AddIconResourceReference(new IconsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is ImagesResourceReference _imgRef)
                {
                    var _refMgr = _imgRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.IconResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            var _newImgRef = new IconsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty);
                            var _index = _refMgr.GetIndex(_imgRef);
                            if (_index < _refMgr.IconResourceReferenceCount - 1)
                                _refMgr.InsertIconResourceReference(_index + 1, _newImgRef);
                            else
                                _refMgr.AddIconResourceReference(_newImgRef);
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewModelResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewModelResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.ModelResourceReferences.Any(_mrr => _mrr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.InsertModelResourceReference(0, new ModelsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is ModelsResourceReference _mdlRef)
                {
                    var _refMgr = _mdlRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.ModelResourceReferences.Any(_mrr => _mrr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            _refMgr.InsertModelResourceReference(_refMgr.GetIndex(_mdlRef), new ModelsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewModelResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewModelResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.ModelResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.AddModelResourceReference(new ModelsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is ModelsResourceReference _mdlRef)
                {
                    var _refMgr = _mdlRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.ModelResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            var _newMdlRef = new ModelsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty);
                            var _index = _refMgr.GetIndex(_mdlRef);
                            if (_index < _refMgr.ModelResourceReferenceCount - 1)
                                _refMgr.InsertModelResourceReference(_index + 1, _newMdlRef);
                            else
                                _refMgr.AddModelResourceReference(_newMdlRef);
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewFragmentResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewFragmentResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.FragmentResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.InsertFragmentResourceReference(0, new FragmentsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is FragmentsResourceReference _fragRef)
                {
                    var _refMgr = _fragRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.FragmentResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            _refMgr.InsertFragmentResourceReference(_refMgr.GetIndex(_fragRef), new FragmentsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewFragmentResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewFragmentResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.FragmentResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.AddFragmentResourceReference(new FragmentsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is FragmentsResourceReference _fragRef)
                {
                    var _refMgr = _fragRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.FragmentResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            var _newFragRef = new FragmentsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty);
                            var _index = _refMgr.GetIndex(_fragRef);
                            if (_index < _refMgr.FragmentResourceReferenceCount - 1)
                                _refMgr.InsertFragmentResourceReference(_index + 1, _newFragRef);
                            else
                                _refMgr.AddFragmentResourceReference(_newFragRef);
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewBrushResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewBrushResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.BrushesResourceReferences.Any(_brr => _brr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.InsertBrushResourceReference(0, new BrushesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is BrushesResourceReference _brshRef)
                {
                    var _refMgr = _brshRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.BrushesResourceReferences.Any(_brr => _brr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            _refMgr.InsertBrushResourceReference(_refMgr.GetIndex(_brshRef), new BrushesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewBrushResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewBrushResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.BrushesResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.AddBrushResourceReference(new BrushesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is BrushesResourceReference _brshRef)
                {
                    var _refMgr = _brshRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.BrushesResourceReferences.Any(_irr => _irr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            var _newBrshRef = new BrushesResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty);
                            var _index = _refMgr.GetIndex(_brshRef);
                            if (_index < _refMgr.BrushResourceReferenceCount - 1)
                                _refMgr.InsertBrushResourceReference(_index + 1, _newBrshRef);
                            else
                                _refMgr.AddBrushResourceReference(_newBrshRef);
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewBrushSetResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewBrushSetResourcesBefore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.BrushSetResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.InsertBrushSetResourceReference(0, new BrushSetsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is BrushSetsResourceReference _bSetRef)
                {
                    var _refMgr = _bSetRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.BrushSetResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            _refMgr.InsertBrushSetResourceReference(_refMgr.GetIndex(_bSetRef), new BrushSetsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void cbNewBrushSetResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNewBrushSetResourcesAfter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PartsFolder _folder)
            {
                if (_folder.Parent is ResourceReferenceManager _refMgr)
                {
                    var _newRef = new NewReference((name) => !_refMgr.BrushSetResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_newRef.ShowDialog() ?? false)
                    {
                        _refMgr.AddBrushSetResourceReference(new BrushSetsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty));
                    }
                }
            }
            else
            {
                if (e.Parameter is BrushSetsResourceReference _bSetRef)
                {
                    var _refMgr = _bSetRef.Parent;
                    if (_refMgr != null)
                    {
                        var _newRef = new NewReference((name) => !_refMgr.BrushSetResourceReferences.Any(_frr => _frr.Name.Equals(name)))
                        {
                            Owner = Window.GetWindow(this)
                        };
                        if (_newRef.ShowDialog() ?? false)
                        {
                            var _newBSetRef = new BrushSetsResourceReference(_refMgr, _newRef.ReferenceName, string.Empty, string.Empty, string.Empty);
                            var _index = _refMgr.GetIndex(_bSetRef);
                            if (_index < _refMgr.BrushSetResourceReferenceCount - 1)
                                _refMgr.InsertBrushSetResourceReference(_index + 1, _newBSetRef);
                            else
                                _refMgr.AddBrushSetResourceReference(_newBSetRef);
                        }
                    }
                }
            }
        }
        #endregion

        #endregion
    }

    public class ContextMenuSelector : StyleSelector
    {
        public ContextMenuSelector(PackageExplorer pExplore)
        {
            _PExplore = pExplore;
        }

        private PackageExplorer _PExplore;

        #region public override Style SelectStyle(object item, DependencyObject container)
        public override Style SelectStyle(object item, DependencyObject container)
        {
            //ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            //TreeViewItem _item = container as TreeViewItem;
            Style _resource(string name)
                => _PExplore.Resources[name] as Style;

            switch (item)
            {
                case BrushCollectionPart: return _resource(@"stylePassthrough");
                case MetaModel: return _resource(@"styleMetaModel");
                case Model3DPart: return _resource(@"stylePassthrough");
                case MetaModelFragment: return _resource(@"stylePart");

                case PartsFolder:
                    {
                        var _parts = item as PartsFolder;
                        // (visual) resources
                        // .../images
                        if (_parts.PartType == typeof(BitmapImagePart))
                            return _resource(@"stylePreviewFolder");
                        // .../icons
                        if (_parts.PartType == typeof(IconPart))
                            return _resource(@"stylePreviewFolder");
                        // .../fragments
                        if (_parts.PartType == typeof(PartsFolder))
                            return _resource(@"styleNone");
                        // .../models
                        if (_parts.PartType == typeof(Model3DPart))
                            return _resource(@"styleModelFolder");
                        // (visual) resource references
                        if (_parts.PartType == typeof(ImagesResourceReference))
                            return _resource(@"styleImagesResourceFolder");
                        if (_parts.PartType == typeof(IconsResourceReference))
                            return _resource(@"styleIconsResourceFolder");
                        if (_parts.PartType == typeof(ModelsResourceReference))
                            return _resource(@"styleModelsResourceFolder");
                        if (_parts.PartType == typeof(FragmentsResourceReference))
                            return _resource(@"styleFragmentsResourceFolder");
                        if (_parts.PartType == typeof(BrushSetsResourceReference))
                            return _resource(@"styleBrushSetsResourceFolder");
                        if (_parts.PartType == typeof(BrushesResourceReference))
                            return _resource(@"styleBrushesResourceFolder");

                        // cell templating/
                        // .../cell spaces
                        if (_parts.PartType == typeof(CellSpace))
                            return _resource(@"styleCellSpaceFolder");
                        // .../cell panels
                        if (_parts.PartType == typeof(BasePanel))
                            return _resource(@"styleCellPanelFolder");
                    }
                    return _resource(@"styleFolder");

                case BitmapImagePart: return _resource(@"styleImage");
                case IconPart: return _resource(@"stylePart");
                case CorePackagePartsFolder: return _resource(@"styleIkosaFolder");

                case RoomSet:
                case BackgroundCellGroupSet:
                case MapContext:
                    return _resource(@"styleFolder");

                case ImagesResourceReference: return _resource(@"styleImagesResourceReference");
                case IconsResourceReference: return _resource(@"styleIconsResourceReference");
                case ModelsResourceReference: return _resource(@"styleModelsResourceReference");
                case FragmentsResourceReference: return _resource(@"styleFragmentsResourceReference");
                case BrushSetsResourceReference: return _resource(@"styleBrushesResourceReference");
                case BrushesResourceReference: return _resource(@"styleBrushSetsResourceReference");
                case LocalMap: return _resource(@"styleOpen");

                case Room:
                case BackgroundCellGroup:
                case CellMaterial:
                case CellSpace:
                    return _resource(@"stylePart");

                case VisualResources:
                default:
                    {
                        // TODO:
                    }
                    return _resource(@"styleNone");
            }
        }
        #endregion
    }
}
