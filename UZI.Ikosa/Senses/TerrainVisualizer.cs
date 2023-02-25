using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Senses
{
    /// <summary>Class used to provide sense ability to terrain visual effect calculations</summary>
    public class TerrainVisualizer
    {
        internal TerrainVisualizer(SensorySet senses, LightRange hostLightLevel, PlanarPresence planar)
        {
            FilteredSenses = senses.BestTerrainSenses
                .Where(_s => (_s.PlanarPresence & planar) != PlanarPresence.None)
                .ToList();
            IgnoresVisualEffects = FilteredSenses.Where(_s => _s.IgnoresVisualEffects).ToList();
            UsesSight = FilteredSenses.Where(_s => _s.UsesSight).ToList();
            NotUsesSight = FilteredSenses.Where(_s => !_s.UsesSight).ToList();
            NoSight = UsesSight.Count == 0;
            IgnoreVisual = IgnoresVisualEffects.Count != 0;
            HostLightLevel = hostLightLevel;
            VisualPlanarPresence = FilteredSenses.Any() ? FilteredSenses.Max(_s => _s.PlanarPresence) : PlanarPresence.None;
        }

        public TerrainVisualizer(List<SensoryBase> senses, LightRange hostLightLevel)
        {
            FilteredSenses = senses;
            IgnoresVisualEffects = FilteredSenses.Where(_s => _s.IgnoresVisualEffects).ToList();
            UsesSight = FilteredSenses.Where(_s => _s.UsesSight).ToList();
            NotUsesSight = FilteredSenses.Where(_s => !_s.UsesSight).ToList();
            NoSight = UsesSight.Count == 0;
            IgnoreVisual = IgnoresVisualEffects.Count != 0;
            HostLightLevel = hostLightLevel;
            VisualPlanarPresence = PlanarPresence.Both;
        }

        public List<SensoryBase> FilteredSenses { get; private set; }
        public List<SensoryBase> IgnoresVisualEffects { get; private set; }
        public List<SensoryBase> UsesSight { get; private set; }
        public List<SensoryBase> NotUsesSight { get; private set; }
        public LightRange HostLightLevel { get; private set; }
        public bool NoSight { get; private set; }
        public bool IgnoreVisual { get; private set; }
        public PlanarPresence VisualPlanarPresence { get; set; }
    }
}
