using Uzi.Core;

namespace Uzi.Ikosa.Deltas
{
    public abstract class DeltaType { }

    // NOTE: IArmor is "Armor"
    // NOTE: IShield is "Shield"

    [SourceInfo(@"Alchemical")]
    public class Alchemical : DeltaType { }

    [SourceInfo(@"Circumstance")]
    public class Circumstance<SubType> : DeltaType { }

    [SourceInfo(@"Competence")]
    public class Competence : DeltaType { }

    /// <summary>Same source used for critical range extenders to prevent stacking</summary>
    [SourceInfo(@"CriticalRange")]
    public class CriticalRange : DeltaType { }

    [SourceInfo(@"Deflection")]
    public class Deflection : DeltaType { }

    [SourceInfo(@"Enhancement")]
    public class Enhancement : DeltaType { }

    [SourceInfo(@"Insight")]
    public class Insight : DeltaType { }

    [SourceInfo(@"Luck")]
    public class Luck : DeltaType { }

    [SourceInfo(@"Morale")]
    public class Morale : DeltaType { }

    [SourceInfo(@"NaturalArmor")]
    public class NaturalArmor : DeltaType { }

    [SourceInfo(@"Profane")]
    public class Profane : DeltaType { }

    [SourceInfo(@"Resistance")]
    public class Resistance : DeltaType { }

    [SourceInfo(@"Sacred")]
    public class Sacred : DeltaType { }

    [SourceInfo(@"Size")]
    public class Size : DeltaType { }

    [SourceInfo(@"Inherent")]
    public class Inherent : DeltaType { }

    [SourceInfo(@"Racial")]
    public class Racial : DeltaType { }

    [SourceInfo(@"Drain")]
    public class Drain : DeltaType { }
}
