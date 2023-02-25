using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class TeamAttitude
    {
        private string _Description;
        private Attitude _Attitude;
        private bool _Constituted;

        public string Description { get => _Description; set => _Description = value; }

        public Attitude Attitude { get => _Attitude; set => _Attitude = value; }

        /// <summary>True if there is a formal public declaration of the attitude (ie, declaration)</summary>
        public bool IsConstituted { get => _Constituted; set => _Constituted = value; }
    }
}
