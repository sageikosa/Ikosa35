using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    [KnownType(typeof(Description))]
    public class LoadableProjectileWeaponInfo : ProjectileWeaponInfo
    {
        #region construction
        public LoadableProjectileWeaponInfo()
            : base()
        {
        }

        protected LoadableProjectileWeaponInfo(LoadableProjectileWeaponInfo copySource)
            : base(copySource)
        {
            LoadedAmmunition = copySource.LoadedAmmunition.Select(_a => _a.Clone() as Info).ToList();
        }
        #endregion

        [DataMember]
        public List<Info> LoadedAmmunition { get; set; }

        public override object Clone()
        {
            return new LoadableProjectileWeaponInfo(this);
        }
    }
}
