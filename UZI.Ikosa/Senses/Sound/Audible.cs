using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class Audible : IAudible
    {
        public Audible(Guid soundGroupID, Guid sourceID, string name, params (int exceed, string descript)[] descriptions)
        {
            _SGID = soundGroupID;
            _SID = sourceID;
            _Name = name;
            _Descriptions = descriptions.ToList();
        }

        #region state
        private Guid _SGID;
        private Guid _SID;
        private string _Name;
        private List<(int exceed, string descript)> _Descriptions;
        #endregion

        public Guid SoundGroupID => _SGID;
        public Guid SourceID => _SID;
        public string Name => _Name;

        public SoundInfo GetSoundInfo(ISensorHost sensors, SoundAwareness awareness)
        {
            // strength/proximity dial-in
            var _exceed = awareness.CheckExceed;

            // clarity of description boosted by being aware of object
            var _clarity = _exceed + (((sensors?.Awarenesses?.GetAwarenessLevel(SourceID) ?? AwarenessLevel.None) >= AwarenessLevel.Aware) ? 10 : 0);

            // strength description
            var _presence = Math.Max(awareness.SourceRange, 1);
            var _strength = (awareness.Magnitude ?? 0) / _presence;
            var _strDescript = SoundInfo.GetStrengthDescription(_presence, _strength, _exceed);

            // last (highest) description exceeded
            var _descript = _Descriptions.OrderBy(_d => _d.exceed).LastOrDefault(_d => _clarity >= _d.exceed).descript ?? @"sound";
            return new SoundInfo
            {
                Strength = _strength,
                Description = $@"{_strDescript}{_descript}"
            };
        }

        public void LostSoundInfo(ISensorHost sensors)
        {
        }
    }
}
