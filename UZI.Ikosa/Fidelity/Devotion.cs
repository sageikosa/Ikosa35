using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Advancement;
using Uzi.Core;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Target of a creature's devotion.  Determines cleric's available influences.</summary>
    [Serializable]
    public class Devotion : Adjunct
    {
        public Devotion(string name)
            : base(typeof(Devotion))
        {
            _Name = name;
        }

        private string _Name;

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (Anchor != null)
            {
                foreach (var _dev in Anchor.Adjuncts.OfType<Devotion>().Where(_d => !_d.Name.Equals(Name)).ToList())
                {
                    _dev.Eject();
                }
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }

        public override bool IsProtected => true;

        /// <summary>Name must be unique per campaign</summary>
        public string Name => _Name;

        /// <summary>Certain classes cannot stray more than one step from the alignment</summary>
        public Alignment Alignment
            => Campaign.SystemCampaign.Devotions.ContainsKey(_Name)
            ? Campaign.SystemCampaign.Devotions[_Name].Alignment
            : Alignment.TrueNeutral;

        /// <summary>Devotions with war influences need to have information on the devotional weapon</summary>
        public Type WeaponType
            => Campaign.SystemCampaign.Devotions.ContainsKey(_Name)
            ? Campaign.SystemCampaign.Devotions[_Name].WeaponType
            : typeof(Club);

        #region public IEnumerable<Influence> Influences(IPrimaryInfluenceClass influenceClass)
        /// <summary>
        /// Governs available powers and spells for a cleric.  
        /// Pulled from Campaign.DevotionalInfluences for the Type
        /// </summary>
        public IEnumerable<Influence> Influences(IPrimaryInfluenceClass influenceClass)
        {
            if (Campaign.SystemCampaign.Devotions.ContainsKey(_Name))
            {
                foreach (var _inf in from _list in Campaign.SystemCampaign.Devotions[_Name].Influences
                                     select Activator.CreateInstance(_list.ListedType, this, influenceClass) as Influence)
                {
                    yield return _inf;
                }
            }

            yield break;
        }
        #endregion

        public override object Clone() => new Devotion(Name);
    }
}