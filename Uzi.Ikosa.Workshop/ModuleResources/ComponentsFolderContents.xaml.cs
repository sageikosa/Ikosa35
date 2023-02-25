using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.Guildsmanship.Overland;
using Uzi.Ikosa.UI;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SiteFolderContents.xaml
    /// </summary>
    public partial class ComponentsFolderContents : UserControl
    {
        public ComponentsFolderContents()
        {
            InitializeComponent();
        }

        public IHostTabControl HostTabControl
        {
            get { return (IHostTabControl)GetValue(HostedTabControlProperty); }
            set { SetValue(HostedTabControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HostedTabControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HostedTabControlProperty =
            DependencyProperty.Register(nameof(HostTabControl), typeof(IHostTabControl), typeof(ComponentsFolderContents),
                new PropertyMetadata(null));

        public PartsFolder PartsFolder => DataContext as PartsFolder;

        private string GetNewNamedComponent(string title, string tryName)
        {
            var _dlg = new NewName(title, tryName, PartsFolder.Parent as ModuleResources)
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                return _dlg.ReturnedName;
            }
            return null;
        }

        private void miNewMap_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                var _name = GetNewNamedComponent(@"New Map", @"map");
                if (!string.IsNullOrWhiteSpace(_name))
                {
                    _resources.AddPart(new LocalMapSite(new Description(_name)));
                }
            }
        }

        private void miNewSettlement_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                var _name = GetNewNamedComponent(@"New Settlement", @"settlement");
                if (!string.IsNullOrWhiteSpace(_name))
                {
                    _resources.AddPart(new Settlement(new Description(_name)));
                }
            }
        }

        private void miNewCreature_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                var _newCritter = new NewCreature(_resources)
                {
                    Owner = Window.GetWindow(this)
                };
                if (_newCritter.ShowDialog() ?? false)
                {
                    var _critter = _newCritter.GetCreature();
                    _resources.AddPart(new CreatureNode(new Description(_newCritter.CreatureName), _critter));
                }
            }
        }

        private void miNewEncounter_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                var _name = GetNewNamedComponent(@"New Encounter Table", @"encountertable");
                if (!string.IsNullOrWhiteSpace(_name))
                {
                    _resources.AddPart(new EncounterTable(new Description(_name), new DieRoller(20)));
                }
            }
        }

        private void miNewRegion_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                var _name = GetNewNamedComponent(@"New Region", @"region");
                if (!string.IsNullOrWhiteSpace(_name))
                {
                    _resources.AddPart(new Region(new Description(_name)));
                }
            }
        }

        private void miNewSitePathGraph_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                // TODO: need to gather two site endpoints, path type and reverse factor
                var _name = GetNewNamedComponent(@"New Encounter Table", @"encountertable");
                if (!string.IsNullOrWhiteSpace(_name))
                {
                    _resources.AddPart(new SitePathGraph(new Description(_name), null));
                }
            }
        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
            }
        }

        private void lstComponents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                var _visual = _resources?.Module.Visuals;
                if (lstComponents?.SelectedItem is ModuleNode _modNode)
                {
                    switch (_modNode)
                    {
                        case CreatureNode _critterNode:
                            HostTabControl.FindOrOpen(_ce => _ce.Creature == _critterNode.Creature,
                                () => new CreatureEditor(_critterNode.Creature.GetPresentableObjectVM(_visual, null) as PresentableCreatureVM, HostTabControl));
                            break;

                        case LocalMapSite _mapSite:
                            HostTabControl.FindOrOpen(_lmse => _lmse.LocalMapSite == _mapSite,
                                () => new LocalMapSiteTab(_mapSite, HostTabControl));
                            break;

                        case Settlement _settlement:
                            HostTabControl.FindOrOpen(_s => _s.Settlement == _settlement,
                                () => new SettlementTab(_settlement, HostTabControl));
                            break;

                        case NonPlayerService _service:
                            // TODO: non-player service editor
                            break;

                        case Region _region:
                            // TODO: region editor
                            break;

                        case EncounterTable _encounterTable:
                            HostTabControl.FindOrOpen(_s => _s.EncounterTable == _encounterTable,
                                () => new EncounterTableTab(_encounterTable, HostTabControl));
                            break;

                        case SitePathGraph _sitePathGraph:
                            // TODO: site-path-graph editor
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        #region new Non-Player Service
        private void miNewCraftsman_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Craftsman", @"craftsman");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new Craftsman(new Description(_name)));
            }
        }

        private void miNewLodging_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Lodging", @"lodging");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new Lodging(new Description(_name)));
            }
        }

        private void miNewMagicItemCrafter_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Magic Item Crafter", @"magicitemcrafter");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new MagicItemCrafter(new Description(_name)));
            }
        }

        private void miNewMerchant_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Merchant", @"merchant");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new Merchant(new Description(_name)));
            }
        }

        private void miNewOracle_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Oracle", @"oracle");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new Oracle(new Description(_name)));
            }
        }

        private void miNewRecuperation_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Recuperation", @"recuperation");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new Recuperation(new Description(_name)));
            }
        }

        private void miNewSage_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Sage", @"sage");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new Sage(new Description(_name)));
            }
        }

        private void miNewSpellCaster_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Spell-Caster", @"spellcaster");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new SpellCasterService(new Description(_name)));
            }
        }

        private void miNewWorkshop_Click(object sender, RoutedEventArgs e)
        {
            var _name = GetNewNamedComponent(@"New Workshop", @"workshop");
            if (PartsFolder.Parent is ModuleResources _resources)
            {
                _resources.AddPart(new WorkshopService(new Description(_name)));
            }
        }
        #endregion
    }
}
