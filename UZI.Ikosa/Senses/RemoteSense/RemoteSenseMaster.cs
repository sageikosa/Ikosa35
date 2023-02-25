using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class RemoteSenseMaster : GroupMasterAdjunct
    {
        public RemoteSenseMaster(object source, RemoteSenseGroup group)
            : base(source, group)
        {
        }

        public RemoteSenseGroup RemoteSenseGroup => Group as RemoteSenseGroup;
        public Creature Creature => Anchor as Creature;
        public ISensorHost SensorHost => RemoteSenseGroup?.Target.SensorHost;

        public override object Clone()
            => new RemoteSenseMaster(Source, RemoteSenseGroup);

        public static IEnumerable<ISensorHost> GetSensorHosts(IAdjunctable adjunctable)
        {
            foreach (var _master in adjunctable.Adjuncts.OfType<RemoteSenseMaster>())
            {
                yield return _master.RemoteSenseGroup.Target.SensorHost;
            }
            yield break;
        }
    }
}
