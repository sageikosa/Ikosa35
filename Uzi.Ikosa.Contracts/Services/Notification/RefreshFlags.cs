using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    [Flags]
    public enum RefreshFlags
    {
        [EnumMember] None = 0x0,
        [EnumMember] Creature = 0x1,
        [EnumMember] Items = 0x2,
        [EnumMember] ItemsCreature = 0x3,
        [EnumMember] Awarenesses = 0x4,
        [EnumMember] AwarenessCreature = 0x5,
        [EnumMember] AwarenessItems = 0x6,
        [EnumMember] AwarenessItemsCreature = 0x7,
        [EnumMember] SensorHost = 0x8,
        [EnumMember] SensorHostCreature = 0x9,
        [EnumMember] SensorHostItems = 0xA,
        [EnumMember] SensorHostItemsCreature = 0xB,
        [EnumMember] SensorHostAwarenesses = 0xC,
        [EnumMember] SensorHostAwarenessCreature = 0xD,
        [EnumMember] SensorHostAwarenessItems = 0xE,
        [EnumMember] SensorHostAwarenessItemsCreature = 0xF,
        [EnumMember] Actions = 0x10,
        [EnumMember] ActionsCreature = 0x11,
        [EnumMember] ActionsItems = 0x12,
        [EnumMember] ActionsItemsCreature = 0x13,
        [EnumMember] ActionsAwarenesses = 0x14,
        [EnumMember] ActionsAwarenessCreature = 0x15,
        [EnumMember] ActionsAwarenessItems = 0x16,
        [EnumMember] ActionsAwarenessItemsCreature = 0x17,
        [EnumMember] ActionsSensorHost = 0x18,
        [EnumMember] ActionsSensorHostCreature = 0x19,
        [EnumMember] ActionsSensorHostItems = 0x1A,
        [EnumMember] ActionsSensorHostItemsCreature = 0x1B,
        [EnumMember] ActionsSensorHostAwarenesses = 0x1C,
        [EnumMember] ActionsSensorHostAwarenessCreature = 0x1D,
        [EnumMember] ActionsSensorHostAwarenessItems = 0x1E,
        [EnumMember] ActionsSensorHostAwarenessItemsCreature = 0x1F
    }
}
