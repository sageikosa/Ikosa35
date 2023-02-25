using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public class MorningStar : FragmentGenerator
    {
        public MorningStar()
        {
            Grip = new Grip(GetOrigin(),1);
            Haft = new Haft(Grip.ConnectionPoint(@"Top"), 2);
            Sphere = new Sphere(Haft.ConnectionPoint(@"Top"), 3);
            Spike = new Spike(Haft.ConnectionPoint(@"Top"), 0);
            this.Grip.Length = 0.5;
            this.Grip.Radius = 0.1;
            this.Haft.Length = 1;
            this.Haft.Radius = 0.08;
            this.Sphere.Radius = 0.25;
            this.Spike.Length = 0.5;
            this.Spike.Radius = 0.0833;
        }

        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public Sphere Sphere { get; set; }
        public Spike Spike { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            yield return Grip.GenerateElement(winfx, uzi);
            yield return Haft.GenerateElement(winfx, uzi);
            yield return Sphere.GenerateElement(winfx, uzi);

            // generate elements for spikes, but change vector each time
            var _mk = 3;
            for (int _x = -1; _x <= 1; _x++)
                for (int _y = -1; _y <= 1; _y++)
                    for (int _z = -1; _z <= 1; _z++)
                    {
                        if ((_x != 0) || (_y != 0) || (_z != 0))
                        {
                            _mk++;
                            Spike.MeshKey = _mk;
                            Spike.Direction = new Vector3D(_x, _y, _z);
                            yield return Spike.GenerateElement(winfx, uzi);
                        }
                    }
            yield break;
        }
    }
}
