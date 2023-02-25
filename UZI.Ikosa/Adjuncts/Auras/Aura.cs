using System;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Aura : IAura
    {
        public Aura(Guid id, AuraStrength strength)
        {
            _Strength = strength;
            _ID = id;
        }

        #region private data
        private AuraStrength _Strength;
        private Guid _ID;
        #endregion

        public Guid ID { get { return _ID; } }

        public AuraStrength AuraStrength { get { return _Strength; } }
    }
}
