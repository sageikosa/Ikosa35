using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa
{
    [Serializable]
    public abstract class CreatureSubType : ISourcedObject
    {
        protected CreatureSubType(object source) { _Source = source; }

        public abstract string Name { get; }

        #region ISourcedObject Members
        private object _Source;
        public object Source
        {
            get { return _Source; }
        }
        #endregion

        public abstract CreatureSubType Clone(object source);
    }

    [Serializable]
    public abstract class CreatureAlignmentSubType : CreatureSubType
    {
        protected CreatureAlignmentSubType(object source)
            : base(source)
        {
        }
    }

    [Serializable]
    public abstract class CreatureElementalSubType : CreatureSubType
    {
        protected CreatureElementalSubType(object source)
            : base(source)
        {
        }
    }

    [Serializable]
    public abstract class BaseCreatureSpeciesSubType : CreatureSubType
    {
        protected BaseCreatureSpeciesSubType(object source)
            : base(source)
        {
        }
    }

    [Serializable]
    public class CreatureSpeciesSubType<CommonSpecies> : BaseCreatureSpeciesSubType
        where CommonSpecies : Species
    {
        public CreatureSpeciesSubType(object source, string name)
            : base(source)
        {
            _Name = name;
        }

        private string _Name;
        public override string Name { get { return _Name; } }

        public override CreatureSubType Clone(object source)
        {
            return new CreatureSpeciesSubType<CommonSpecies>(source, _Name);
        }
    }
}
