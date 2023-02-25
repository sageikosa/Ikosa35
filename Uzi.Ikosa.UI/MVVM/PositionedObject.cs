using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.UI
{
    public struct PositionedObject
    {
        public ICoreObject CoreObject { get; set; }
        public double Distance { get; set; }
        public int ZOffset { get; set; }
        public int YOffset { get; set; }
        public int XOffset { get; set; }
    }
}
