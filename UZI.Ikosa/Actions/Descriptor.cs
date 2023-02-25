using System;
using System.Security.Cryptography;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Abstract descriptor base for tags on spells</summary>
    [Serializable]
    public abstract class Descriptor
    {
        public string Name { get { return this.GetType().Name; } }
    }

    [Serializable]
    public abstract class EnergyDescriptor : Descriptor { }

    [Serializable]
    public abstract class AlignmentDescriptor : Descriptor { }

    /// <summary>Acid spell descriptor</summary>
    [Serializable]
    public class Acid : EnergyDescriptor { }

    /// <summary>Air spell descriptor</summary>
    [Serializable]
    public class Air : Descriptor { }

    /// <summary>Chaotic spell descriptor</summary>
    [Serializable]
    public class Chaotic : AlignmentDescriptor { }

    /// <summary>Cold spell descriptor</summary>
    [Serializable]
    public class Cold : EnergyDescriptor { }

    [Serializable]
    public class Curse : Descriptor { }

    /// <summary>Darkness spell descriptor</summary>
    [Serializable]
    public class Darkness : Descriptor { }

    /// <summary>Death spell descriptor</summary>
    [Serializable]
    public class Death : Descriptor { }

    /// <summary>Earth spell descriptor</summary>
    [Serializable]
    public class Earth : Descriptor { }

    /// <summary>Electricity spell descriptor</summary>
    [Serializable]
    public class Electricity : EnergyDescriptor { }

    /// <summary>Evil spell descriptor</summary>
    [Serializable]
    public class Evil : AlignmentDescriptor { }

    /// <summary>Fear spell descriptor</summary>
    [Serializable]
    public class Fear : Descriptor { }

    /// <summary>Fire spell descriptor</summary>
    [Serializable]
    public class Fire : EnergyDescriptor { }

    /// <summary>Force spell descriptor</summary>
    [Serializable]
    public class Force : EnergyDescriptor { }

    /// <summary>Good spell descriptor</summary>
    [Serializable]
    public class Good : AlignmentDescriptor { }

    /// <summary>LanguageDependent spell descriptor</summary>
    [Serializable]
    public class LanguageDependent : Descriptor { }

    /// <summary>Lawful spell descriptor</summary>
    [Serializable]
    public class Lawful : AlignmentDescriptor { }

    /// <summary>Light spell descriptor</summary>
    [Serializable]
    public class Light : Descriptor { }

    /// <summary>MindAffecting spell descriptor</summary>
    [Serializable]
    public class MindAffecting : Descriptor { }

    /// <summary>Sonic spell descriptor</summary>
    [Serializable]
    public class Sonic : EnergyDescriptor { }

    /// <summary>Uses sound, but not for damage</summary>
    [Serializable]
    public class SoundBased : Descriptor { }

    /// <summary>Water spell descriptor</summary>
    [Serializable]
    public class Water : Descriptor { }

}
