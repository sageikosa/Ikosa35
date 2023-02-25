using System;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Universal
{
    public class DevotionalDefinition
    {
        public Alignment Alignment { get; set; }
        public Collection<TypeListItem> Influences { get; set; }
        public Type WeaponType { get; set; }
    }
}
