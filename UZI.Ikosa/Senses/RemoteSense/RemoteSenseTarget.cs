using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class RemoteSenseTarget : GroupMemberAdjunct
    {
        public RemoteSenseTarget(object source, RemoteSenseGroup group)
            : base(source, group)
        {
        }

        public RemoteSenseGroup RemoteSenseGroup => Group as RemoteSenseGroup;
        public Creature Creature => RemoteSenseGroup?.Master?.Creature;
        public ISensorHost SensorHost => Anchor as ISensorHost;

        public override object Clone()
            => new RemoteSenseTarget(Source, RemoteSenseGroup);
    }
}
