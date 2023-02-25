using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Languages
{
    [Serializable]
    public class NamingSchema
    {
        private Language _Language;
        private string _Name;

        public NamingSchema(Language language, string name)
        {
            _Language = language;
            _Name = name;
        }

        public Language Language => _Language;
        public string Name => _Name;
    }
}
