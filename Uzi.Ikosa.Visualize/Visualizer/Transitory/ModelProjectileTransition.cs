using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class ModelProjectileTransition : TransientVisualizer
    {
        public ModelProjectileTransition(Model3D model):
            base()
        {
            Model = model;
        }

        public Model3D Model { get; private set; }

        // TODO: rotation of model in "flight": flight-relative axis, direction, number of revolutions
        protected override System.Windows.Media.Animation.Timeline GetDirectAnimations(Visualization visualization)
        {
            throw new NotImplementedException();
        }


        public override Contracts.TransientVisualizerInfo ToInfo()
        {
            throw new NotImplementedException();
        }
    }
}
