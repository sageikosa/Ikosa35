using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>
    /// Indicates the time needed to perform an action, or the length of an adjunct
    /// </summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum TimeType : byte
    {
        /// <summary>Free actions have no budgetary restrictions...</summary>
        [EnumMember]
        Free = 0,

        /// <summary>Opportunistic actions (typically attacks) can usually be made once per round; or more with special features.</summary>
        [EnumMember]
        Opportunistic,

        /// <summary>Twitch action made in response to a limited set of conditions, out of normal order.</summary>
        [EnumMember]
        Reactive,

        /// <summary>Free selection on your turn</summary>
        [EnumMember]
        FreeOnTurn,

        /// <summary>A single twitchy action can be made on one's turn</summary>
        [EnumMember]
        Twitch,

        /// <summary>
        /// A sub-action only can be used as part of another action, and consumes time like a free action.
        /// For example: strikes in a full attack sequence, and steps in a move action.
        /// </summary>
        [EnumMember]
        SubAction,

        /// <summary>Action involving little effort</summary>
        [EnumMember]
        Brief,

        /// <summary>Action involving effort</summary>
        [EnumMember]
        Regular,

        /// <summary>Action taking all out effort</summary>
        [EnumMember]
        Total,

        /// <summary>Budget is fully engaged until the span has completed</summary>
        [EnumMember]
        Span,

        /// <summary>Span that is for timeline use</summary>
        [EnumMember]
        TimelineScheduling,

        /// <summary>Consume time that has been gathered by a timeline savings action.</summary>
        [EnumMember]
        TimeSavings
    }
}
