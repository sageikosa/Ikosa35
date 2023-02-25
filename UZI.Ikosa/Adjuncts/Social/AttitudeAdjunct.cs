using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    // TODO: attitude adjuster UI for game master...(and possibly for players?)

    /// <summary>
    /// Attitude towards a particular creature
    /// </summary>
    [Serializable]
    public class AttitudeAdjunct : PredispositionBase
    {
        #region ctor()
        public AttitudeAdjunct(object source, string targetInfo, Guid targetID, Attitude attitude)
            : base(source)
        {
            _Attitude = attitude;
            _Info = targetInfo;
            _TargetID = targetID;
        }
        #endregion

        #region private data
        private Guid _TargetID;
        private string _Info;
        private Attitude _Attitude;
        #endregion

        public Guid TargetID => _TargetID;
        public string TargetInfo => _Info;
        public Attitude Attitude => _Attitude;

        public override string Description
            => $@"Act {_Attitude.ToString()} towards {_Info} (source: {Source.SourceName()})";

        public override object Clone()
            => new AttitudeAdjunct(Source, TargetInfo, TargetID, this.Attitude);
    }

    public static class AttitudeAttachment
    {
        public static Attitude AttitudeTowards(this IAdjunctable self, Guid targetID)
            => (from _adj in self.Adjuncts
                let _att = _adj as AttitudeAdjunct
                where _att != null && _att.TargetID.Equals(targetID)
                orderby _att.Attitude descending
                select _att.Attitude).FirstOrDefault();

        /// <summary>
        /// Explicitly has a bad attitude (adjunct) or is a known un-friendly creature;
        /// otherwise must not be explicitly friendly nor a same team, and must be treating unknown as unfriendly.
        /// </summary>
        public static bool IsUnfriendly(this Creature self, Guid targetID)
        {
            // always friendly to oneself
            if (self.ID == targetID)
                return false;

            // explicit attitude
            if (self.AttitudeTowards(targetID) < Attitude.Unbiased)
                return true;

            // known unfriendly
            if (self.UnfriendlyCreatures.Contains(targetID))
                return true;

            // known friendly are not unfriendly
            if (self.FriendlyCreatures.Contains(targetID))
                return false;

            // same team are not unfriendly
            if (self.GetSameTeams(targetID).Any())
                return false;

            // unknown treated as unfriendly?
            return self.TreatUnknownAsUnFriendly;
        }

        /// <summary>
        /// Explicitly has a good attitude (adjunct), is on a same team, or is a known friendly creature.
        /// </summary>
        public static bool IsFriendly(this Creature self, Guid targetID)
        {
            // to one's own self be true
            if (self.ID == targetID)
            {
                return true;
            }

            // good general attitude
            if (self.AttitudeTowards(targetID) > Attitude.Unbiased)
            {
                return true;
            }

            // same team
            if (self.GetSameTeams(targetID).Any())
            {
                return true;
            }

            // known friendly
            if (self.FriendlyCreatures.Contains(targetID))
                return true;

            return false;
        }
    }
}
