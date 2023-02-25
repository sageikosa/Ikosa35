using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterModeler.Generator
{
    public class TopRadiusProvider : ITopRadiusProvider
    {
        public TopRadiusProvider(ITopRadiusProvider source, double factor)
        {
            Source = source;
            Factor = factor;
        }

        public ITopRadiusProvider Source { get; set; }
        public double Factor { get; set; }

        #region ITopRadiusProvider Members

        public double Radius { get { return Source.Radius * Factor; } }

        #endregion

        #region ITopDimensionProvider Members

        public double Thickness { get { return Source.Thickness * Factor; } }

        public double Width { get { return Source.Width * Factor; } }

        #endregion
    }
}
