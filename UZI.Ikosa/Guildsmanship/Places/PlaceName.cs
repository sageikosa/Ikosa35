using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Languages;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class PlaceName
    {
        private NamingSchema _Schema;
        private Description _Description;

        public PlaceName(NamingSchema schema, Description description)
        {
            _Schema = schema;
            _Description = description;
        }

        public NamingSchema Schema => _Schema;
        public Description Description => _Description;
    }
}
