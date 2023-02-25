using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Objects;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public class PresentableThingVM<CObject> : PresentableContext
        where CObject : class, ICoreObject
    {
        public CObject Thing { get; set; }

        public override ICoreObject CoreObject => Thing;

        public void Rebind()
        {
            // save
            var _t = Thing;

            // clear
            Thing = null;
            DoPropertyChanged(nameof(Thing));
            OnRebindClear();

            // reset
            Thing = _t;
            DoPropertyChanged(nameof(Thing));
            OnRebindReset();
        }

        protected virtual void OnRebindClear() { }
        protected virtual void OnRebindReset() { }
    }

    public static class PresentableThingVMHelpers
    {
        public static PresentableContext GetPresentableObjectVM<CObject>(this CObject coreObject, VisualResources visualResources, PresentableCreatureVM possessor)
            where CObject : class, ICoreObject
        {
            PresentableContext _addGeneral(PresentableContext context)
            {
                context.VisualResources = visualResources;
                context.Possessor = possessor;
                return context;
            }

            switch (coreObject)
            {
                case IFlatObjectSide _side:
                    return _addGeneral(new FlatObjectSideVM { Thing = _side });

                case Creature _c:
                    return _addGeneral(new PresentableCreatureVM { Thing = _c });

                case CloseableContainerObject _cco:
                    return _addGeneral(new PresentableCloseableContainerVM { Thing = _cco });

                case ContainerObject _co:
                    return _addGeneral(new PresentableContainerObjectVM { Thing = _co });

                case ContainerItemBase _cib:
                    return _addGeneral(new PresentableContainerItemVM { Thing = _cib });

                case SlottedContainerItemBase _scib:
                    return _addGeneral(new PresentableSlottedContainerItemVM { Thing = _scib });

                case KeyRing _kr:
                    return _addGeneral(new KeyRingVM { Thing = _kr });

                case KeyItem _ki:
                    return _addGeneral(new KeyItemVM { Thing = _ki });

                case Quiver _q:
                    return _addGeneral(new QuiverVM { Thing = _q });

                case BoltSash _bs:
                    return _addGeneral(new BoltSashVM { Thing = _bs });

                case SlingBag _sb:
                    return _addGeneral(new SlingBagVM { Thing = _sb });

                case ArrowBundle _ab:
                    return _addGeneral(new ArrowBundleVM { Thing = _ab });

                case CrossbowBoltBundle _cbb:
                    return _addGeneral(new CrossbowBoltBundleVM { Thing = _cbb });

                case SlingAmmoBundle _sab:
                    return _addGeneral(new SlingAmmoBundleVM { Thing = _sab });

                case PortalBase _portal:
                    return _addGeneral(new PresentablePortalVM { Thing = _portal });

                case MechanismMount _mm:
                    return _addGeneral(new PresentableMechanismMountVM { Thing = _mm });

                case DoubleMeleeWeaponBase _dmw:
                    return _addGeneral(new DoubleMeleeWeaponVM { Thing = _dmw });

                case MeleeWeaponBase _mw:
                    return _addGeneral(new MeleeWeaponVM { Thing = _mw });

                case ProjectileWeaponBase _pw:
                    return _addGeneral(new ProjectileWeaponVM { Thing = _pw });

                case NaturalWeapon _nw:
                    return _addGeneral(new NaturalWeaponVM { Thing = _nw });

                case ArmorBase _ab:
                    return _addGeneral(new ArmorVM { Thing = _ab });

                case ShieldBase _sb:
                    return _addGeneral(new ShieldVM { Thing = _sb });

                case Wand _wand:
                    return _addGeneral(new WandVM { Thing = _wand });

                case Potion _potion:
                    return _addGeneral(new PotionVM { Thing = _potion });

                case Scroll _scroll:
                    return _addGeneral(new ScrollVM { Thing = _scroll });

                case SpellBook _spellBook:
                    return _addGeneral(new SpellBookVM { Thing = _spellBook });

                case CoinSet _cs:
                    return _addGeneral(new CoinSetVM { Thing = _cs });

                case Gem _gem:
                    return _addGeneral(new GemVM { Thing = _gem });

                case ItemBase _item:
                    return _addGeneral(new ItemVM { Thing = _item });

                case Furnishing _furnish:
                    {
                        switch (_furnish)
                        {
                            case FlexibleFlatPanel _flex:
                                return new FlexibleFlatPanelVM(_flex, visualResources);
                            case HollowFurnishing _hollow:
                                return new HollowFurnishingVM(_hollow, visualResources);
                            case HollowFurnishingLid _lid:
                                return new HollowFurnishingLidVM(_lid, visualResources);
                        }
                        return new FurnishingVM(_furnish, visualResources);
                    }

                case Conveyance _convey:
                    return _addGeneral(new ConveyanceVM(_convey));

                default:
                    return coreObject == null 
                        ? null 
                        : _addGeneral(new PresentableThingVM<CObject> { Thing = coreObject });
            }
        }
    }
}
