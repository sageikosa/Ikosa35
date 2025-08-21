using System;

namespace Uzi.Ikosa
{
    /// <summary>Base class for anything that can be bound to a creature</summary>
    [Serializable]
    public abstract class CreatureBindBase : ICreatureBound, ICreatureBind
    {
        protected CreatureBindBase()
        {
        }

        protected Creature _Creature = null;
        public Creature Creature => _Creature;

        public bool BindTo(Creature creature)
        {
            if (CanAdd(creature))
            {
                if (_Creature == null)
                {
                    _Creature = creature;
                    OnAdd();
                    return true;
                }
            }

            return false;
        }

        /// <summary>Override to conditionally deny adding to the creature</summary>
        public virtual bool CanAdd(Creature testCreature) => true;

        /// <summary>default implementation: no operation</summary>
        protected virtual void OnAdd() { }

        public void UnbindFromCreature()
        {
            if (CanRemove())
            {
                OnRemove();
                _Creature = null;
            }
        }

        /// <summary>Override to conditionally deny removal from the creature</summary>
        public virtual bool CanRemove() => true;

        protected virtual void OnRemove() { }
    }

    /// <summary>Interface for CoreObjects that are bound to a creature.</summary>
    public interface ICreatureBound
    {
        Creature Creature { get; }
    }

    /// <summary>Interface for CoreObjects that can bind to (and unbind from) a creature</summary>
    public interface ICreatureBind : ICreatureBound
    {
        bool BindTo(Creature creature);
        void UnbindFromCreature();
    }
}
