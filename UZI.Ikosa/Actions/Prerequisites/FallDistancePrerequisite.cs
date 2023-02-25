using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Used to finalize a fall distance (tumble, slow fall, feather fall and contingencies may alter this)
    /// </summary>
    [Serializable]
    public class FallDistancePrerequisite : StepPrerequisite
    {
        /// <summary>
        /// Used to finalize a fall distance (tumble, slow fall, feather fall and contingencies may alter this)
        /// </summary>
        public FallDistancePrerequisite(object source, Interaction workSet, string key, string name, double distance)
            : base(source, workSet, key, name)
        {
            Distance = distance;
            Ready = false;
        }

        //public FallDistancePrerequisite(object source, ICore iCore, string key, string name, double distance)
        //    : base(source, iCore, key, name)
        //{
        //    Distance = distance;
        //    Ready = false;
        //}

        #region private data
        private double _Distance;
        private bool _Ready;
        #endregion

        public double Distance { get { return _Distance; } set { _Distance = value; } }

        public bool Ready { get { return _Ready; } set { _Ready = value; } }

        public override bool IsReady { get { return Ready; } }
        public override bool IsSerial { get { return true; } }

        /// <summary>IStep member</summary>
        public override bool FailsProcess { get { return false; } }

        public override CoreActor Fulfiller
            => Qualification?.Target as CoreActor;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            return null;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
        }
    }
}
