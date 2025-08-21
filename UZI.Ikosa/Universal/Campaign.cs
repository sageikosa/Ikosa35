using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml;
using System.Reflection;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.TypeListers;
using System.Windows.Markup;
using Uzi.Ikosa.Magic;
using System.IO;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Advancement.NPCClasses;

namespace Uzi.Ikosa.Universal
{
    /// <summary>&quot;Manifest&quot; for the campaign within an AppDomain</summary>
    public class Campaign
    {
        // -=-=-=-=-= Availablity Listers =-=-=-=-=-
        // Cache-Load lists on construction
        public Dictionary<string, Dictionary<string, TypeListItem>> FeatLists { get; private set; }
        public Dictionary<string, Dictionary<string, TypeListItem>> ClassLists { get; private set; }
        public Dictionary<string, Dictionary<string, TypeListItem>> SpeciesLists { get; private set; }
        public Dictionary<string, Dictionary<string, TypeListItem>> TemplateLists { get; private set; }

        /// <summary>Spell lists keyed by type name</summary>
        public Dictionary<string, ClassSpellList> SpellLists { get; private set; }

        public SortedDictionary<string, ItemTypeListItem> SimpleWeapons { get; private set; }
        public SortedDictionary<string, ItemTypeListItem> MartialWeapons { get; private set; }
        public SortedDictionary<string, ItemTypeListItem> ExoticWeapons { get; private set; }
        public SortedDictionary<string, ItemTypeListItem> AmmunitionTypes { get; private set; }

        public SortedDictionary<string, ItemTypeListItem> ArmorTypes { get; private set; }
        public SortedDictionary<string, ItemTypeListItem> ShieldTypes { get; private set; }

        public SortedDictionary<string, TypeListItem> WeaponAbilities { get; private set; } // TODO: weapon abilities list
        public SortedDictionary<string, TypeListItem> ArmorAbilities { get; private set; }  // TODO: armor abilities list
        public SortedDictionary<string, TypeListItem> ShieldAbilities { get; private set; } // TODO: shield abilities list

        public Dictionary<string, DevotionalDefinition> Devotions { get; private set; }
        public Dictionary<string, TypeListItem> Languages { get; private set; }

        // TODO: item types (sub-lists? [eg, wonderous items, rings, rods, staves, special weapons])
        // TODO: sub skills

        public TypeListItem GetClassTypeListItem(string category, string fullName)
        {
            if (ClassLists.ContainsKey(category))
            {
                var _dict = ClassLists[category];
                if (_dict.ContainsKey(fullName))
                {
                    return _dict[fullName];
                }
            }
            return null;
        }

        #region Construction and Initialization
        public Campaign()
        {
            Initialize();

            // get feats
            var _list = new Dictionary<string, TypeListItem>();
            ProcessAssembly(Assembly.GetAssembly(typeof(FeatBase)), _list, typeof(FeatBase));
            FeatLists.Add(@"General", _list);

            // get classes
            _list = [];
            ProcessAssembly(Assembly.GetAssembly(typeof(AdvancementClass)), _list, typeof(AdvancementClass));
            ClassLists.Add(@"All", _list);
            _list = [];
            ProcessAssembly(Assembly.GetAssembly(typeof(CharacterClass)), _list, typeof(CharacterClass));
            ClassLists.Add(@"Character", _list);

            // get species
            _list = [];
            ProcessAssembly(Assembly.GetAssembly(typeof(Species)), _list, typeof(Species), (type) =>
            {
                return type.GetConstructors().Where(_t => !_t.GetParameters().Any() && _t.IsPublic).Any();
            });
            SpeciesLists.Add(@"All", _list);

            // get templates
            _list = [];
            //ProcessAssembly(Assembly.GetAssembly(typeof(TemplateBase)), _list, typeof(TemplateBase));
            TemplateLists.Add(@"All", _list);

            // languages
            ProcessAssembly(Assembly.GetAssembly(typeof(Language)), Languages, typeof(Language));

            // weapons
            foreach (var _type in Assembly.GetAssembly(typeof(WeaponBase)).GetTypes())
            {
                ProcessWeaponItem(_type);
            }

            // ammunition
            foreach (var _type in Assembly.GetAssembly(typeof(AmmunitionBase)).GetTypes())
            {
                ProcessAmmoItem(_type);
            }

            // armor
            foreach (var _type in Assembly.GetAssembly(typeof(ArmorBase)).GetTypes())
            {
                if (_type.IsSubclassOf(typeof(ArmorBase))
                    && !_type.IsAbstract && _type.IsPublic)
                {
                    var _info = ItemBase.GetInfo(_type);
                    if (!ArmorTypes.ContainsKey(_info.Name))
                    {
                        ArmorTypes.Add(_info.Name, new ItemTypeListItem(_type, _info));
                    }
                }
            }

            // shields
            foreach (var _type in Assembly.GetAssembly(typeof(ShieldBase)).GetTypes())
            {
                if (_type.IsSubclassOf(typeof(ShieldBase))
                    && !_type.IsAbstract && _type.IsPublic)
                {
                    var _info = ItemBase.GetInfo(_type);
                    if (!ShieldTypes.ContainsKey(_info.Name))
                    {
                        ShieldTypes.Add(_info.Name, new ItemTypeListItem(_type, _info));
                    }
                }
            }

            // devotions
            LoadDefaultDevotions();

            // spell lists
            SpellLists.Add(typeof(Cleric).FullName, DefaultSpellListInitializer.ClericSpells());
            SpellLists.Add(typeof(Sorcerer).FullName, DefaultSpellListInitializer.SorcererSpells());
            SpellLists.Add(typeof(Wizard).FullName, DefaultSpellListInitializer.WizardSpells());
            SpellLists.Add(typeof(Adept).FullName, DefaultSpellListInitializer.AdeptSpells());
            SpellLists.Add(typeof(ChaosInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(ChaosInfluence)));
            SpellLists.Add(typeof(DivinationInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(DivinationInfluence)));
            SpellLists.Add(typeof(EvilInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(EvilInfluence)));
            SpellLists.Add(typeof(FireInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(FireInfluence)));
            SpellLists.Add(typeof(GoodInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(GoodInfluence)));
            SpellLists.Add(typeof(HealingInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(HealingInfluence)));
            SpellLists.Add(typeof(LawInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(LawInfluence)));
            SpellLists.Add(typeof(ProtectionInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(ProtectionInfluence)));
            SpellLists.Add(typeof(StrengthInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(StrengthInfluence)));
            SpellLists.Add(typeof(WarInfluence).FullName, DefaultSpellListInitializer.Influence(typeof(WarInfluence)));
            // TODO: influence lists

            // TODO: crafts
            // TODO: professions
            // TODO: info keys...
        }

        public Campaign(string fileName, string pathBase)
        {
            Initialize();

            BaseUri = new Uri(pathBase);

            // Xml Setup
            var _reader = new XmlTextReader(fileName)
            {
                Normalization = true
            };
            var _xml = new XmlDocument
            {
                PreserveWhitespace = false
            };
            _xml.Load(_reader);

            LoadFromXml(_xml);
        }

        private void Initialize()
        {
            FeatLists = [];
            ClassLists = [];
            SpeciesLists = [];
            TemplateLists = [];
            SpellLists = [];
            SimpleWeapons = [];
            MartialWeapons = [];
            ExoticWeapons = [];
            AmmunitionTypes = [];
            ArmorTypes = [];
            ShieldTypes = [];
            Devotions = [];
            Languages = [];
        }
        #endregion

        public Uri BaseUri { get; private set; }

        private Uri _GlobalBaseUri = null;
        /// <summary>This should be set to point to the global system location for model files</summary>
        public Uri GlobalBaseUri
        {
            get { return _GlobalBaseUri; }
            set { _GlobalBaseUri = value; }
        }

        #region private Assembly GetAssembly(string assemblyName)
        private Assembly GetAssembly(string assemblyName)
        {
            foreach (Assembly _asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (_asm.FullName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    return _asm;
                }
            }
            return Assembly.Load(assemblyName);
        }
        #endregion

        #region private void ProcessAssembly(Assembly assembly, Dictionary<string, TypeListItem> targetList, Type compareType)
        private void ProcessAssembly(Assembly assembly, Dictionary<string, TypeListItem> targetList, Type compareType)
        {
            foreach (Type _probeType in assembly.GetTypes())
            {
                if (_probeType.IsSubclassOf(compareType)
                    && !_probeType.IsAbstract && _probeType.IsPublic)
                {
                    if (!targetList.ContainsKey(_probeType.FullName))
                    {
                        targetList.Add(_probeType.FullName, new TypeListItem(_probeType, _probeType.Name));
                    }
                }
            }
        }
        #endregion

        #region private void ProcessAssembly(Assembly assembly, Dictionary<string, TypeListItem> targetList, Type compareType)
        private void ProcessAssembly(Assembly assembly, Dictionary<string, TypeListItem> targetList, Type compareType, Func<Type, bool> validator)
        {
            foreach (var _probeType in assembly.GetTypes())
            {
                if (_probeType.IsSubclassOf(compareType)
                    && !_probeType.IsAbstract && _probeType.IsPublic)
                {
                    if (!targetList.ContainsKey(_probeType.FullName) && validator(_probeType))
                    {
                        targetList.Add(_probeType.FullName, new TypeListItem(_probeType, _probeType.Name));
                    }
                }
            }
        }
        #endregion

        #region private void ProcessListContainer(Dictionary<string, Dictionary<string, TypeListItem>> masterDictionary, XmlNode container, Type compareType)
        private void ProcessListContainer(Dictionary<string, Dictionary<string, TypeListItem>> masterDictionary,
            XmlNode container, Type compareType)
        {
            var _listName = container.Attributes[@"name"].Value;
            Dictionary<string, TypeListItem> _loading;
            if (!masterDictionary.ContainsKey(_listName))
            {
                _loading = [];
                masterDictionary.Add(_listName, _loading);
            }
            else
            {
                _loading = masterDictionary[_listName];
            }
            foreach (XmlNode _node in container.ChildNodes)
            {
                var _class = _node.Attributes[@"class"].Value;
                var _assembly = _node.Attributes[@"assembly"].Value;
                var _description = _node.Attributes[@"description"].Value;
                if (_class.Equals(@"*"))
                {
                    // examine entire assembly
                    Assembly _asm = GetAssembly(_assembly);
                    ProcessAssembly(_asm, _loading, compareType);
                }
                else
                {
                    Type _newType = GetAssembly(_assembly).GetType(_class);
                    if (_newType.IsSubclassOf(compareType) && !_newType.IsAbstract && _newType.IsPublic)
                    {
                        if (!_loading.ContainsKey(_class))
                        {
                            _loading.Add(_class, new TypeListItem(_newType, _description));
                        }
                    }
                }
            }
        }
        #endregion

        #region private void ProcessWeaponItem(Type type)
        private void ProcessWeaponItem(Type type)
        {
            if (type.IsSubclassOf(typeof(WeaponBase))
                && !type.IsAbstract && type.IsPublic)
            {
                ItemInfoAttribute _info = ItemBase.GetInfo(type);
                if (typeof(IExoticWeapon).IsAssignableFrom(type))
                {
                    // exotic
                    if (!ExoticWeapons.ContainsKey(_info.Name))
                    {
                        ExoticWeapons.Add(_info.Name, new ItemTypeListItem(type, _info));
                    }
                }
                else if (typeof(IMartialWeapon).IsAssignableFrom(type))
                {
                    // martial
                    if (!MartialWeapons.ContainsKey(_info.Name))
                    {
                        MartialWeapons.Add(_info.Name, new ItemTypeListItem(type, _info));
                    }
                }
                else
                {
                    // simple
                    if (!SimpleWeapons.ContainsKey(_info.Name))
                    {
                        SimpleWeapons.Add(_info.Name, new ItemTypeListItem(type, _info));
                    }
                }
            }
        }
        #endregion

        #region private void ProcessAmmoItem(Type type)
        private void ProcessAmmoItem(Type type)
        {
            if (type.IsSubclassOf(typeof(AmmunitionBase)) && !type.IsAbstract && type.IsPublic)
            {
                ItemInfoAttribute _info = ItemBase.GetInfo(type);
                AmmunitionTypes.Add(_info.Name, new ItemTypeListItem(type, _info));
            }
        }
        #endregion

        #region private void ProcessSpellListContainer(XmlNode container)
        private void ProcessSpellListContainer(XmlNode container)
        {
            // ensure class spell list in dictionary
            var _casterClass = container.Attributes[@"class"].Value;
            ClassSpellList _classSpells = null;
            if (!SpellLists.ContainsKey(_casterClass))
            {
                _classSpells = [];
                SpellLists.Add(_casterClass, _classSpells);
            }
            else
            {
                _classSpells = SpellLists[_casterClass];
            }

            // ensure class spell level in spell list
            var _level = Convert.ToInt32(container.Attributes[@"level"].Value);
            ClassSpellLevel _csl = null;
            if (!_classSpells.ContainsKey(_level))
            {
                _csl = new ClassSpellLevel(_level);
                _classSpells.Add(_level, _csl);
            }
            else
            {
                _csl = _classSpells[_level];
            }

            // instantiate all SpellDefs, add to list
            foreach (XmlNode _node in container.ChildNodes)
            {
                // construct SpellDef and add to spell list
                var _spellClass = _node.Attributes[@"class"].Value;
                var _assembly = _node.Attributes[@"assembly"].Value;
                Type _sdType = GetAssembly(_assembly).GetType(_spellClass);
                if (_sdType.IsSubclassOf(typeof(SpellDef)) && !_sdType.IsAbstract && _sdType.IsPublic)
                {
                    var _spellDef = (SpellDef)Activator.CreateInstance(_sdType);
                    _csl.Add(new ClassSpell(_level, _spellDef));
                }
            }
        }
        #endregion

        #region private void LoadFromXml(XmlDocument xml)
        private void LoadFromXml(XmlDocument xml)
        {
            // XML Setup
            var _xmlns = new XmlNamespaceManager(xml.NameTable);
            _xmlns.AddNamespace(@"uzi", xml.DocumentElement.NamespaceURI);
            var _campNode = xml.SelectSingleNode(@"uzi:Campaign", _xmlns);

            // process each child node
            foreach (var _container in _campNode.ChildNodes.OfType<XmlNode>())
            {
                switch (_container.Name)
                {
                    case @"Merge":
                        // TODO: load another document (recursively)
                        // TODO: use ~ for Ikosa.Root
                        break;

                    case @"Feats":
                        ProcessListContainer(FeatLists, _container, typeof(FeatBase));
                        break;

                    case @"Classes":
                        ProcessListContainer(ClassLists, _container, typeof(CharacterClass));
                        break;

                    case @"SpeciesList":
                        ProcessListContainer(SpeciesLists, _container, typeof(Species));
                        break;

                    case @"Templates":
                        //ProcessListContainer(TemplateLists, _container, typeof(TemplateBase));
                        break;

                    case @"WeaponTypes":
                        #region Weapon Types
                        foreach (XmlNode _node in _container.ChildNodes)
                        {
                            var _class = _node.Attributes["class"].Value;
                            var _assembly = _node.Attributes["assembly"].Value;
                            var _description = _node.Attributes["description"].Value;
                            if (_class.Equals("*"))
                            {
                                // examine entire assembly
                                Assembly _asm = GetAssembly(_assembly);
                                foreach (Type _probeType in _asm.GetTypes())
                                {
                                    ProcessWeaponItem(_probeType);
                                }
                            }
                            else
                            {
                                Type _newType = GetAssembly(_assembly).GetType(_class);
                                ProcessWeaponItem(_newType);
                            }
                        }
                        #endregion
                        break;

                    case @"ItemTypes":
                        break;

                    case @"SpellList":
                        ProcessSpellListContainer(_container);
                        break;

                        // TODO: devotions and influences
                        // TODO: crafts
                        // TODO: professions

                        // TODO: region
                        // TODO: resources
                        // TODO: info keys
                        // TODO: etc...
                }
            }
        }
        #endregion

        private Collection<SpellDef> LoadSpellDefs(string fileName)
        {
            var _context = new ParserContext
            {
                BaseUri = BaseUri
            };

            var _file = new FileStream(string.Concat(BaseUri.OriginalString, fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
            var _coll = (Collection<SpellDef>)XamlReader.Load(_file, _context);
            _file.Close();
            return _coll;
        }

        #region public static Campaign SystemCampaign { get; set; }
        private static Campaign _SystemCampaign = null;
        public static Campaign SystemCampaign
        {
            get => _SystemCampaign ??= new Campaign();
            set => _SystemCampaign = value;
        }
        #endregion

        /// <summary>Current tracked time of the campaign world</summary>
        public double Current_Time { get; set; }

        // TODO: time tick across all settings and any objects "outside" a setting...

        #region public Uri ResolveBaseUri(string fileName, Uri localUri)
        /// <summary>
        /// Gets the BaseURI for resolving XAML
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="localUri"></param>
        /// <returns></returns>
        public Uri ResolveBaseUri(string fileName, Uri localUri)
        {
            if (!fileName.Contains(@":"))
            {
                switch (fileName[0])
                {
                    case '~':
                        return GlobalBaseUri;
                    case '#':
                        return BaseUri;
                    default:
                        return localUri;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region public string ResolveFileName(string fileName, Uri localUri)
        /// <summary>
        /// Expands specially prefixed filenames 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="localUri"></param>
        /// <returns></returns>
        public string ResolveFileName(string fileName, Uri localUri)
        {
            if (!fileName.Contains(@":"))
            {
                switch (fileName[0])
                {
                    case '~':
                        return string.Concat(GlobalBaseUri.OriginalString, fileName.Substring(1));
                    case '#':
                        return string.Concat(BaseUri.OriginalString, fileName.Substring(1));
                    default:
                        return string.Concat(localUri.OriginalString, fileName);
                }
            }
            else
            {
                return fileName;
            }
        }
        #endregion

        // -=-=-=-=-= Universal Resources =-=-=-=-=-
        // TODO: world region map
        // TODO: graphical resources
        // TODO: referenced assemblies

        #region setup default devotions
        private DevotionalDefinition CreateDevotion<WpnType>(Alignment align, params TypeListItem[] influences)
            where WpnType : WeaponBase
        {
            var _infColl = new Collection<TypeListItem>();
            foreach (var _inf in influences)
            {
                _infColl.Add(_inf);
            }

            return new DevotionalDefinition
            {
                Alignment = align,
                Influences = _infColl,
                WeaponType = typeof(WpnType)
            };
        }

        private void LoadDefaultDevotions()
        {
            Devotions.Add(@"All Magic", CreateDevotion<Quarterstaff>(Alignment.TrueNeutral,
                new TypeListItem(typeof(DivinationInfluence), @"Divination"),
                new TypeListItem(typeof(WarInfluence), @"War")
                // MAGIC
                ));
            Devotions.Add(@"Elven", CreateDevotion<Longsword>(Alignment.ChaoticGood,
                new TypeListItem(typeof(ChaosInfluence), @"Chaos"),
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                new TypeListItem(typeof(ProtectionInfluence), @"Protection"),
                new TypeListItem(typeof(WarInfluence), @"War")
                ));
            Devotions.Add(@"Woodlands", CreateDevotion<Longbow>(Alignment.NeutralGood,
                // ANIMAL
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                new TypeListItem(typeof(PlantInfluence), @"Plant"),
                new TypeListItem(typeof(SunInfluence), @"Sun")
                ));
            Devotions.Add(@"Slaughter", CreateDevotion<MorningStar>(Alignment.ChaoticEvil,
                new TypeListItem(typeof(ChaosInfluence), @"Chaos"),
                new TypeListItem(typeof(EvilInfluence), @"Evil"),
                new TypeListItem(typeof(TrickeryInfluence), @"Trickery"),
                new TypeListItem(typeof(WarInfluence), @"War")
                ));
            Devotions.Add(@"Wandering", CreateDevotion<Quarterstaff>(Alignment.TrueNeutral,
                // LUCK
                new TypeListItem(typeof(ProtectionInfluence), @"Protection")
                // TRAVEL
                ));
            Devotions.Add(@"Gnomes", CreateDevotion<BattleAxe>(Alignment.NeutralGood,
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                new TypeListItem(typeof(ProtectionInfluence), @"Protection"),
                new TypeListItem(typeof(TrickeryInfluence), @"Trickery")
                ));
            Devotions.Add(@"Orcs", CreateDevotion<Spear>(Alignment.ChaoticEvil,
                new TypeListItem(typeof(ChaosInfluence), @"Chaos"),
                new TypeListItem(typeof(EvilInfluence), @"Evil"),
                new TypeListItem(typeof(StrengthInfluence), @"Strength"),
                new TypeListItem(typeof(WarInfluence), @"War")
                ));
            Devotions.Add(@"Valor", CreateDevotion<Longsword>(Alignment.LawfulGood,
                new TypeListItem(typeof(ChaosInfluence), @"Chaos"),
                new TypeListItem(typeof(LawInfluence), @"Law"),
                new TypeListItem(typeof(WarInfluence), @"War")
                ));
            Devotions.Add(@"Tyranny", CreateDevotion<Flail>(Alignment.NeutralEvil,
                // DESTRUCTION
                new TypeListItem(typeof(EvilInfluence), @"Evil"),
                new TypeListItem(typeof(LawInfluence), @"Law"),
                new TypeListItem(typeof(WarInfluence), @"War")
                ));
            Devotions.Add(@"Heroism", CreateDevotion<Greatsword>(Alignment.ChaoticGood,
                new TypeListItem(typeof(ChaosInfluence), @"Chaos"),
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                // LUCK
                new TypeListItem(typeof(StrengthInfluence), @"Strength")
                ));
            Devotions.Add(@"Dwarves", CreateDevotion<Warhammer>(Alignment.LawfulGood,
                new TypeListItem(typeof(EarthInfluence), @"Earth"),
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                new TypeListItem(typeof(LawInfluence), @"Law"),
                new TypeListItem(typeof(ProtectionInfluence), @"Protection")
                ));
            Devotions.Add(@"Dead", CreateDevotion<Scythe>(Alignment.NeutralEvil,
                // DEATH
                new TypeListItem(typeof(EvilInfluence), @"Evil"),
                new TypeListItem(typeof(TrickeryInfluence), @"Trickery")
                ));
            Devotions.Add(@"Nature", CreateDevotion<Quarterstaff>(Alignment.TrueNeutral,
                new TypeListItem(typeof(AirInfluence), @"Air"),
                // ANIMAL
                new TypeListItem(typeof(EarthInfluence), @"Earth"),
                new TypeListItem(typeof(FireInfluence), @"Fire"),
                new TypeListItem(typeof(PlantInfluence), @"Plant"),
                new TypeListItem(typeof(WaterInfluence), @"Water")
                ));
            Devotions.Add(@"Rogues", CreateDevotion<Rapier>(Alignment.ChaoticNeutral,
                new TypeListItem(typeof(ChaosInfluence), @"Chaos"),
                // LUCK
                new TypeListItem(typeof(TrickeryInfluence), @"Trickery")
                ));
            Devotions.Add(@"Sun", CreateDevotion<LightMace>(Alignment.NeutralGood,
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                new TypeListItem(typeof(HealingInfluence), @"Healing"),
                new TypeListItem(typeof(StrengthInfluence), @"Strength"),
                new TypeListItem(typeof(SunInfluence), @"Sun")
                ));
            Devotions.Add(@"Justice", CreateDevotion<HeavyMace>(Alignment.LawfulNeutral,
                // DESTRUCTION
                new TypeListItem(typeof(LawInfluence), @"Law"),
                new TypeListItem(typeof(ProtectionInfluence), @"Protection"),
                new TypeListItem(typeof(StrengthInfluence), @"Strength")
                ));
            Devotions.Add(@"Hidden Magic", CreateDevotion<Dagger>(Alignment.NeutralEvil,
                new TypeListItem(typeof(EvilInfluence), @"Evil"),
                new TypeListItem(typeof(DivinationInfluence), @"Divination")
                // MAGIC
                ));
            Devotions.Add(@"Death Magic", CreateDevotion<Dagger>(Alignment.LawfulNeutral,
                // DEATH
                new TypeListItem(typeof(LawInfluence), @"Law")
                // MAGIC
                ));
            Devotions.Add(@"Halflings", CreateDevotion<ShortSword>(Alignment.LawfulGood,
                new TypeListItem(typeof(GoodInfluence), @"Good"),
                new TypeListItem(typeof(LawInfluence), @"Law"),
                new TypeListItem(typeof(ProtectionInfluence), @"Protection")
                ));
        }
        #endregion
    }
}
