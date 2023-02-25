using System;
using Uzi.Core;

namespace Uzi.Ikosa.Creatures.Types
{
    /// <summary>
    /// Creature type might be controlling a monster advancement class.  
    /// Some humanoids do not use creature type this way.  
    /// In which case, CanAdd and OnAdd must be overridden to avoid using Class/PowerDie 
    /// management functions.
    /// </summary>
    [Serializable]
    public abstract class CreatureType : ICreatureBind, ILinkOwner<LinkableDock<CreatureType>>
    {
        protected CreatureType()
            : base()
        {
            _Dock = new BiDiPtr<CreatureTypeDock, CreatureType>(this);
        }

        public virtual string Name { get { return this.GetType().Name; } }

        protected BiDiPtr<CreatureTypeDock, CreatureType> _Dock;
        public Creature Creature { get { return _Dock.LinkDock.Creature; } }

        /// <summary>Indicates that the creature is considered living</summary>
        public virtual bool IsLiving { get { return true; } }

        /// <summary>Override for custom binding side-effects</summary>
        protected virtual void OnBind()
        {
        }

        /// <summary>Override for custom unbinding side-effects</summary>
        protected virtual void OnUnBind()
        {
        }

        #region ICreatureBind Members
        public bool BindTo(Creature creature)
        {
            if (!_Dock.WillAbortChange(creature.CreatureTypeDock))
            {
                this._Dock.LinkDock = creature.CreatureTypeDock;
                return true;
            }
            return false;
        }

        public void UnbindFromCreature()
        {
            if (!_Dock.WillAbortChange(null))
            {
                _Dock.LinkDock = null;
            }
        }
        #endregion

        #region ILinkOwner<LinkableDock<CreatureType>> Members

        public void LinkDropped(LinkableDock<CreatureType> changer)
        {
            var _critterType = changer as CreatureTypeDock;
            if (_critterType != null)
            {
                if ((_Dock.LinkDock == _critterType) && (_critterType.CreatureType != this))
                {
                    OnUnBind();
                    _Dock.LinkDock = null;
                }
            }
        }

        public void LinkAdded(LinkableDock<CreatureType> changer)
        {
            OnBind();
        }

        #endregion
    }
}
