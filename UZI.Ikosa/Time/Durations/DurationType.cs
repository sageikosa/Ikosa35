using System;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Duration types for spell durations
    /// </summary>
    [Serializable]
    public enum DurationType
    {
        /// <summary>See text</summary>
        Custom,
        /// <summary>Spell effect is instantaneous</summary>
        Instantaneous,
        /// <summary>Spell lasts as long as concentration is maintained</summary>
        Concentration,
        /// <summary>Spell lasts as long as concentration is maintained plus a span after concentration ends</summary>
        ConcentrationPlusSpan,
        /// <summary>Spell lasts as for a calculated duration</summary>
        Span,
        /// <summary>Spell effect is permanent</summary>
        Permanent
    }
}
